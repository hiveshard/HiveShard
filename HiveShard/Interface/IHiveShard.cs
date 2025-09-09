namespace HiveShard.Interface
{
    public interface IHiveShard
    {
        public void Process(float seconds);
        public void Initialize();
    }
}