using System.Threading.Tasks;

namespace HiveShard.Initializer.Interfaces;

public interface IInitializer
{
    public Task Initialize(IInitializationTunnel tunnel);
}