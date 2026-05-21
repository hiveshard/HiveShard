namespace HiveShard.Data;

public class Consumption<T>
{
    public Consumption(T message, long offset, Partition partition)
    {
        Message = message;
        Offset = offset;
        Partition = partition;
    }

    public T Message { get; }
    public long Offset { get; }
    public Partition Partition { get; }
}