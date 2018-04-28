using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCryptoToolLib.Common
{
    public abstract class PeriodicTask<T>
    {
        private readonly CancellationToken _ctx;
        private readonly TimeSpan _tick;

        public event Action<T> Signal;

        protected PeriodicTask(TimeSpan tick, CancellationToken ctx)
        {
            _tick = tick;
            _ctx = ctx;
        }

        public async Task Start()
        {
            while (!_ctx.IsCancellationRequested)
            {
                var result = Run();
                Signal?.Invoke(result);
                await Task.Delay(_tick, _ctx);
            }
        }
        
        protected abstract T Run();
    }

    public class PeriodicActionTask<T> : PeriodicTask<T>
    {
        private readonly Func<T> _action;

        public PeriodicActionTask(TimeSpan tick, CancellationToken cancelToken, Func<T> action)
            : base(tick, cancelToken)
        {
            _action = action;
        }

        protected override T Run()
        {
            return _action();
        }
    }
}