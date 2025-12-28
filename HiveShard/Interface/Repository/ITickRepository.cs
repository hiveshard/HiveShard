namespace HiveShard.Interface.Repository
{
    public interface ITickRepository
    {
        public long GetLatestTick();
        public void SetLatestTick(long newTick);
    }
}