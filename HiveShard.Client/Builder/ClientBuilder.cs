using HiveShard.Client.Data;

namespace HiveShard.Client.Builder;

public class ClientBuilder
{
    private string _username;

    internal IsolatedEnvironment Build()
    {
        return new ClientIsolatedEnvironment(_username);
    }

    public ClientBuilder Identify(string username)
    {
        _username = username;
        return this;
    }
}