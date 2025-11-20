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

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LeaderboardItem = AOCNotify.AppConfig.LeaderboardItem;

namespace AOCNotify;

public class AdventClient(HttpClient client, JsonSerializerOptions serializerOptions)
{
    public async Task<LeaderboardResponse> GetAsync(LeaderboardItem config)
    {
        var url = $"https://adventofcode.com/{config.Year}/leaderboard/private/view/{config.LeaderboardId}.json";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Failed to parse Url \"{url}\" for Leaderboard Id {config.LeaderboardId}");
        }
        var request = new HttpRequestMessage
        {
            RequestUri = uri,
            Method = HttpMethod.Get
        };
        request.Headers.Add("Cookie", "session=" + config.Token);

        var response = await client.SendAsync(request);
        var responseContent = response.Content.ReadAsStringAsync().Result;
        if (response.IsSuccessStatusCode)
        {
            var data = JsonSerializer.Deserialize<LeaderboardResponse>(responseContent, serializerOptions);
            if (data == null)
            {
                throw new InvalidOperationException($"Data deserialized to null when sending request to {request.RequestUri} (response: {(int)response.StatusCode}) of response content: {responseContent}");
            }
            return data;
        }
        else
        {
            throw new InvalidOperationException($"{request.Method} {(int)response.StatusCode} {request.RequestUri}\n{responseContent}");
        }
    }
}
