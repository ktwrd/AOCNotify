using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AOCNotify
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.Get();
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("session", Config.GetString("AOC", "Token", ""));
            var taskList = new List<Task>();
            foreach (var id in Config.LeaderboardIdArray)
            {
                taskList.Add(new Task(delegate
                {
                    ProcessLeaderboard(id).Wait();
                }));
            }
            foreach (var i in taskList)
                i.Start();
            Task.WhenAll(taskList).Wait();
        }
        public static HttpClient HttpClient = new HttpClient();
        public static async Task ProcessLeaderboard(string id)
        {
            int year = Config.GetInt("AOC", "Year", DateTime.Now.Year);
            var res = await HttpClient.GetAsync($"https://adventofcode.com/{year}/leaderboard/private/view/{id}.json");
            var stringcontent = res.Content.ReadAsStringAsync().Result;
        }
    }
}