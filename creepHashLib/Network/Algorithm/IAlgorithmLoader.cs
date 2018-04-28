using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCryptoToolLib.Network.Algorithm
{
    public interface IAlgorithmLoader
    {
        IDictionary<string, Uri> Load(CancellationToken cancel);
    }

    public interface IAlgorithLoaderAsync : IAlgorithmLoader
    {
        Task<IDictionary<string, Uri>> LoadAsync(CancellationToken cancel);
    }
}