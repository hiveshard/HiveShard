using DotChaser.Interfaces;

namespace DotChaser.Tests.Providers;

public class TestOutputProvider: IOutputProvider
{
    public void Clear() { }
    public void Print(string text)
    {
        Console.Write(text);
    }
    public void NewLine()
    {
        Console.Write("\n");
    }
}