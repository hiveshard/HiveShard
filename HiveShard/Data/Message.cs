namespace HiveShard.Data;

public class Message<T>
{
    public Message(T payload, Chunk chunk)
    {
        Payload = payload;
        Chunk = chunk;
    }

    public T Payload { get; }
    public Chunk Chunk { get; }
}