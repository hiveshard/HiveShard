using System.Threading.Tasks;

namespace HiveShard.Interface;

public interface IIsolatedEntryPoint
{
    public Task Start();
}