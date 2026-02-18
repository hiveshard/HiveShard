using System.Net.Sockets;
using System.Threading;

namespace HiveShard.Edge;

public class ConnectedClient
{

    public ConnectedClient(TcpClient tcpClient, CancellationTokenSource cancellationTokenSource, NetworkStream stream)
    {
        Stream = stream;
        TcpClient = tcpClient;
        CancellationTokenSource = cancellationTokenSource;
    }

    public NetworkStream Stream { get; }
    public TcpClient TcpClient { get; }
    public CancellationTokenSource CancellationTokenSource { get; }
}