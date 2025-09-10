using DotChaser.Interfaces;

namespace DotChaser.Client.Providers;

public class ClientOutputProvider: IOutputProvider
{
    public void Clear()
    {
        Console.Clear();
    }

    public void Print(string text)
    {
        Console.Write(text);
    }

    public void NewLine()
    {
        Console.Write("\n");
    }
}