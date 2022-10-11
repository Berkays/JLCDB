using NUnit.Framework;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using JLCDB.Utility;
using JLCDB.Query;

namespace JLCDB.Query.Test;

[TestFixture]
public class EngineTest
{
    [Test]
    public void TestSimpleQuery()
    {
        string query = "100uF < C1 and V1 >= 16V and 200UF > C1";

        Dictionary<string, string> clientMap = new()
        {
            {"C1", "Capacitance"},
            {"V1", "Voltage Rating"}
        };

        var filter = Builders<BsonDocument>.Filter;

        var result = Engine.Evaluate(filter, clientMap, query);
        var actual = filter.And(
            filter.Gt("details.Capacitance", MetricConverter.Normalize("uF", "F", 100)),
            filter.Lt("details.Capacitance", MetricConverter.Normalize("UF", "F", 200)),
            filter.Gte("details.Voltage Rating", MetricConverter.Normalize("V", "V", 16))
            );

        var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>();
        var renderedResultFilter = result.Render(documentSerializer, BsonSerializer.SerializerRegistry);
        var renderedActualFilter = actual.Render(documentSerializer, BsonSerializer.SerializerRegistry);

        Assert.IsTrue(renderedActualFilter == renderedResultFilter);
    }

    [Test]
    public void TestSimpleQueryWithTolerance()
    {
        string query = "T1 <= 20% and C1 > 0.1nF";

        Dictionary<string, string> clientMap = new()
        {
            {"C1", "Capacitance"},
            {"T1", "Tolerance"}
        };

        var filter = Builders<BsonDocument>.Filter;

        var result = Engine.Evaluate(filter, clientMap, query);
        var actual = filter.And(
            filter.Lte("details.Tolerance", 20),
            filter.Gt("details.Capacitance", MetricConverter.Normalize("nF", "F", 0.1))
            );

        var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>();
        var renderedResultFilter = result.Render(documentSerializer, BsonSerializer.SerializerRegistry);
        var renderedActualFilter = actual.Render(documentSerializer, BsonSerializer.SerializerRegistry);

        Assert.IsTrue(renderedActualFilter == renderedResultFilter);
    }

    [Test]
    public void TestBaseProperties()
    {
        string query = "LibraryType = Basic";

        Dictionary<string, string> clientMap = new()
        {
            {"C1", "Capacitance"},
            {"V1", "Voltage Rating"}
        };

        var filter = Builders<BsonDocument>.Filter;

        var result = Engine.Evaluate(filter, clientMap, query);
        var actual = filter.Eq("LibraryType", "Basic");

        var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>();
        var renderedResultFilter = result.Render(documentSerializer, BsonSerializer.SerializerRegistry);
        var renderedActualFilter = actual.Render(documentSerializer, BsonSerializer.SerializerRegistry);

        Assert.IsTrue(renderedActualFilter == renderedResultFilter);
    }
}