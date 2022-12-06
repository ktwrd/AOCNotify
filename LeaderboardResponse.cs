using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AOCNotify
{
    public class LeaderboardResponse
    {
        [JsonPropertyName("owner_id")]
        public int OwnerId { get; set; }
        [JsonPropertyName("members")]
        public Dictionary<int, LeaderboardMember> Members { get; set; }
        [JsonPropertyName("event")]
        public string Event { get; set; }
    }
    public class LeaderboardMember
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("last_star_ts")]
        public long LastStarTimestamp { get; set; }
        [JsonPropertyName("completion_day_level")]
        public Dictionary<int, Dictionary<int, LeaderboardCompletion>> CompletionLevel { get; set; }
        [JsonPropertyName("local_score")]
        public int LocalScore { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("global_score")]
        public int GlobalScore { get; set; }
        [JsonPropertyName("stars")]
        public int Stars { get; set; }
    }
    public class LeaderboardCompletion
    {
        [JsonPropertyName("get_star_ts")]
        public long Timestamp { get; set; }
        [JsonPropertyName("star_index")]
        public long Index { get; set; }
    }
}
