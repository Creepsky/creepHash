using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCryptoToolLib.Network.Algorithm
{
    public interface IAlgorithmLoader
    {
        IDictionary<Mining.Algorithm, Uri> Load(CancellationToken cancel);
    }

    public interface IAlgorithLoaderAsync : IAlgorithmLoader
    {
        Task<IDictionary<Mining.Algorithm, Uri>> LoadAsync(CancellationToken cancel);
    }
}