using HiveShard.Data;
using HiveShard.Initializer.Interfaces;
using HiveShard.Workers.Initialization.Tests.Config;
using HiveShard.Workers.Initialization.Tests.Events;

namespace HiveShard.Workers.Initialization.Tests.Initializer;

public class DependentInitializer: IInitializer
{
    private SecretDependencyConfig _secretDependencyConfig;

    public DependentInitializer(SecretDependencyConfig secretDependencyConfig)
    {
        _secretDependencyConfig = secretDependencyConfig;
    }

    public static readonly Chunk PublishedChunk = new (0, 0);
    public void Initialize(IInitializationTunnel tunnel)
    {
        tunnel.Send(new DependencySecretEvent(_secretDependencyConfig.Secret),PublishedChunk);
    }
}