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

using FluentScheduler;
using kate.shared.Helpers;
using NLog;

namespace AOCNotify;

public class JobManagerLogger
{
    private readonly Logger _log = LogManager.GetCurrentClassLogger();
    public void Start()
    {
        if (_started) return;
        _started = true;
        JobManager.JobException += OnException;
        JobManager.JobStart += OnStart;
        JobManager.JobEnd += OnEnd;
    }
    public void End()
    {
        if (!_started) return;
        _started = false;
        JobManager.JobException -= OnException;
        JobManager.JobStart -= OnStart;
        JobManager.JobEnd -= OnEnd;
    }
    private bool _started = false;
    private void OnException(JobExceptionInfo info)
    {
        _log.Error(info.Exception, nameof(JobManager.JobException) + " in " + info.Name);
    }
    private void OnStart(JobStartInfo info)
    {
        _log.Trace(nameof(JobManager.JobStart) + " " + info.Name);
    }
    private void OnEnd(JobEndInfo info)
    {
        _log.Debug($"{nameof(JobManager.JobEnd)} {info.Name} (duration: {FormatHelper.Duration(info.Duration)}, next run: {info.NextRun?.ToLocalTime()})");
    }
}