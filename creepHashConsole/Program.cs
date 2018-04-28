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
using System.Threading;
using MultiCryptoToolLib.Common.Logging;
using MultiCryptoToolLib.Mining;

namespace MultiMinerConsole
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            ConsoleLogger.Register();

            if (args.Length < 3)
            {
                Logger.Fatal("Too few parameters");
                return Usage();
            }

            Coin.LoadFromJson("confs/coins.json");
            Miner.LoadFromJson("confs/miner.json");

            var uri = default(Uri);
            var coin = string.Empty;
            var address = string.Empty;

            foreach (var i in args)
            {
                var splits = i.Split('=');

                if (splits.Length < 2)
                {
                    Logger.Warning($"Unknown argument {i}");
                    continue;
                }

                var key = splits[0];
                var value = splits[1];

                try
                {
                    if (key == "-uri")
                        uri = new Uri(value);
                    else if (key == "-coin")
                        coin = value;
                    else if (key == "-address")
                        address = value;
                    else
                        Logger.Warning($"Unknown argument {i}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Could not parse {key}: {e}");
                }
            }

            if (uri == null || coin == null || string.IsNullOrWhiteSpace(address))
            {
                Logger.Fatal("Invalid argument(s)");
                return Usage();
            }

            var cancelToken = new CancellationTokenSource();
            var exitToken = new CancellationTokenSource();
            var multiMiner = new MultiMinerBase(uri, coin, address, cancelToken.Token);

            Console.CancelKeyPress += (c, a) =>
            {
                a.Cancel = true;
                cancelToken.Cancel();
            };

            multiMiner.Run();
            exitToken.Cancel();

            return cancelToken.Token.IsCancellationRequested ? 1 : 0;
        }

        private static int Usage()
        {
            Logger.Info("MultiMinerConsole -uri=URI -coin=COIN -address=ADDRESS");
            Logger.Info("   -uri:       the URI of the server");
            Logger.Info("   -coin:      the name of the coin to mine");
            Logger.Info("   -address:   the address for the coin to mine");
            return 1;
        }
    }
}
