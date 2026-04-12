namespace HiveShard.Interface.Repository;

public interface ITickRepository
{
    public void SetLatestTick(long newTick);
}