using kate.shared.Helpers;
using Nini.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AOCNotify
{
    public static class Config
    {
        public static string ConfigFilename => "config.ini";
        public static string ConfigLocation
        => Path.Combine(
            Directory.GetCurrentDirectory(),
            ConfigFilename);
        public static IniConfigSource Source;
        private static Timer BusStationTimer;
        static Config()
        {
            if (!File.Exists(ConfigLocation))
                File.WriteAllText(ConfigLocation, "");

            ReadFrom(ConfigLocation);
        }
        internal static void ReadFrom(string location = null)
        {
            var resetEvent = new AutoResetEvent(false);
            if (BusStationTimer != null)
            {
                BusStationTimer = new Timer(delegate
                {
                    if (HasChanges)
                    {
                        Save();
                        HasChanges = false;
                    }
                    resetEvent.Set();
                }, resetEvent, 0, 1000);
            }
            Source = new IniConfigSource(location ?? ConfigLocation);
            MergeDefaultData();
            if (BusStationTimer != null)
                resetEvent.WaitOne(1);
        }

        private static void MergeDefaultData()
        {
            foreach (var groupPair in DefaultData)
            {
                foreach (var pair in groupPair.Value)
                {
                    if (!Get(groupPair.Key).Contains(pair.Key))
                    {
                        Set(groupPair.Key, pair.Key, pair.Value);
                    }
                }
            }
            Save();
        }

        public static Dictionary<string, Dictionary<string, object>> DefaultData = new Dictionary<string, Dictionary<string, object>>()
        {
            {"Discord", new Dictionary<string, object>()
                {
                    {"Enable", false },
                    {"Webhooks", "" }
                }
            },
            {"AOC", new Dictionary<string, object>()
                {
                    {"Leaderboards", "" },
                    {"Year", DateTimeOffset.UtcNow.Year },
                    {"Token", "" }
                }
            }
        };
        public static string[] DiscordWebhookArray
        {
            get
            {
                return GetString("Discord", "Webhooks", "").Split(' ');
            }
            set
            {
                Set("Discord", "Webhooks", string.Join(" ", value));
            }
        }
        public static string[] LeaderboardIdArray
        {
            get
            {
                return GetString("AOC", "Leaderboards", "").Split(' ');
            }
            set
            {
                Set("AOC", "Leaderboards", string.Join(" ", value));
            }
        }

        public delegate void ConfigSetDelegate(string group, string key, object value);
        public static ConfigSetDelegate OnWrite;
        public static VoidDelegate OnSave;

        public static void Save()
        {
            var startNS = GeneralHelper.GetNanoseconds();
            if (!File.Exists(ConfigLocation))
                File.WriteAllText(ConfigLocation, "");
            Source.Save();
            Console.WriteLine($"[ServerConfig] Saved {GeneralHelper.GetNanoseconds() - startNS}ns");
            HasChanges = false;
            OnSave?.Invoke();
        }

        public static Dictionary<string, Dictionary<string, object>> Get()
        {
            var dict = new Dictionary<string, Dictionary<string, object>>();
            foreach (IConfig cfg in Source.Configs)
            {
                if (cfg == null) continue;
                dict.Add(cfg.Name, new Dictionary<string, object>());
                foreach (var key in cfg.GetKeys())
                {
                    var value = cfg.Get(key);
                    dict[cfg.Name].Add(key, value);
                }
            }
            return dict;
        }
        public static void Set(Dictionary<string, Dictionary<string, object>> dict)
        {
            foreach (var group in dict)
            {
                foreach (var item in group.Value)
                {
                    Set(group.Key, item.Key, item.Value);
                }
            }
            HasChanges = true;
            Save();
        }

        public static IConfig Get(string group)
        {
            var cfg = Source.Configs[group];
            if (cfg == null)
                cfg = Source.Configs.Add(group);
            return cfg;
        }
        public static void Set(string group, string key, object value)
        {
            var cfg = Get(group);
            cfg.Set(key, value);
            HasChanges = true;
        }

        private static bool HasChanges = false;

        public static string Get(string group, string key) => Get(group).Get(key);
        public static string Get(string group, string key, string fallback) => Get(group).Get(key, fallback);

        public static string GetExpanded(string group, string key) => Get(group).GetExpanded(key);

        public static string GetString(string group, string key) => Get(group).GetString(key);
        public static string GetString(string group, string key, string fallback) => Get(group).GetString(key, fallback);

        public static int GetInt(string group, string key) => Get(group).GetInt(key);
        public static int GetInt(string group, string key, int fallback) => Get(group).GetInt(key, fallback);
        public static int GetInt(string group, string key, int fallback, bool fromAlias) => Get(group).GetInt(key, fallback, fromAlias);

        public static long GetLong(string group, string key) => Get(group).GetLong(key);
        public static long GetLong(string group, string key, long fallback) => Get(group).GetLong(key, fallback);

        public static bool GetBoolean(string group, string key) => Get(group).GetBoolean(key);
        public static bool GetBoolean(string group, string key, bool fallback) => Get(group).GetBoolean(key, fallback);

        public static float GetFloat(string group, string key) => Get(group).GetFloat(key);
        public static float GetFloat(string group, string key, float fallback) => Get(group).GetFloat(key, fallback);

        public static string[] GetKeys(string group) => Get(group).GetKeys();
        public static string[] GetValues(string group) => Get(group).GetValues();
        public static void Remove(string group, string key) => Get(group).Remove(key);
    }
}
