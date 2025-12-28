using System.Threading;
using HiveShard.Interface;
using HiveShard.Interface.Providers;

namespace HiveShard.Provider
{
    public class CancellationProvider: ICancellationProvider
    {
        private readonly CancellationTokenSource _source = new();
        public CancellationToken GetToken() => _source.Token;

        public void RequestCancellation()
        {
            _source.Cancel();
        }
    }
}