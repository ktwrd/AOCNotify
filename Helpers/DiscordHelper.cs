using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AOCNotify.Helpers;

public class DiscordHelper
{
    /// <summary>
    /// Chunk up message lines so each item in the result has a
    /// maximum length of the <paramref name="maxLineLength"/> provided.
    /// </summary>
    public static string[] ChunkMessageLines(string[] existing, int maxLineLength)
    {
        var ss = new List<string>();
        var w = "";
        for (int i = 0; i < existing.Length; i++)
        {
            var l = existing[i];
            if ((w.Length + l.Length) > maxLineLength)
            {
                ss.Add(w);
                w = "";
            }

            w += $"{l}\n";
        }

        if (!string.IsNullOrEmpty(w))
        {
            ss.Add(w);
        }

        return ss.ToArray();
    }

    public static async Task SendMessage(string url, string body, string mediaType = "text/plain")
    {
        var client = new HttpClient();
        await client.PostAsync(url, new StringContent(body, Encoding.UTF8, mediaType));
    }

    public static async Task SendMessage(
        string url,
        string[] lines,
        DiscordWebhookEventArgs args)
    {
        var submsg = ChunkMessageLines(lines, 2000);
        if (submsg.Length > 0)
        {
            for (int i = 0; i < submsg.Length; i++)
            {
                var d = new DiscordWebhookEventArgs()
                {
                    Username = args.Username,
                    AvatarUrl = args.AvatarUrl,
                    Content = submsg[i]
                };

                await SendMessage(url, d);
                if (i < (submsg.Length - 1))
                    await Task.Delay(500);
            }
        }
    }

    public static async Task SendMessage(string url, DiscordWebhookEventArgs args)
    {
        var json = JsonSerializer.Serialize(args, new JsonSerializerOptions()
        {
            IncludeFields = true
        });
        await SendMessage(url, json, "application/json");
    }
}