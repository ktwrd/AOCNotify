using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using AOCNotify.Helpers;

namespace AOCNotify;

public class NotifyActionParams
{
    public NotifyActionParams()
    {
        DiscordWebhookUrl = "";
        DiscordWebhookArgs = new();
        Year = DateTimeOffset.UtcNow.Year;
        Token = "";
        LeaderboardId = "";
        StoreDirectory = "./store";

    }

    public NotifyActionParams(DiscordWebhookItem webhook, AdventConfig rootConfig)
        : this(rootConfig)
    {
        DiscordWebhookUrl = webhook.Url;
        ApplyConfigOverride(webhook);
    }

    public NotifyActionParams(AdventConfig rootConfig)
        : this()
    {
        Year = rootConfig.Year;
        Token = rootConfig.Token;
        LeaderboardId = rootConfig.LeaderboardId;
        StoreDirectory = rootConfig.StoreDirectory;
    }

    public NotifyActionParams(string url, DiscordConfigItem discord, AdventConfig rootConfig)
        : this(rootConfig)
    {
        DiscordWebhookUrl = url;
        Year = rootConfig.Year;
        Token = rootConfig.Token;
        LeaderboardId = rootConfig.LeaderboardId;
        ApplyConfigOverride(discord);
    }
    public NotifyActionParams(DiscordWebhookItem webhookUrl, DiscordConfigItem discord, AdventConfig rootConfig)
        : this(rootConfig)
    {
        DiscordWebhookUrl = webhookUrl.Url;
        Year = rootConfig.Year;
        Token = rootConfig.Token;
        LeaderboardId = rootConfig.LeaderboardId;
        ApplyConfigOverride(discord);
        ApplyConfigOverride(webhookUrl);
    }
    private void ApplyConfigOverride(IConfigOverride ride)
    {
        if (!string.IsNullOrEmpty(ride.LeaderboardId))
        {
            LeaderboardId = ride.LeaderboardId;
        }
        if (ride.Year != null)
        {
            Year = (int)ride.Year;
        }
        if (!string.IsNullOrEmpty(ride.Username))
        {
            DiscordWebhookArgs.Username = ride.Username;
        }

        if (!string.IsNullOrEmpty(ride.AvatarUrl))
        {
            DiscordWebhookArgs.AvatarUrl = ride.AvatarUrl;
        }
    }
    
    [Required]
    public string DiscordWebhookUrl { get; set; }
    public DiscordWebhookEventArgs DiscordWebhookArgs { get; set; }
    public int Year { get; set; }
    public string Token { get; set; }
    public string LeaderboardId { get; set; }
    public string StoreDirectory { get; set; }

    public string GetStoreLocation()
    {
        var hash = GetStoreHash();
        var fn = $"leaderboard-{hash}.json";
        if (!Directory.Exists(StoreDirectory))
            Directory.CreateDirectory(StoreDirectory);
        return Path.Join(StoreDirectory, fn);
    }

    public string GetStoreHash()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Year.ToString());
        sb.Append(Token);
        sb.Append(LeaderboardId);
        sb.Append(DiscordWebhookUrl);
        return HashHelper.GetSha1Hash(sb.ToString());
    }
}

public class DiscordWebhookEventArgs
{
    [JsonPropertyName("username")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Username { get; set; }

    [JsonPropertyName("avatar_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Content { get; set; } = "";
}