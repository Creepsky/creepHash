using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network.Algorithm
{
    public class AlgorithmJsonLoader : IAlgorithmLoader
    {
        private readonly JArray _json;

        public AlgorithmJsonLoader(JArray json)
        {
            _json = json;
        }

        public IDictionary<Mining.Algorithm, Uri> Load(CancellationToken cancel)
        {
            var collection = new Dictionary<Mining.Algorithm, Uri>();

            foreach (var j in _json)
            {

            }

            return collection;
        }
    }
}