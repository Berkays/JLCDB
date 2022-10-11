using Microsoft.Extensions.Options;

using JLCDB.API.Models;
using JLCDB.Query;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace JLCDB.API.Services;

public class ComponentsService
{
    private readonly IMongoDatabase database;

    private List<string>? mainCategoriesCache;
    private readonly List<string> basePropertiesCache = new()
    {
        "Package",
        "LibraryType",
        "Price"
    };
    private readonly Dictionary<string, List<string>> extraPropertiesCache;

    public ComponentsService(IOptions<DatabaseStoreSettings> DatabaseSettings)
    {
        var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

        database = mongoClient.GetDatabase(DatabaseSettings.Value.Name);

        extraPropertiesCache = new Dictionary<string, List<string>>();
    }

    public async Task<List<string>> GetMainCategoriesAsync()
    {
        if (mainCategoriesCache == null)
        {
            var cursor = await database.ListCollectionNamesAsync();

            mainCategoriesCache = await cursor.ToListAsync();
            return mainCategoriesCache;
        }

        return mainCategoriesCache;
    }

    public List<string> GetBaseProperties()
    {
        return basePropertiesCache;
        // if (basePropertiesCache != null)
        //     return basePropertiesCache;
        // var collection = database.GetCollection<BaseComponent>("Capacitors");

        // var cursor = await collection.FindAsync(new BsonDocument());
        // var document = await cursor.FirstOrDefaultAsync();

        // var properties = document.GetType().GetProperties().Select(n => n.Name).ToList();

        // this.basePropertiesCache = properties;
        // return properties;
    }

    public async Task<List<string>> GetExtraPropertiesAsync(string mainCategory)
    {
        if (extraPropertiesCache.ContainsKey(mainCategory))
            return extraPropertiesCache[mainCategory];

        if (mainCategoriesCache?.Contains(mainCategory) == false)
            return new List<string>(0);

        var collection = database.GetCollection<BsonDocument>(mainCategory);

        var projection = Builders<BsonDocument>.Projection.Include("details");

        var document = await collection.Find(new BsonDocument()).Project(projection).FirstOrDefaultAsync();

        if (document.ElementCount < 2)
            return new List<string>();

        var details = document.GetElement(1).Value.AsBsonDocument;
        var names = details.Names.ToList();

        extraPropertiesCache.Add(mainCategory, names);

        return names;
    }

    public async Task<string?> GetResultsAsync(string mainCategory, Dictionary<string, string> clientMap, string query)
    {
        if (mainCategoriesCache?.Contains(mainCategory) == false)
            return null;

        var collection = database.GetCollection<BsonDocument>(mainCategory);

        var builder = Builders<BsonDocument>.Filter;
        var filter = Query.Engine.Evaluate(builder, clientMap, query);

        var documentSerializer = MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>();
        var renderedResultFilter = filter.Render(documentSerializer, MongoDB.Bson.Serialization.BsonSerializer.SerializerRegistry);
        var document = await collection.Find(filter).Limit(1000).ToListAsync();

        if (document != null)
            return document.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson });

        return null;
    }
}