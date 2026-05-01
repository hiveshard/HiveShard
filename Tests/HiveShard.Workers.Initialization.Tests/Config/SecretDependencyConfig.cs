namespace HiveShard.Workers.Initialization.Tests.Config;

public class SecretDependencyConfig
{
    public SecretDependencyConfig(int secret)
    {
        Secret = secret;
    }

    public int Secret { get; }
}