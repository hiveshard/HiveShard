namespace HiveShard.Interface;

public interface IEventEmitterType
{
    public string Identity { get; }
    public bool EmitsFirstTickOnly { get; }
}