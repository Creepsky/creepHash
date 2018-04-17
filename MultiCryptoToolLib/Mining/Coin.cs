using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Mining
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
        public Algorithm Algorithm { get; }

        public Coin(string name, string shortName, Algorithm algorithm)
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
                Coins.Add(new Coin(name, shortName, Algorithm.FromString(algorithm)));
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
