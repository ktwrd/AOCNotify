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
using FluentScheduler;
using NLog;

namespace AOCNotify;

public class ReloadConfigJob(AppConfig config) : IJob
{
    private readonly Logger _log = LogManager.GetCurrentClassLogger();
    public void Execute()
    {
        var location = Program.GetConfigLocation();
        _log.Trace($"Reloading config: {location}");
        try
        {
            config.ReadFromFile(location);
            _log.Trace("Finished reloading config file");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to reload config at: " + location);
        }
    }
}