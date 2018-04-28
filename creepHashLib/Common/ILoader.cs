using System.Threading;
using System.Threading.Tasks;

namespace MultiCryptoToolLib.Common
{
    public interface ILoader<out T>
    {
        T Load(CancellationToken ctx);
    }

    public interface ILoaderAsync<T> : ILoader<T>
    {
        Task<T> LoadAsync(CancellationToken ctx);
    }
}