using System.Threading;
using HiveShard.Interface;

namespace HiveShard.Provider
{
    public class CancellationProvider: ICancellationProvider
    {
        private CancellationTokenSource _source = new CancellationTokenSource();
        public CancellationToken GetToken() => _source.Token;

        public void RequestCancellation()
        {
            _source.Cancel();
        }
    }
}