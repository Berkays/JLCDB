using System.Text.RegularExpressions;

using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NumericFilterFunc = System.Func<MongoDB.Driver.FilterDefinitionBuilder<MongoDB.Bson.BsonDocument>, System.Func<MongoDB.Driver.FieldDefinition<MongoDB.Bson.BsonDocument, double>, double, MongoDB.Driver.FilterDefinition<MongoDB.Bson.BsonDocument>>>;

using JLCDB.Utility;

namespace JLCDB.Query;

public static class Engine
{
    private static readonly string[] numeric_operators = new[]
    {
        "<",
        "<=",
        "=",
        ">=",
        ">",
    };
    private static readonly NumericFilterFunc[] numeric_operator_ops = new NumericFilterFunc[]
    {
        (builder) => builder.Lt,
        (builder) => builder.Lte,
        (builder) => builder.Eq,
        (builder) => builder.Gte,
        (builder) => builder.Gt,
    };

    public static FilterDefinition<BsonDocument> Evaluate(FilterDefinitionBuilder<BsonDocument> builder, Dictionary<string, string> clientMap, string query)
    {
        string pattern = @"([A-Z]\d+)\s*(<|>|<=|>=|=)\s*[+-]?([0-9]*\.?[0-9]?)?([k|m|u|n|p]?[a-zA-Z]|%)|[+-]?([0-9]*\.?[0-9]?)?([k|m|u|n|p]?[a-zA-Z]|%)\s*(<|>|<=|>=|=)\s*([A-Z]\d+)";
        RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;

        Regex regex = new(pattern, options);

        List<FilterDefinition<BsonDocument>> filters = new();
        foreach (Match match in regex.Matches(query.Replace(" ", "")).AsEnumerable<Match>())
        {
            // Option-1: field operator value unit
            // Option-2: value unit operator  (Reverse operator)
            string @operator = String.Empty;
            string field = String.Empty;
            string value = String.Empty;
            string unit = String.Empty;

            var groups = match.Groups.Values.Where(n => n.Success == true).Skip(1).ToArray();
            string value1 = groups[0].Value.Trim();
            string value2 = groups[1].Value.Trim();
            string value3 = groups[2].Value.Trim();
            string value4 = groups[3].Value.Trim();

            if (clientMap.ContainsKey(value1))
            {
                // Option-1
                field = value1;
                @operator = value2;
                value = value3;
                unit = value4;
            }
            else if (clientMap.ContainsKey(value4))
            {
                // Option-2
                field = value4;
                @operator = value3;
                value = value1;
                unit = value2;
            }
            else
            {
                throw new Exception("Invalid Query");
            }

            int operatorIndex = Array.IndexOf<string>(numeric_operators, @operator);
            if (operatorIndex > -1)
            {
                // Reverse operator
                if (field == value4)
                    operatorIndex = numeric_operators.Length - operatorIndex - 1;

                var filterFunc = numeric_operator_ops[operatorIndex](builder);

                double numeric_value = Convert.ToDouble(value);

                // Percentage doesn't have unit
                if (unit != "%")
                    numeric_value = MetricConverter.Normalize(unit, unit[^1].ToString(), numeric_value);

                string property = clientMap[field];
                filters.Add(filterFunc($"details.{property}", numeric_value));

                continue;
            }

            throw new NotImplementedException($"Operator {@operator} not implemented");
        }

        var k = builder.And(filters) & builder.And(EvaluateBaseProperties(builder, query));
        var documentSerializer = BsonSerializer.SerializerRegistry.GetSerializer<BsonDocument>();
        var renderedResultFilter = k.Render(documentSerializer, BsonSerializer.SerializerRegistry);
        Console.WriteLine(renderedResultFilter);
        return k;
    }

    public static List<FilterDefinition<BsonDocument>> EvaluateBaseProperties(FilterDefinitionBuilder<BsonDocument> builder, string query)
    {
        string pattern = @"(price)\s*(<=|>=|=|<|>)\s*([0-9]*\.?[0-9]*)|(Package)\s*(=)\s*(\S*)\s*|(LibraryType)\s*(=)\s*(Basic|Extended)";

        RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;
        Regex regex = new(pattern, options);

        List<FilterDefinition<BsonDocument>> filters = new();

        foreach (Match match in regex.Matches(query.Replace(" ", "")).AsEnumerable<Match>())
        {
            string @operator = String.Empty;
            string field = String.Empty;
            string value = String.Empty;

            var groups = match.Groups.Values.Where(n => n.Success == true).Skip(1).ToArray();
            field = groups[0].Value.Trim();
            @operator = groups[1].Value.Trim();
            value = groups[2].Value.Trim();

            if (String.Compare(field, "price", true) == 0)
            {
                int operatorIndex = Array.IndexOf<string>(numeric_operators, @operator);
                if (operatorIndex > -1)
                {
                    var filterFunc = numeric_operator_ops[operatorIndex](builder);
                    double numeric_value = Convert.ToDouble(value);

                    filters.Add(filterFunc("Price", numeric_value));
                    continue;
                }

                throw new InvalidDataException($"Invalid operator for price property");
            }
            else if (String.Compare(field, "LibraryType", true) == 0)
            {
                if (String.Compare(value, "Basic", true) != 0 && String.Compare(value, "Extended", true) != 0)
                    throw new InvalidDataException($"Invalid value {value} for LibraryType property");

                filters.Add(builder.Eq("LibraryType", value));
                continue;
            }
            else if (String.Compare(field, "Package", true) == 0)
            {
                filters.Add(builder.Eq("Package", value));
                continue;
            }

            throw new NotImplementedException($"Property {field} not implemented");
        }

        return filters;
    }
}

// Base property parser

// Package (Full Match), LibraryType (Enum Match), 


// Price(Numeric)
