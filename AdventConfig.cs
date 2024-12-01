using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace AOCNotify;

[XmlRoot("AdventConfig")]
public class AdventConfig
{
    public AdventConfig()
    {
        LeaderboardId = "";
        Token = "";
        DiscordConfigItems = [];
        Year = DateTimeOffset.UtcNow.Year;
    }
    
    [Required]
    [XmlElement("LeaderboardId")]
    public string LeaderboardId { get; set; }
    
    [Required]
    [XmlElement("Token")]
    public string Token { get; set; }
    
    [Required]
    [XmlElement("Year")]
    public int Year { get; set; }
    
    [Required]
    [XmlElement("StoreDirectory")]
    public string StoreDirectory { get; set; }

    [Required]
    [XmlElement("Discord")]
    public List<DiscordConfigItem> DiscordConfigItems { get; set; }
}

public class DiscordConfigItem : IConfigOverride
{
    [XmlElement("Webhook")] public List<DiscordWebhookItem> Webhooks { get; set; } = [];
    
    #region IConfigOverride
    /// <summary>
    /// Leaderboard ID that should be used for this webhook instead of the default one in <see cref="AdventConfig.LeaderboardId"/>
    /// </summary>
    [XmlElement(nameof(LeaderboardId))]
    public string? LeaderboardId { get; set; }
    
    /// <summary>
    /// Year that should be used instead of the default specified at <see cref="AdventConfig.Year"/>
    /// </summary>
    [XmlElement("Year")]
    public int? Year { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.Username"/>
    /// </summary>
    [XmlElement(nameof(Username))]
    public string? Username { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.AvatarUrl"/>
    /// </summary>
    [XmlElement(nameof(AvatarUrl))]
    public string? AvatarUrl { get; set; }
    #endregion
}

public class DiscordWebhookItem : IConfigOverride
{
    [Required] [XmlText] public string Url { get; set; } = "";
    
    #region IConfigOverride
    /// <summary>
    /// Leaderboard ID that should be used for this webhook instead of the default one in <see cref="AdventConfig.LeaderboardId"/>
    /// </summary>
    [XmlAttribute(nameof(LeaderboardId))]
    public string? LeaderboardId { get; set; }
    
    /// <summary>
    /// Year that should be used instead of the default specified at <see cref="AdventConfig.Year"/>
    /// </summary>
    [XmlAttribute("Year")]
    public int? Year { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.Username"/>
    /// </summary>
    [XmlAttribute(nameof(Username))]
    public string? Username { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.AvatarUrl"/>
    /// </summary>
    [XmlAttribute(nameof(AvatarUrl))]
    public string? AvatarUrl { get; set; }
    #endregion
}

public interface IConfigOverride
{
    /// <summary>
    /// Leaderboard ID that should be used for this webhook instead of the default one in <see cref="AdventConfig.LeaderboardId"/>
    /// </summary>
    [XmlAttribute(nameof(LeaderboardId))]
    public string? LeaderboardId { get; set; }
    
    /// <summary>
    /// Year that should be used instead of the default specified at <see cref="AdventConfig.Year"/>
    /// </summary>
    [XmlAttribute("Year")]
    public int? Year { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.Username"/>
    /// </summary>
    [XmlAttribute(nameof(Username))]
    public string? Username { get; set; }
    
    /// <summary>
    /// Avatar Url to use in <see cref="DiscordWebhookEventArgs.AvatarUrl"/>
    /// </summary>
    [XmlAttribute(nameof(AvatarUrl))]
    public string? AvatarUrl { get; set; }
}