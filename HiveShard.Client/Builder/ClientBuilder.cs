using HiveShard.Client.Data;
using HiveShard.Data;

namespace HiveShard.Client.Builder;

public class ClientBuilder
{
    private HiveShardClient _client;

    internal IsolatedEnvironment Build()
    {
        return new ClientIsolatedEnvironment(_client);
    }

    public ClientBuilder Identify(HiveShardClient username)
    {
        _client = username;
        return this;
    }
}