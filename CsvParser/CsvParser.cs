global using System.Text.RegularExpressions;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;

using MongoDB.Driver;
using MongoDB.Bson;

namespace JLCDB.Parser;

internal sealed class CsvParser
{
    const int BATCH_SIZE = 150000;
    const string outFile = "encoded.csv";
    static readonly string outFilePath = Path.Combine(Path.GetTempPath(), outFile);

    private static void Encode(string inputFilePath)
    {
        Stopwatch sw = new();
        sw.Start();

        Console.WriteLine("Encoding with iconv...");
        var encodingProcess = new Process();
        encodingProcess.StartInfo.FileName = "/bin/bash";
        encodingProcess.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        encodingProcess.StartInfo.Arguments = $"-c \"iconv -f GBK -t UTF-8 {inputFilePath} > {outFilePath}\"";
        encodingProcess.StartInfo.UseShellExecute = false;
        encodingProcess.StartInfo.RedirectStandardOutput = true;
        encodingProcess.StartInfo.CreateNoWindow = true;
        encodingProcess.Start();
        encodingProcess.BeginOutputReadLine();
        encodingProcess.WaitForExit();

        if (encodingProcess.ExitCode == 127)
        {
            Console.WriteLine("iconv not found");
            throw new Exception("iconv not found");
        }

        if (encodingProcess.ExitCode == 1)
        {
            Console.WriteLine("Error during encoding");
            try
            {
                // File.Delete(inputFilePath);
                File.Delete(outFilePath);
            }
            catch (Exception)
            {
            }
            throw new Exception("Error during encoding");
        }

        sw.Stop();

        File.Delete(inputFilePath);

        Console.WriteLine($"Conversion completed ({sw.Elapsed})");
    }

    public static void Run(string inputFilePath, MongoClient dbClient)
    {
        Encode(inputFilePath);

        var database = dbClient.GetDatabase("data");
        var collections = database.ListCollectionNames().ToList();

        // Purge collections
        foreach (var collection in collections.ToList())
            database.DropCollection(collection);

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null
        };

        Stopwatch sw = new();
        sw.Start();

        var categoryParsers = CreateParsers();

        InsertManyOptions insertManyOptions = new() { IsOrdered = false };
        Dictionary<string, List<BsonDocument>> documentQueue = new();
        int count = 0;


        using var reader = new StreamReader(outFilePath, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        while (csv.Read())
        {
            string lcsc = csv.GetField(0);

            if (lcsc == null)
                continue;

            string firstCategory = csv.GetField(1);
            string secondCategory = csv.GetField(2);
            string package = csv.GetField(4);
            string manufacturer = csv.GetField(6);
            string libraryType = csv.GetField(7);
            string description = csv.GetField(8);
            string datasheet = csv.GetField(9);
            string priceRow = csv.GetField(10).Trim();
            uint stock = csv.GetField<uint>(11);

            if (string.IsNullOrEmpty(priceRow))
                continue;

            int idx1 = priceRow.IndexOf(':');
            int idx2 = priceRow.IndexOf(',');
            string priceString = priceRow.Substring(idx1 + 1, idx2 - idx1 - 1);
            if (!double.TryParse(priceString, out double price))
                continue;

            var component = new Component(
                lcsc, firstCategory, secondCategory, package, manufacturer, libraryType, description, datasheet, price, stock
            );

            bool hasParserDefinition = categoryParsers.TryGetValue(firstCategory, out CategoryParser categoryParser);

            var document = component.ToBsonDocument();
            if (hasParserDefinition == true)
            {
                var componentDetails = categoryParser.Run(description);
                document.AddRange(componentDetails);
            }

            if (documentQueue.ContainsKey(firstCategory) == false)
                documentQueue.Add(firstCategory, new List<BsonDocument>());

            documentQueue[firstCategory].Add(document);

            count++;

            // Free some memory
            if (count % BATCH_SIZE == 0)
            {
                Console.WriteLine($"Inserting batch ({count / BATCH_SIZE})...");
                foreach (var (collection, documents) in documentQueue)
                {
                    var coll = database.GetCollection<BsonDocument>(collection);
                    coll.InsertMany(documents, insertManyOptions);

                    documents.Clear();
                    documentQueue.Remove(collection);
                }
            }
        }

        Console.WriteLine("Inserting remaining batch...");
        foreach (var (collection, documents) in documentQueue)
        {
            var coll = database.GetCollection<BsonDocument>(collection);
            coll.InsertMany(documents, insertManyOptions);

            documents.Clear();
            documentQueue.Remove(collection);
        }

        sw.Stop();
        Console.WriteLine($"{count} components parsed. Took {sw.Elapsed}");
        File.Delete(outFilePath);
    }

    private static Dictionary<string, CategoryParser> CreateParsers()
    {
        return new Dictionary<string, CategoryParser>() {
                {"Capacitors", CapacitorParser.Create()},
                {"Resistors", ResistorParser.Create()},
                {"Inductors", InductorParser.Create()}
            };
    }
}