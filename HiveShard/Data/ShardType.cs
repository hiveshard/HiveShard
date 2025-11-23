using HiveShard.Interface;

namespace HiveShard.Data
{
    public class ShardType
    {
        public ShardType(string name)
        {
            Name = name;
        }

        public static ShardType From<T>()
            where T : class, IHiveShard
        {
            return new ShardType(typeof(T).FullName!);
        }

        public string Name { get; }
    }
}