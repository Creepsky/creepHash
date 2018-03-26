using System;
using System.IO;

namespace MultiCryptoToolLib.Common.Logging
{
    public static class ConsoleLogger
    {
        private static readonly object Lock = new object();

        public static void Log(string message, string file, int line, LogLevel level)
        {
            ConsoleColor foreColor = ConsoleColor.White, backColor = ConsoleColor.Black;

            switch (level)
            {
                case LogLevel.Trace:
                    foreColor = ConsoleColor.Magenta;
                    backColor = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    foreColor = ConsoleColor.Magenta;
                    break;
                case LogLevel.Info:
                    break;
                case LogLevel.Warning:
                    foreColor = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.Error:
                    foreColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    foreColor = ConsoleColor.Red;
                    backColor = ConsoleColor.White;
                    break;
                case LogLevel.Panic:
                    foreColor = ConsoleColor.White;
                    backColor = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            
            Log(message, Path.GetFileName(file), line, foreColor, backColor);
        }

        public static void LogTrace(string message, string file, int line) => Log(message, file, line, LogLevel.Trace);
        public static void LogDebug(string message, string file, int line) => Log(message, file, line, LogLevel.Debug);
        public static void LogInfo(string message, string file, int line) => Log(message, file, line, LogLevel.Info);
        public static void LogWarning(string message, string file, int line) => Log(message, file, line, LogLevel.Warning);
        public static void LogError(string message, string file, int line) => Log(message, file, line, LogLevel.Error);
        public static void LogFatal(string message, string file, int line) => Log(message, file, line, LogLevel.Fatal);
        public static void LogPanic(string message, string file, int line) => Log(message, file, line, LogLevel.Panic);

        public static void LogException(string message, Exception exception, string file, int line)
        {
            file = Path.GetFileName(file);

            lock (Lock)
            {
                foreach (var messageLine in message.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    LogUnsafe(messageLine, file, line, ConsoleColor.Red);

                foreach (var messageLine in exception.Message.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    LogUnsafe(messageLine, file, line, ConsoleColor.Red);

                foreach (var messageLine in exception.StackTrace.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    LogUnsafe(messageLine, file, line, ConsoleColor.DarkGray);
            }
        }

        public static void Log(string message, string file, int line, ConsoleColor foreColor,
                               ConsoleColor backColor = ConsoleColor.Black)
        {
            lock (Lock)
            {
                foreach (var messageLine in message.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    LogUnsafe(messageLine, file, line, foreColor, backColor);
            }
        }

        public static void Register()
        {
            Logger.OnTrace += LogTrace;
            Logger.OnDebug += LogDebug;
            Logger.OnInfo += LogInfo;
            Logger.OnWarning += LogWarning;
            Logger.OnError += LogError;
            Logger.OnFatal += LogFatal;
            Logger.OnPanic += LogPanic;
            Logger.OnException += LogException;
        }

        private static void LogUnsafe(string message, string file, int line, ConsoleColor foreColor,
            ConsoleColor backColor = ConsoleColor.Black)
        {
            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.Write($"{DateTime.Now:T} ");
            System.Console.ResetColor();
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.Write($"{file.PadLeft(18)}, {line.ToString().PadRight(4)} ");
            System.Console.ForegroundColor = foreColor;
            System.Console.BackgroundColor = backColor;
            System.Console.WriteLine(message.Replace("\n", "").Replace("\r", ""));
            System.Console.ResetColor();
        }
    }
}