using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;

namespace MultiCryptoToolLib.Mining
{
    public abstract class MiningInstance
    {
        public Algorithm Algorithm { get; }
        public Miner Miner { get; }
        public IEnumerable<Hardware.Hardware> MiningHardware { get; }
        public Task Task { get; private set; }

        private readonly CancellationTokenSource _internCtx = new CancellationTokenSource();

        protected MiningInstance(Miner miner, IEnumerable<Hardware.Hardware> miningHardware, Algorithm algorithm)
        {
            Miner = miner ?? throw new ArgumentNullException(nameof(miner));
            MiningHardware = miningHardware ?? throw new ArgumentNullException(nameof(miningHardware));
            Algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            
            if (!algorithm.IsMinableWith(Miner))
                throw new ArgumentOutOfRangeException(nameof(algorithm), $"{algorithm} is not minable with {Miner}");
        }

        public Task RunAsync(CancellationToken ctx)
        {
            Task = Task.Run(() => Run(_internCtx.Token, ctx), ctx);
            return Task;
        } 

        public abstract void Run(CancellationToken internCtx, CancellationToken externCtx);

        public void Cancel()
        {
            _internCtx.Cancel();
        }

        public static MiningInstance Create(Miner miner, IEnumerable<Hardware.Hardware> hardware, Algorithm algorithm,
            IPAddress ip, int port)
        {
            if (miner == Miner.CcMiner)
                return new CcminerInstance(hardware, algorithm, ip, port);
            //if (miner == Miner.EthMiner)
            //    return new 
           
            throw new ArgumentOutOfRangeException(nameof(miner), "The miner is not supported");
        }
    }

    public class CcminerInstance : MiningInstance
    {
        private readonly IPAddress _ip;
        private readonly int _port;

        public CcminerInstance(IEnumerable<Hardware.Hardware> miningHardware, Algorithm algorithm, IPAddress ip, int port)
            : base(Miner.CcMiner, miningHardware, algorithm)
        {
            _ip = ip;
            _port = port;
        }

        public override void Run(CancellationToken internCtx, CancellationToken externCtx)
        {
            try
            {
                var uri = $"stratum+tcp://{_ip}:{_port}";
                var parameter = $"-o {uri} -d {string.Join(",", MiningHardware.Select(i => i.Index))} -a {Algorithm} -u benchmark --no-color";
                Logger.Debug($"Starting {Miner.Path} {parameter}");

                Task.Run(() =>
                {
                    while (!internCtx.IsCancellationRequested)
                    {
                        if (externCtx.IsCancellationRequested)
                            Cancel();
                        else
                            externCtx.WaitHandle.WaitOne(1000);
                    }
                }, internCtx);

                foreach (var line in ProcessHelper.ReadLines(Miner.Path, parameter, internCtx))
                    Logger.Debug(line);
            }
            catch (OperationCanceledException e)
            {
                if (e.CancellationToken.IsCancellationRequested)
                    e.CancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}