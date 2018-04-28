using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common.Logging;

namespace MultiCryptoToolLib.Common
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
                    Logger.Debug($"Could not kill the process {path}: {e}");
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