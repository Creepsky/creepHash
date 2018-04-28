/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Common.Logging;

namespace creepHashLib.Common
{
    public static class ProcessHelper
    {
        public static IEnumerable<string> ReadLines(string path, string parameter,
            CancellationToken cancellationToken, bool ignoreEmptyLines = true)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var process = new Process{StartInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = parameter,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }};

            process.Start();

            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                { // do nothing
                    cancellationToken.WaitHandle.WaitOne(100);
                }

                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    Logger.Warning($"Could not kill the process {path}: {e}");
                }
            }, cancellationToken);

            while (!process.StandardOutput.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    if (!process.HasExited)
                        process.Kill();

                    yield break;
                }

                var line = process.StandardOutput.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!ignoreEmptyLines)
                        yield return line;
                }
                else
                    yield return line;
            }
        }
    }
}