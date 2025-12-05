namespace HiveShard.Client.Data;

public class ClientIsolatedEnvironment: IsolatedEnvironment
{
    internal ClientIsolatedEnvironment(string username)
    {
        Username = username;
    }

    public string Username { get; }
}