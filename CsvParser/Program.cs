using MongoDB.Driver;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Configuration;

namespace JLCDB.Parser
{
    public class CSVParser
    {
        public static async Task<int> Main()
        {
            string connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString", EnvironmentVariableTarget.Process);
            string databaseName = Environment.GetEnvironmentVariable("Database__Name", EnvironmentVariableTarget.Process);

            if (String.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Database__ConnectionString is not defined");
                return 1;
            }

            if (String.IsNullOrEmpty(databaseName))
            {
                Console.WriteLine("Database__Name is not defined");
                return 1;
            }

            bool useLocalDownload = false;
            try
            {
                Boolean.TryParse(Environment.GetEnvironmentVariable("UseLocalDownload", EnvironmentVariableTarget.Process), out useLocalDownload);
            }
            catch { }

            string filePath;
            if (useLocalDownload == true)
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "input.csv");
            }
            else
            {
                string remoteFilename = await Fetcher.GetRemoteFilename();

                if (remoteFilename == "asd")
                {
                    Console.WriteLine("Database is already up to date");
                    return 0;
                }

                Console.WriteLine("Downloading...");
                filePath = await Fetcher.Download();
                Console.WriteLine("Fetched JLCPCB data");
            }



            var mongoConfiguration = MongoClientSettings.FromConnectionString(connectionString);

            if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString() == "Arm64")
            {
                mongoConfiguration.Compressors = new List<CompressorConfiguration>() {
                    new CompressorConfiguration(CompressorType.Zlib)
                };
            }
            else
            {
                mongoConfiguration.Compressors = new List<CompressorConfiguration>() {
                    new CompressorConfiguration(CompressorType.ZStandard),
                    new CompressorConfiguration(CompressorType.Snappy),
                };
            }

            MongoClient dbClient = new(mongoConfiguration);
            dbClient.GetDatabase(databaseName);
            Console.WriteLine("Connected to DB");
            CsvParser.Run(filePath, dbClient);

            return 0;
        }
    }
}
