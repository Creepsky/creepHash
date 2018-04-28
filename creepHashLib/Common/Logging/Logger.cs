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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MultiCryptoToolLib.Common.Logging
{
    public delegate void LogMessage(string message, string file, int line);
    public delegate void ExceptionMessage(string message, Exception exception, string file, int line);

    public enum LogLevel
    {
        Trace = 10000000,
        Debug = 1000000,
        Info = 100000,
        Warning = 10000,
        Error = 1000,
        Fatal = 100,
        Panic = 1
    }

    public static class Logger
    {
        public static event LogMessage OnTrace;
        public static event LogMessage OnDebug;
        public static event LogMessage OnInfo;
        public static event LogMessage OnWarning;
        public static event LogMessage OnError;
        public static event LogMessage OnFatal;
        public static event LogMessage OnPanic;
        public static event ExceptionMessage OnException;

        [Conditional("DEBUG")]
        public static void Trace(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            OnTrace?.Invoke(message, file, line);
        }

        [Conditional("DEBUG")]
        public static void Debug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        {
            OnDebug?.Invoke(message, file, line);
        }
        public static void Info(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnInfo?.Invoke(message, file, line);
        public static void Warning(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnWarning?.Invoke(message, file, line);
        public static void Error(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnError?.Invoke(message, file, line);
        public static void Fatal(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnFatal?.Invoke(message, file, line);
        public static void Panic(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnPanic?.Invoke(message, file, line);
        public static void Exception(string message, Exception exception, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) =>
            OnException?.Invoke(message, exception, file, line);
    }
}