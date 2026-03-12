using System.Threading.Tasks;

namespace HiveShard.Initializer.Interfaces;

public interface IInitializer
{
    public void Initialize(IInitializationTunnel tunnel);
}