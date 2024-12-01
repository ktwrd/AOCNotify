using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using NLog;

namespace AOCNotify;

public class Program
{
    public static void Main(string[] args)
    {
        var xmlLocation = Environment.GetEnvironmentVariable("AOC_CONFIG");
        if (string.IsNullOrEmpty(xmlLocation))
        {
            Console.Error.WriteLine("Missing required environment variable AOC_CONFIG");
            Environment.Exit(1);
            return;
        }

        if (!File.Exists(xmlLocation))
        {
            Console.Error.WriteLine($"File location specified in environment variable AOC_CONFIG ({xmlLocation}) does not exist.");
            Environment.Exit(1);
            return;
        }
        
        var log = LogManager.GetCurrentClassLogger();
        log.Info($"Reading config from {xmlLocation}");

        var config = GetConfig(xmlLocation);
        log.Info($"Processing {config.DiscordConfigItems.Count} endpoints.");
        foreach (var item in config.DiscordConfigItems)
        {
            foreach (var url in item.Webhooks)
            {
                var paramItem = new NotifyActionParams(url, item, config);
                log.Info($"Processing {paramItem.GetStoreHash()}");
                log.Info(JsonSerializer.Serialize(paramItem, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    IncludeFields = true,
                }));
                var action = new NotifyAction(paramItem);
                action.ExecuteAsync().Wait();
            }
        }
        log.Info("Done");
    }

    private static AdventConfig GetConfig(string location)
    {
        using (var fs = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return DeserializeXml<AdventConfig>(fs);
        }
    }
    private static T DeserializeXml<T>(Stream inputStream)
        where T : class, new()
    {
        using (XmlReader reader = XmlReader.Create(inputStream, new (){ IgnoreWhitespace = true }))
        {
            var ser = new XmlSerializer(typeof(T));
            return (ser.Deserialize(reader) as T) ?? new();
        }
    }
    private static T DeserializeXml<T>(string content)
        where T : class, new()
    {
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        try
        {
            var res = DeserializeXml<T>(inputStream);
            inputStream.Dispose();
            return res;
        }
        catch
        {
            inputStream.Dispose();
            throw;
        }
    }
}