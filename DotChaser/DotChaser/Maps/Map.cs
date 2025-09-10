namespace DotChaser.Maps
{
    public abstract class Map
    {
        public abstract int[] Grid { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
    }
}