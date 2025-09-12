using System.Threading.Tasks;

namespace HiveShard.Interface;

public interface IInitializer
{
    public Task Initialize();
}