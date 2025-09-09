namespace HiveShard.Interface
{
    public interface ITickRepository
    {
        public long GetLatestTick();
        public void SetLatestTick(long newTick);
    }
}