namespace HiveShard.Interface.Providers
{
    public interface ITickRepository
    {
        public long GetLatestTick();
        public void SetLatestTick(long newTick);
    }
}