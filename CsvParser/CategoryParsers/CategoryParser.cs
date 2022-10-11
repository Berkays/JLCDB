using MongoDB.Bson;

namespace JLCDB.Parser;

internal class CategoryParser
{
    private Dictionary<string, Func<string, double?>> processor { get; set; }

    public CategoryParser()
    {
        this.processor = new Dictionary<string, Func<string, double?>>();
    }

    public static CategoryParser Build()
    {
        return new CategoryParser();
    }

    public CategoryParser Add(string field, ValueParser parser)
    {
        this.processor.Add(field, desc => parser.ParseSingle(desc));
        return this;
    }

    public BsonDocument Run(string description)
    {
        var bsonDocument = new BsonDocument();
        foreach (var (k, v) in this.processor)
        {
            var value = v(description);
            if (value == null)
                continue;
            bsonDocument.AddRange(new BsonDocument(k, value));
        }

        return new BsonDocument()
        {
            { "details", bsonDocument}
        };
    }
}