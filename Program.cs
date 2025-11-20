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

using AOCNotify.Handlers;
using FluentScheduler;
using kate.shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AOCNotify;

public static class Program
{
    public static void Main(string[] args)
    {
        PrintVersion();
        var nlogConfigLocation = Path.GetFullPath("./nlog.config");
        Console.WriteLine("NLog Config Location: " + nlogConfigLocation);
        LogManager.Setup().LoadConfigurationFromFile(nlogConfigLocation);

        JobManager.JobException += JobManagerOnException;
        JobManager.JobStart += JobManagerOnStart;
        JobManager.JobEnd += JobManagerOnEnd;

        JobManager.Initialize();

        RunJobsBlocking();
    }
    private static void PrintVersion()
    {
        var content = "-------- AOC Notify Release: " + typeof(Program).Assembly.GetName().Version;
        Console.WriteLine("".PadRight(content.Length, '-'));
        Console.WriteLine(content);
        Console.WriteLine();
    }
    private static void RunJobsBlocking()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var services = serviceCollection.BuildServiceProvider();
        JobManager.AddJob(UpdateLeaderboardHandlerJob, s =>
        {
            s
            .WithName(nameof(UpdateLeaderboardHandlerJob))
            .ToRunNow().AndEvery(5).Minutes();
        });
        JobManager.AddJob(ReloadConfig, s =>
        {
            s
            .WithName(nameof(ReloadConfig))
            .ToRunEvery(1).Minutes()
            .DelayFor(1).Minutes();
        });

        JobManager.Start();

        Task.Delay(-1).Wait();
        async void UpdateLeaderboardHandlerJob()
        {
            var handler = services.GetRequiredService<UpdateLeaderboardHandler>();
            await handler.Run();
        }
        void ReloadConfig()
        {
            var location = GetConfigLocation();
            var log = LogManager.GetLogger(typeof(Program).Namespace + "." + nameof(ReloadConfig));
            log.Trace($"Reloading config: {location}");
            try
            {
                services.GetRequiredService<AppConfig>().ReadFromFile(GetConfigLocation());
                log.Trace("Finished reloading config file");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to reload config");
            }
        }
    }
    private static readonly Logger JogManagerLog = LogManager.GetLogger(nameof(JobManager));
    private static void JobManagerOnException(JobExceptionInfo info)
    {
        JogManagerLog.Error(info.Exception, nameof(JobManager.JobException) + " in " + info.Name);
    }
    private static void JobManagerOnStart(JobStartInfo info)
    {
        JogManagerLog.Trace(nameof(JobManager.JobStart) + " " + info.Name);
    }
    private static void JobManagerOnEnd(JobEndInfo info)
    {
        JogManagerLog.Debug($"{nameof(JobManager.JobEnd)} {info.Name} (duration: {FormatHelper.Duration(info.Duration)}, next run: {info.NextRun?.ToLocalTime()})");
    }
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(GetConfig())
            .AddSingleton(new HttpClient())
            .AddSingleton(new JsonSerializerOptions()
            {
                WriteIndented = true
            });

        services
            .AddSingleton<AdventClient>()
            .AddSingleton<UpdateLeaderboardHandler>();
    }
    private static AppConfig GetConfig()
    {
        var config = new AppConfig();
        var location = Path.GetFullPath("./config.xml");
        if (!File.Exists(location))
        {
            config.WriteToFile(location);
            throw new InvalidOperationException($"Could not find config file, so an empty one was written to: \"{location}\"");
        }
        config.ReadFromFile(location);
        return config;
    }
    private static string GetConfigLocation()
    {
        return Path.GetFullPath("./config.xml");
    }
}