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

        var jobManagerLogger = new JobManagerLogger();
        jobManagerLogger.Start();
        JobManager.Initialize();

        try
        {
            RunJobsBlocking();
        }
        finally
        {
            jobManagerLogger.End();
        }
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

        var registry = services.GetRequiredService<Registry>();

        registry.Schedule(services.GetRequiredService<ReloadConfigJob>())
            .ToRunEvery(1).Minutes()
            .DelayFor(1).Minutes();

        registry.Schedule(services.GetRequiredService<UpdateLeaderboardJob>())
            .ToRunNow()
            .AndEvery(5).Minutes();

        JobManager.Start();

        Task.Delay(-1).Wait();
    }
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(GetConfig())
            .AddSingleton(new HttpClient())
            .AddSingleton(new JsonSerializerOptions()
            {
                WriteIndented = true
            })
            .AddSingleton(new Registry());

        services
            .AddSingleton<AdventClient>()
            .AddSingleton<UpdateLeaderboardHandler>()
            .AddSingleton<SendNotificationHandler>();
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
    public static string GetConfigLocation()
    {
        return Path.GetFullPath("./config.xml");
    }
}