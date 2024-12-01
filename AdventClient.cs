using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AOCNotify;

public class AdventClient
{
    public string Token { get; private set; }

    public AdventClient(string token)
    {
        Token = token;
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("Cookie", $"session={Token}");
    }

    private readonly HttpClient _httpClient;

    private void EnsureRequest(HttpRequestMessage req)
    {
        req.Headers.Remove("Cookie");
        req.Headers.Add("Cookie", $"session={Token}");
    }

    public Task<LeaderboardResponse> GetLeaderboardAsync(string leaderboardId)
    {
        return GetLeaderboardAsync(leaderboardId, DateTimeOffset.UtcNow.Year);
    }

    public async Task<LeaderboardResponse> GetLeaderboardAsync(string leaderboardId, int year)
    {
        var url = $"https://adventofcode.com/{year}/leaderboard/private/view/{leaderboardId}.json";

        var req = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };
        EnsureRequest(req);

        var res = await _httpClient.SendAsync(req);
        var stringContent = res.Content.ReadAsStringAsync().Result;
        if (!res.IsSuccessStatusCode)
        {
            throw new ApplicationException(
                $"Failed to send request to URL {url}\nStatus Code: {res.StatusCode}\nContent\n{stringContent}");
        }
        
        var data = JsonSerializer.Deserialize<LeaderboardResponse>(stringContent, new JsonSerializerOptions
        {
            IncludeFields = true,
        });
        if (data == null)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize data from URL {url}\nStatus Code: {res.StatusCode}\nContent\n{stringContent}");
        }

        return data;
    }
}