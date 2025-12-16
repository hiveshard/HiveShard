using System.Threading.Tasks;

namespace HiveShard;

public abstract class IsolatedEnvironment
{
    public abstract bool IsUnique { get; }
}