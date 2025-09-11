namespace DotChaser.Events
{
    public class InitialGrid
    {
        public int[] Grid { get; }
        public int Height { get; }
        public int Width { get; }

        public InitialGrid(int[] grid, int height, int width)
        {
            Grid = grid;
            Height = height;
            Width = width;
        }
    }
}