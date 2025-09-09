using System.Threading;

namespace HiveShard.Interface
{
    public interface ICancellationProvider
    {
        public CancellationToken GetToken();
    }
}