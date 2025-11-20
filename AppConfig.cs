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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace AOCNotify;

[XmlRoot("AppConfig")]
public class AppConfig
{
    private readonly Lock _filesystemLock = new();
    public void WriteToFile(string location)
    {
        lock (_filesystemLock)
        {
            using var file = new FileStream(location, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            file.SetLength(0);
            file.Seek(0, SeekOrigin.Begin);
            Write(file);
        }
    }

    public void ReadFromFile(string location)
    {
        lock (_filesystemLock)
        {
            if (!File.Exists(location))
            {
                throw new ArgumentException($"{location} does not exist", nameof(location));
            }

            var content = File.ReadAllText(location);
            var xmlSerializer = new XmlSerializer(GetType());
            var xmlTextReader = new XmlTextReader(new StringReader(content)) { XmlResolver = null };
            var data = (AppConfig?)xmlSerializer.Deserialize(xmlTextReader);
            if (data == null)
            {
                return;
            }

            foreach (var p in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                p.SetValue(this, p.GetValue(data));
            }

            foreach (var f in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                f.SetValue(this, f.GetValue(data));
            }
        }
    }

    public void Write(Stream stream)
    {
        var serializer = new XmlSerializer(GetType());
        var options = new XmlWriterSettings()
        {
            Indent = true
        };
        using var writer = XmlWriter.Create(stream, options);
        serializer.Serialize(writer, this);
    }

    public class NotifyTargetElement
    {
        [XmlElement("Discord")]
        public List<DiscordNotifyTarget> Discord { get; set; } = [];
    }

    public class DiscordNotifyTarget : BaseNotifyTarget
    {
        [Required]
        [XmlElement("WebhookUrl")]
        public string WebhookUrl { get; set; } = "";

        [XmlElement("Message.Username")]
        public string? Username { get; set; }
        [XmlElement("Message.AvatarUrl")]
        public string? AvatarUrl { get; set; }
    }

    public class BaseNotifyTarget
    {
        [Required]
        [XmlAttribute("Id")]
        public string Id { get; set; } = "";
    }

    public class LeaderboardItem
    {
        [Required]
        [XmlElement("Token")]
        public string Token { get; set; } = "";

        [XmlAttribute("Year")]
        public int Year { get; set; } = DateTimeOffset.UtcNow.Year;

        [XmlAttribute("DisplayName")]
        public string? DisplayName { get; set; }

        [Required]
        [XmlAttribute("Id")]
        public string LeaderboardId { get; set; } = "";

        [XmlElement("NotifyTargetId")]
        public List<string> NotifyTargetIds { get; set; } = [];
    }

    [XmlElement("Leaderboard")]
    public List<LeaderboardItem> Leaderboards { get; set; } = [];

    [XmlElement("NotifyTargets")]
    public NotifyTargetElement NotifyTargets { get; set; } = new();
}
