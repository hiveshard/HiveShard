namespace HiveShard.Data;

public class Consumption<T>
{
    public Consumption(T message, long offset)
    {
        Message = message;
        Offset = offset;
    }

    public T Message { get; }
    public long Offset { get; }
}