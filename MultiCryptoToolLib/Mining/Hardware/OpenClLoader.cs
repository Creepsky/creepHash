using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;

namespace MultiCryptoToolLib.Mining.Hardware
{
    public class OpenClLoader : ILoaderAsync<ISet<Hardware>>
    {
        public ISet<Hardware> Load(CancellationToken ctx)
        {
            var platforms = ProcessHelper
                .ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--platforms", ctx)
                .Select(i => i.Split(';')[1])
                .ToList();

            return StringListToHardwares(
                ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--devices",
                    ctx), platforms);
        }

        public async Task<ISet<Hardware>> LoadAsync(CancellationToken ctx)
        {
            var platforms =
                (await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}",
                        "--platforms", ctx), ctx))
                .Select(i => i.Split(';')[1])
                .ToList();

            return StringListToHardwares(
                await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--devices",
                        ctx), ctx), platforms);
        }

        private static ISet<Hardware> StringListToHardwares(IEnumerable<string> strings, IList<string> platforms) =>
            new HashSet<Hardware>(strings.Select(i =>
            {
                var tokens = i.Split(';');
                var platformIndex = int.Parse(tokens[0]);

                return new Hardware
                {
                    Index = int.Parse(tokens[1]),
                    Name = tokens[2],
                    PlatformIndex = platformIndex,
                    Platform = platforms[platformIndex],
                    Type = HardwareType.OpenCl
                };
            }));
    }
}