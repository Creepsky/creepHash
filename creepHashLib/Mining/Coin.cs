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
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace creepHashLib.Mining
{
    public class Coin
    {
        private static readonly IList<Coin> Coins = new List<Coin>();

        public struct Info
        {
            public Coin Coin;
            public int Port;
            public double Profitability;
        }

        /// <summary>
        /// The name of the coin
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The short name of the coin
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// The hash algorithm that is used to mine the coin
        /// </summary>
        public string Algorithm { get; }

        public Coin(string name, string shortName, string algorithm)
        {
            Name = name;
            ShortName = shortName;
            Algorithm = algorithm;
        }

        public Info CreateInfo(int port, double profitability) =>
            new Info {Coin = this, Port = port, Profitability = profitability};

        public bool IsMinableWith(Miner miner) => miner.Algorithms.Contains(Algorithm);

        public static void LoadFromJson(string path)
        {
            Coins.Clear();
            var lines = File.ReadAllText(path);
            var algorithmsJson = JObject.Parse(lines);

            foreach (var token in algorithmsJson)
            {
                var shortName = token.Key;
                var name = token.Value["name"].Value<string>();
                var algorithm = token.Value["algorithm"].Value<string>();
                Coins.Add(new Coin(name, shortName, algorithm));
            }
        }

        public static Coin FromString(string name)
        {
            var coin = Coins.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase) ||
                                                 string.Equals(i.ShortName, name, StringComparison.CurrentCultureIgnoreCase));

            if (coin == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown coin {name}");

            return coin;
        }

        public override string ToString()
        {
            return $"{Name} ({ShortName})";
        }
    }

    public static class CoinExtensions
    {
        public static bool IsMinableWith(this Coin coin, Miner miner)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            if (miner == null)
                throw new ArgumentNullException(nameof(miner));

            return miner.Algorithms.Contains(coin.Algorithm);
        }
    }
}
