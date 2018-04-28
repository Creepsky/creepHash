using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network.Algorithm
{
    // TODO: remove this shit
    public class AlgorithmJsonLoader : IAlgorithmLoader
    {
        private readonly JArray _json;

        public AlgorithmJsonLoader(JArray json)
        {
            _json = json;
        }

        public IDictionary<string, Uri> Load(CancellationToken cancel)
        {
            var collection = new Dictionary<string, Uri>();

            foreach (var j in _json)
            {
            }

            return collection;
        }
    }
}