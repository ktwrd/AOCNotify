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

using Discord.Webhook;
using kate.shared.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static AOCNotify.AppConfig;

namespace AOCNotify.Handlers;

public class UpdateLeaderboardHandler(
    AppConfig appConfig,
    AdventClient adventClient,
    JsonSerializerOptions serializerOptions)
{
    private readonly Logger _log = LogManager.GetCurrentClassLogger();

    public async Task Run()
    {
        var allTargets = appConfig.NotifyTargets.Discord.Cast<BaseNotifyTarget>().ToList();
        _log.Info($"Started task (targets: {allTargets.Count}, leaderboards: {appConfig.Leaderboards.Count})");

        var index = 0;
        foreach (var leaderboard in appConfig.Leaderboards)
        {
            foreach (var notifyTargetId in leaderboard.NotifyTargetIds.Distinct())
            {
                var notifyTarget = allTargets.FirstOrDefault(e => e.Id.Equals(notifyTargetId));
                if (notifyTarget == null)
                {
                    throw new InvalidOperationException($"Could not find Notify Target with Id {notifyTargetId} for Leaderboard {leaderboard.DisplayName} ({leaderboard.LeaderboardId}, {leaderboard.Year}, index={index})");
                }
                await ProcessLeaderboard(leaderboard, notifyTarget);
            }

            index++;
        }
    }

    private async Task ProcessLeaderboard(
        LeaderboardItem leaderboardItem,
        BaseNotifyTarget notifyTarget)
    {
        var leaderboardIdent = $"[leaderboard={leaderboardItem.LeaderboardId}, notifyTarget={notifyTarget.Id}]";
        _log.Info($"{leaderboardIdent} Started task");
        var previous = await ReadPreviousLeaderboard(leaderboardItem, notifyTarget);
        var current = await adventClient.GetAsync(leaderboardItem);

        var content = GenerateContent(leaderboardItem.Year, previous, current);
        if (content.Count == 0)
        {
            _log.Info($"{leaderboardIdent} Skipped since content hasn't changed.");
            return;
        }

        await SendMessage(leaderboardItem, notifyTarget, content);
        await WriteLeaderboard(current, leaderboardItem, notifyTarget);
        _log.Info($"{leaderboardIdent} Finished!");
    }
    private async Task WriteLeaderboard(
        LeaderboardResponse leaderboard,
        LeaderboardItem leaderboardItem,
        BaseNotifyTarget notifyTarget)
    {
        var location = GetLeaderboardFilename(leaderboardItem, notifyTarget);
        await using var file = File.Open(location, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        file.SetLength(0);
        await JsonSerializer.SerializeAsync(file, leaderboard, serializerOptions);
        _log.Info($"Wrote {NeoSmart.PrettySize.PrettySize.Bytes(file.Length)} to: {location}");
    }
    private async Task SendMessage(
        LeaderboardItem leaderboardItem, 
        BaseNotifyTarget notifyTarget, IEnumerable<string> content)
    {
        if (notifyTarget is DiscordNotifyTarget discord)
        {
            await SendDiscordMessagee(leaderboardItem, discord, content);
        }
        else
        {
            throw new NotImplementedException($"Where NotifyTarget {notifyTarget.Id} Type is {notifyTarget.GetType()}");
        }
    }
    private async Task SendDiscordMessagee(
        LeaderboardItem leaderboardItem,
        DiscordNotifyTarget notifyTarget, IEnumerable<string> content)
    {
        var leaderboardIdent = $"[leaderboard={leaderboardItem.LeaderboardId}, notifyTarget={notifyTarget.Id}]";
        using var client = new DiscordWebhookClient(notifyTarget.WebhookUrl);
        var chunked = content.ChunkByLength().ToList();
        var index = 1;
        foreach (var submessageContent in chunked)
        {
            _log.Info($"{leaderboardIdent} Sending message {index}/{chunked.Count}");
            await InternalDiscordSendMessage(client, notifyTarget, string.Join("\n", submessageContent));
            await Task.Delay(1500);
            index++;
        }
    }
    private static async Task InternalDiscordSendMessage(
        DiscordWebhookClient client,
        DiscordNotifyTarget notifyTarget,
        string content)
    {
        try
        {
            await client.SendMessageAsync(
                text: content,
                username: notifyTarget.Username,
                avatarUrl: notifyTarget.AvatarUrl);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[notifyTargetId={notifyTarget.Id}] Failed to send message into webhook {notifyTarget.WebhookUrl}\nContent: {content}", ex);
        }
    }
    private async Task<LeaderboardResponse?> ReadPreviousLeaderboard(LeaderboardItem leaderboardItem, BaseNotifyTarget notifyTarget)
    {
        var leaderboardIdent = $"[leaderboard={leaderboardItem.LeaderboardId}, notifyTarget={notifyTarget.Id}]";
        var location = GetLeaderboardFilename(leaderboardItem, notifyTarget);
        _log.Info($"{leaderboardIdent} Reading previous leaderboard: {location}");
        if (!File.Exists(location))
        {
            _log.Debug($"{leaderboardIdent} File doesn't exist: {location}");
            return null;
        }
        await using var file = File.OpenRead(location);
        return await JsonSerializer.DeserializeAsync<LeaderboardResponse>(file, serializerOptions);
    }

    private string GetLeaderboardFilename(LeaderboardItem leaderboardItem, BaseNotifyTarget notifyTarget)
    {
        var cachePath = Path.GetFullPath("./cache/");
        var filename = $"leaderboard-cache-{leaderboardItem.Year}-{leaderboardItem.LeaderboardId}-{notifyTarget.Id}.json";
        if (!Directory.Exists(cachePath))
        {
            try
            {
                Directory.CreateDirectory(cachePath);
                _log.Debug($"Created directory: {cachePath}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create directory: {cachePath}", ex);
            }
        }
        var location = Path.Join(cachePath, filename);
        return Path.GetFullPath(location);
    }

    private ICollection<string> GenerateContent(
        int year,
        LeaderboardResponse? previous,
        LeaderboardResponse current)
    {
        var previousContent = previous == null ? [] : GenerateContent(year, previous);
        var currentContent = current == null ? [] : GenerateContent(year, current);

        return currentContent.Concat(previousContent).Where(e => !previousContent.Contains(e)).ToList();
    }
    private ICollection<string> GenerateContent(
        int year,
        LeaderboardResponse leaderboard)
    {
        var list = new List<KeyValuePair<long, string>>();
        var data = leaderboard.Members
            .SelectMany(a
            => a.Value.CompletionLevel.SelectMany(b
            => b.Value.Select(c =>
            new
            {
                memberPair = a,
                dayPair = b,
                completionPair = c
            })));
        foreach (var row in data)
        {
            var endTimestamp = DateTimeOffset.FromUnixTimeSeconds(row.completionPair.Value.Timestamp);
            var diff = endTimestamp - new DateTime(year, 12, row.dayPair.Key, 5, 0, 0, DateTimeKind.Utc);

            var content = string.Join(" ",
                $"`{row.memberPair.Value.Name}` solved",
                $"day {row.dayPair.Key}",
                $"part {row.completionPair.Key}",
                "in",
                FormatHelper.Duration(diff)
            );

            list.Add(new KeyValuePair<long, string>(row.completionPair.Value.Timestamp, content));
        }
        return list.OrderBy(v => v.Key).Select(v => v.Value).ToList();
    }
}
