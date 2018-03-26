using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiCryptoToolLib.Mining
{
    public class Algorithm
    {
        public string Name { get; }

        public Algorithm(string name)
        {
            Name = name;
        }

        public bool IsMinableWith(Miner miner) => miner.Algorithms.Contains(this);

        public override string ToString()
        {
            return Name;
        }

        public static readonly Algorithm Bastion = new Algorithm("bastion");
        public static readonly Algorithm Bitcore = new Algorithm("bitcore");
        public static readonly Algorithm Blake = new Algorithm("blake");
        public static readonly Algorithm Blakecoin = new Algorithm("blakecoin");
        public static readonly Algorithm Blake2S = new Algorithm("blake2s");
        public static readonly Algorithm Bmw = new Algorithm("bmw");
        public static readonly Algorithm Cryptolight = new Algorithm("cryptolight");
        public static readonly Algorithm Cryptonight = new Algorithm("cryptonight");
        public static readonly Algorithm C11Flax = new Algorithm("c11/flax");
        public static readonly Algorithm Decred = new Algorithm("decred");
        public static readonly Algorithm Deep = new Algorithm("deep");
        public static readonly Algorithm DmdGr = new Algorithm("dmd-gr");
        public static readonly Algorithm Equihash = new Algorithm("equihash");
        public static readonly Algorithm Ethash = new Algorithm("ethash");
        public static readonly Algorithm Fresh = new Algorithm("fresh");
        public static readonly Algorithm Fugue256 = new Algorithm("fugue256");
        public static readonly Algorithm Groestl = new Algorithm("groestl");
        public static readonly Algorithm Hsr = new Algorithm("hsr");
        public static readonly Algorithm Jackpot = new Algorithm("jackpot");
        public static readonly Algorithm Keccak = new Algorithm("keccak");
        public static readonly Algorithm Keccakc = new Algorithm("keccakc");
        public static readonly Algorithm Lbry = new Algorithm("lbry");
        public static readonly Algorithm Luffa = new Algorithm("luffa");
        public static readonly Algorithm Lyra2 = new Algorithm("lyra2");
        public static readonly Algorithm Lyra2V2 = new Algorithm("lyra2v2");
        public static readonly Algorithm Lyra2Z = new Algorithm("lyra2z");
        public static readonly Algorithm Hmq1725 = new Algorithm("hmq1725");
        public static readonly Algorithm Jha = new Algorithm("jha");
        public static readonly Algorithm MyrGr = new Algorithm("myr-gr");
        public static readonly Algorithm Neoscrypt = new Algorithm("neoscrypt");
        public static readonly Algorithm Nist5 = new Algorithm("nist5");
        public static readonly Algorithm Penta = new Algorithm("penta");
        public static readonly Algorithm Phi = new Algorithm("phi");
        public static readonly Algorithm Polytimos = new Algorithm("polytimos");
        public static readonly Algorithm Quark = new Algorithm("quark");
        public static readonly Algorithm Qubit = new Algorithm("qubit");
        public static readonly Algorithm Scrypt = new Algorithm("scrypt");
        public static readonly Algorithm ScryptN = new Algorithm("scrypt:N");
        public static readonly Algorithm ScryptJane = new Algorithm("scrypt-jane");
        public static readonly Algorithm S3 = new Algorithm("s3");
        public static readonly Algorithm Sha256D = new Algorithm("sha256d");
        public static readonly Algorithm Sha256T = new Algorithm("sha256t");
        public static readonly Algorithm Sia = new Algorithm("sia");
        public static readonly Algorithm Sib = new Algorithm("sib");
        public static readonly Algorithm Skein = new Algorithm("skein");
        public static readonly Algorithm Skein2 = new Algorithm("skein2");
        public static readonly Algorithm Skunk = new Algorithm("skunk");
        public static readonly Algorithm Timetravel = new Algorithm("timetravel");
        public static readonly Algorithm Tribus = new Algorithm("tribus");
        public static readonly Algorithm X11Evo = new Algorithm("x11evo");
        public static readonly Algorithm X11 = new Algorithm("x11");
        public static readonly Algorithm X13 = new Algorithm("x13");
        public static readonly Algorithm X14 = new Algorithm("x14");
        public static readonly Algorithm X15 = new Algorithm("x15");
        public static readonly Algorithm X17 = new Algorithm("x17");
        public static readonly Algorithm Vanilla = new Algorithm("vanilla");
        public static readonly Algorithm Veltor = new Algorithm("veltor");
        public static readonly Algorithm Whirlpool = new Algorithm("whirlpool");
        public static readonly Algorithm Wildkeccak = new Algorithm("wildkeccak");
        public static readonly Algorithm Zr5 = new Algorithm("zr5");

        public static readonly IList<Algorithm> AllAlgorithms = new List<Algorithm>
        {
            Bastion,
            Bitcore,
            Blake,
            Blakecoin,
            Blake2S,
            Bmw,
            Cryptolight,
            Cryptonight,
            C11Flax,
            Decred,
            Deep,
            DmdGr,
            Equihash,
            Ethash,
            Fresh,
            Fugue256,
            Groestl,
            Hmq1725,
            Hsr,
            Jackpot,
            Jha,
            Keccak,
            Keccakc,
            Lbry,
            Luffa,
            Lyra2,
            Lyra2V2,
            Lyra2Z,
            MyrGr,
            Neoscrypt,
            Nist5,
            Penta,
            Phi,
            Polytimos,
            Quark,
            Qubit,
            Scrypt,
            ScryptN,
            ScryptJane,
            S3,
            Sha256D,
            Sha256T,
            Sia,
            Sib,
            Skein,
            Skein2,
            Skunk,
            Timetravel,
            Tribus,
            X11Evo,
            X11,
            X13,
            X14,
            X15,
            X17,
            Vanilla,
            Veltor,
            Whirlpool,
            Wildkeccak,
            Zr5
        };

        public static Algorithm FromString(string name)
        {
            var algorithm = AllAlgorithms.FirstOrDefault(i => i.Name == name.ToLower());

            if (algorithm == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown algorithm {name}");

            return algorithm;
        }
    }
}
