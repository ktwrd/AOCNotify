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
using System.Linq;
using System.Threading.Tasks;
using Discord.Webhook;
using NLog;
using static AOCNotify.AppConfig;

namespace AOCNotify.Handlers;

public class SendNotificationHandler()
{
    private readonly Logger _log = LogManager.GetCurrentClassLogger();

    public async Task SendAsync(
        LeaderboardItem item,
        BaseNotifyTarget target,
        IEnumerable<string> content)
    {
        if (target is DiscordNotifyTarget discord)
        {
            await SendDiscordAsync(item, discord, content);
        }
        else
        {
            throw new NotImplementedException($"NotifyTarget with Id {target.Id} has invalid type: {target.GetType()}");
        }
    }

    private async Task SendDiscordAsync(
        LeaderboardItem item,
        DiscordNotifyTarget target,
        IEnumerable<string> content)
    {
        var leaderboardIdent = $"[leaderboard={item.LeaderboardId}, notifyTarget={target.Id}]";
        using var client = new DiscordWebhookClient(target.WebhookUrl);
        var chunked = content.ChunkByLength().ToList();
        var index = 1;
        foreach (var chunk in chunked)
        {
            _log.Info($"{leaderboardIdent} Sending message {index}/{chunked.Count}");
            await SendDiscordAsync(client, target, string.Join("\n", chunk));
            await Task.Delay(1500);
            index++;
        }
    }

    private async Task SendDiscordAsync(
        DiscordWebhookClient client,
        DiscordNotifyTarget target,
        string content)
    {
        try
        {
            await client.SendMessageAsync(
                text: content,
                username: target.Username,
                avatarUrl: target.AvatarUrl);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"[notifyTargetId={target.Id}] Failed to send message into webhook {target.WebhookUrl}\nContent: {content}", ex);
        }
    }
}