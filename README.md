# Advent of Code Notifications
Console application written in C# for sending notifications when someone completes a challenge in a [private leaderboard](https://adventofcode.com/2025/leaderboard/private) for [Advent of Code](https://adventofcode.com).

The task to pull the leaderboard and send notifications is executed every 5 minutes (and at launch).

To use this application, download the [latest release](https://github.com/ktwrd/AOCNotify/releases/latest), extract it to whereever you want, and create a config file that looks something like this:
```xml
<!-- ****** AOCNotify Example Config File ****** -->
<?xml version="1.0" encoding="utf-8" ?>
<AppConfig>
  <!-- List of reusable targets when a referencing leaderboard changes -->
  <NotifyTargets>
    <!-- The "Id" attribute is always required for a notify target -->
    <Discord Id="my_discord_server">
      <!-- WebhookUrl: REQUIRED -->
      <WebhookUrl>SUPER COOL DISCORD WEBHOOK URL GOES HERE</WebhookUrl>
      <!-- AvatarUrl: Optional -->
      <AvatarUrl>https://res.kate.pet/upload/adventofcode.png</AvatarUrl>
      <!-- Username: Optional -->
      <Username>Advent of Code</Username>
    </Discord>
  </NotifyTargets>

  <Leaderboard Year="2025" Id="123456"> <!-- the "Id" attribute MUST BE A NUMBER!!! -->

    <!-- the element NotifyTargetId contains the Id of a
         NotifyTarget that should be notified of any leaderboard changes-->
    <NotifyTargetId>my_discord_server</NotifyTargetId>
    <Token>YOUR TOKEN GOES HERE WHICH IS JUST YOUR AOC SESSION COOKIE!</Token>
  </Leaderboard>

  <!-- You can have multiple leaderboards btw! -->
  <Leaderboard Year="2024" Id="123456">
    <NotifyTargetId>my_discord_server</NotifyTargetId>
    <Token>YOUR TOKEN GOES HERE WHICH IS JUST YOUR AOC SESSION COOKIE!</Token>
  </Leaderboard>
</AppConfig>
```

Once you've made your config file, just put it in the same folder as `AOCNotify.exe` (or `AOCNotify` for Mac/Linux users) and run the executable with no arguments.

Multiple log files are generated, but this can be modified in `nlog.config`. For instructions on how to modify it, then you can look at [the NLog wiki](https://nlog-project.org/config/).

When you run AOC Notify, it'll run continuously like a background service. This is done so it can easily be used as a systemd service, or a Windows service.
At some point in the future, I plan on making a one-shot mode so it can be used in cron.

For a more information about the config file format, you can look at [AppConfig.cs](AppConfig.cs).

## How do I get my Advent of Code Token!

To get your token, do the following:
- Login to [adventofcode.com](https://adventofcode.com)
- Navigate to your private leaderboard
- Open up your browser developer tools (can be done with Shift+Ctrl+I, or F12)
- Click on the "Network" tab
- Then append `.json` to the end of your URL (should now look like: `https://adventofcode.com/<year>/leaderboard/private/view/<leaderboard id>.json`)
- Select the latest request in the "Network" tab of your browser developer tools
- Then finally, copy the value for "session" in your request cookies (it should be 128 characters long, and only contain the letters A to F and numbers 0 to 9)
