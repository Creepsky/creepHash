using System;
using System.Net;
using System.Threading;
using MultiCryptoToolLib.Common.Logging;
using MultiCryptoToolLib.Mining;

namespace MultiMinerConsole
{
    internal class Program
    {
        private static int Main()
        {
            ConsoleLogger.Register();
            
            var cancelToken = new CancellationTokenSource();
            var exitToken = new CancellationTokenSource();
            var multiMiner = new MultiMinerBase(IPAddress.Parse("45.76.81.7"), cancelToken.Token);

            Console.CancelKeyPress += (c, a) =>
            {
                a.Cancel = true;
                cancelToken.Cancel();
            };

            multiMiner.Run();
            exitToken.Cancel();

            return cancelToken.Token.IsCancellationRequested ? 1 : 0;
        }
    }
}
