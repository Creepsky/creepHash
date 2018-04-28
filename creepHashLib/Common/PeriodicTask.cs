/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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