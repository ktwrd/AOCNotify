using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AOCNotify.Helpers;
using NLog;

namespace AOCNotify;

public class NotifyAction
{
    private readonly NotifyActionParams _params;
    private readonly Logger _log;

    public NotifyAction(NotifyActionParams @params)
    {
        _params = @params;
        _log = LogManager.GetCurrentClassLogger();
        _log.Properties["id"] = _params.GetStoreHash();
    }

    public async Task ExecuteAsync()
    {
        var client = new AdventClient(_params.Token);
        _log.Info($"Fetching previous leaderboard");
        var previous = GetPreviousResult();
        _log.Info($"Fetching current leaderboard");
        var current = await client.GetLeaderboardAsync(_params.LeaderboardId, _params.Year);

        _log.Info($"Building message content");
        var previousPrintStrings = Array.Empty<string>();
        if (previous != null)
        {
            previousPrintStrings = CalculatePrintStrings(previous);
        }
        var currentPrintStrings = CalculatePrintStrings(current);
        
        var messageLines = new List<string>();
        foreach (var i in previousPrintStrings.Concat(currentPrintStrings))
        {
            if (!messageLines.Contains(i))
            {
                messageLines.Add(i);
            }
        }
        
        var storeLocation = _params.GetStoreLocation();
        _log.Info($"Writing current leaderboard to {storeLocation}");
        var currentAsJson = JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
        await File.WriteAllTextAsync(storeLocation, currentAsJson);
        
        _log.Info($"Sending discord message(s) to {_params.DiscordWebhookUrl}");
        await DiscordHelper.SendMessage(
            _params.DiscordWebhookUrl,
            messageLines.ToArray(),
            _params.DiscordWebhookArgs);
        _log.Info("Done");
    }

    private string[] CalculatePrintStrings(LeaderboardResponse leaderboard)
    {
        var list = new List<KeyValuePair<long, string>>();
        foreach (var mpair in leaderboard.Members)
        {
            foreach (var dayPair in mpair.Value.CompletionLevel)
            {
                foreach (var completionPair in dayPair.Value)
                {
                    var endTimestamp = DateTimeOffset.FromUnixTimeSeconds(completionPair.Value.Timestamp);
                    var diff = endTimestamp - new DateTime(Config.GetInt("AOC", "Year", DateTimeOffset.Now.Year), 12, dayPair.Key, 5, 0, 0, DateTimeKind.Utc);
                    string content = string.Join(" ", new string[]
                    {
                        $"`{mpair.Value.Name}` solved",
                        $"day {dayPair.Key}",
                        $"part {completionPair.Key}",
                        $"({Math.Floor(diff.TotalHours)}:{(Math.Floor(diff.TotalMinutes) % 60).ToString().PadLeft(2, '0')}:{(Math.Floor(diff.TotalSeconds) % 60 % 60).ToString().PadLeft(2, '0')})"
                    });

                    list.Add(new KeyValuePair<long, string>(completionPair.Value.Timestamp, content));
                }
            }
        }
        return list.OrderBy(v => v.Key).Select(v => v.Value).ToArray();
    }
    private LeaderboardResponse? GetPreviousResult()
    {
        var location = _params.GetStoreLocation();
        if (!File.Exists(location))
            return null;
        var content = File.ReadAllText(location);
        try
        {
            var data = JsonSerializer.Deserialize<LeaderboardResponse>(content, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });
            if (data == null)
                throw new InvalidOperationException($"Content parsed to null\n{content}");
            return data;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to read previous result\n{ex}");
            return null;
        }
    }
}