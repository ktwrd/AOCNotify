using kate.shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AOCNotify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.Get();
            Config.Save();
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("cookie", "session=" + Config.GetString("AOC", "Token", ""));
            var taskList = new List<Task>();
            foreach (var id in Config.LeaderboardIdArray)
            {
                foreach (var wb in Config.DiscordWebhookArray)
                {
                    taskList.Add(new Task(delegate
                    {
                        ProcessLeaderboard(id, wb).Wait();
                    }));
                }
            }
            foreach (var i in taskList)
                i.Start();
            Task.WhenAll(taskList).Wait();
        }
        public static HttpClient HttpClient = new HttpClient();
        public static async Task ProcessLeaderboard(string id, string webhook)
        {
            var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int year = Config.GetInt("AOC", "Year", DateTime.Now.Year);
            var url = $"https://adventofcode.com/{year}/leaderboard/private/view/{id}.json";
            var res = await HttpClient.GetAsync(url);
            Console.WriteLine($"GET: {url}");
            var stringcontent = res.Content.ReadAsStringAsync().Result;
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                LeaderboardResponse deser = JsonSerializer.Deserialize<LeaderboardResponse>(stringcontent, serializerOptions);
                if (deser == null)
                {
                    Console.WriteLine($"[ProcessLeaderboard:{id}] Failed to deserialize\n================\n{stringcontent}\n================");
                    return;
                }
                string filename = Path.Combine(Directory.GetCurrentDirectory(), $"cached-{id}-{MD5Hash(webhook.Trim())}.json");
                LeaderboardResponse? previous = null;
                if (File.Exists(filename))
                    previous = JsonSerializer.Deserialize<LeaderboardResponse>(File.ReadAllText(filename), serializerOptions);
                string[] previousPrintStrings = previous == null ? Array.Empty<string>() : GetUserStrings(previous);
                string[] currentPrintStrings = GetUserStrings(deser);

                string[] targetPrintStrings = currentPrintStrings.Concat(previousPrintStrings).ToArray();
                targetPrintStrings = targetPrintStrings.Where(v => targetPrintStrings.Where(t => t == v).Count() < 2).ToArray();
                File.WriteAllText(filename, JsonSerializer.Serialize(deser, serializerOptions));

                SendMessage(targetPrintStrings, webhook);
                Console.WriteLine($"[ProcessLeaderboard:{id}] {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start}ms");
            }
            else
            {
                Console.WriteLine($"[ProcessLeaderboard:{id}] Failed with code {(int)res.StatusCode}\n================\n{stringcontent}\n================");
            }
        }
        public static void SendMessage(string[] lines, string url)
        {
            var submessage = new List<string>();
            string working = "";
            foreach (var thing in lines)
            {
                if (working.Length + thing.Length > 2000)
                {
                    submessage.Add(working);
                    working = "";
                }
                working += $"{thing}\n";
            }
            if (working.Length > 0)
                submessage.Add(working);
            var submessageArr = submessage.ToArray();
            for (int i = 0; i < submessageArr.Length; i++)
            {
                string cnt = submessageArr[i];
                var dict = new Dictionary<string, object>()
                {
                    {"content", cnt },
                    {"avatar_url", "https://cdn.discordapp.com/avatars/1048078704485609523/2095d8f4face4397289f29954d61d777.png" },
                    {"username", "Advent of Code" }
                };
                HttpClient.PostAsJsonAsync(url, dict, serializerOptions).Wait();
                SleepFor(500).Wait();
            }
        }
        public static Task SleepFor(int duration)
        {
            System.Threading.Thread.Sleep(duration);
            return Task.CompletedTask;
        }
        public static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            IncludeFields = true,
            WriteIndented = true
        };
        public static string[] GetUserStrings(LeaderboardResponse leaderboard)
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

        public static string MD5Hash(string content)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(content);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }
    }
}