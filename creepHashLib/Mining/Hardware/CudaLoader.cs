using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;

namespace MultiCryptoToolLib.Mining.Hardware
{
    public class CudaLoader : ILoaderAsync<ISet<Hardware>>
    {
        public ISet<Hardware> Load(CancellationToken cancellationToken) => StringListToHardwares(
            ProcessHelper.ReadLines($"miner/ethminer{Filename.GetFileExtensionOs()}", "-U --list-devices",
                cancellationToken));

        public async Task<ISet<Hardware>> LoadAsync(CancellationToken ctx) => StringListToHardwares(await Task.Run(
            () => ProcessHelper.ReadLines($"miner/ethminer{Filename.GetFileExtensionOs()}", "-U --list-devices",
                ctx), ctx));

        private static ISet<Hardware> StringListToHardwares(IEnumerable<string> strings) => new HashSet<Hardware>(strings.Select(i =>
            {
                var match = Regex.Match(i, @"\[(\d+)\]\s(.+)");
                if (match.Success)
                {
                    return new Hardware
                    {
                        Index = int.Parse(match.Groups[1].Value),
                        Name = match.Groups[2].Value,
                        Type = HardwareType.Cuda
                    };
                }

                return new Hardware();
            })
            .Where(i => !string.IsNullOrWhiteSpace(i.Name)));
    }
}