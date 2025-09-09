using System.Threading;
using System.Threading.Tasks;

namespace HiveShard.Fabric
{
    public interface IFabric
    {
        Task Start(CancellationToken cancellationToken);
        Task WaitForReady();
    }
}