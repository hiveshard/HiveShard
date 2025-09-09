using HiveShard.Interface;

namespace HiveShard.Repository
{
    public class TickRepository: ITickRepository
    {
        private long _latestTick;
        public long GetLatestTick() => _latestTick;

        public void SetLatestTick(long newTick)
        {
            _latestTick = newTick;
        }
    }
}