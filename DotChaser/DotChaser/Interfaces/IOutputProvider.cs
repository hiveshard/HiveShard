namespace DotChaser.Interfaces
{
    public interface IOutputProvider
    {
        public void Clear();
        public void Print(string text);
        public void NewLine();
    }
}