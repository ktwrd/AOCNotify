/*
 *   Copyright 2022-2025 Kate Ward <kate@dariox.club>
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AOCNotify;

public class LeaderboardResponse
{
    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }
    [JsonPropertyName("members")]
    public Dictionary<int, LeaderboardMember> Members { get; set; } = [];
    [JsonPropertyName("event")]
    public string Event { get; set; } = "";
}
public class LeaderboardMember
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("last_star_ts")]
    public long LastStarTimestamp { get; set; }
    [JsonPropertyName("completion_day_level")]
    public Dictionary<int, Dictionary<int, LeaderboardCompletion>> CompletionLevel { get; set; } = [];
    [JsonPropertyName("local_score")]
    public int LocalScore { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
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
