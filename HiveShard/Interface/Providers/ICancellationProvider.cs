using System.Threading;

namespace HiveShard.Interface.Providers
{
    public interface ICancellationProvider
    {
        public CancellationToken GetToken();
    }
}