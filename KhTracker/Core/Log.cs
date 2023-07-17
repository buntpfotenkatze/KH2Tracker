using System.IO;

namespace KhTracker;

public class Log
{
    private readonly StreamWriter writer;

    public Log(string path)
    {
        writer = new StreamWriter(path);
        writer.AutoFlush = true;
    }

    public void Close()
    {
        writer.Close();
    }

    public void Record(string text)
    {
        writer.WriteLine(text);
    }

    public void RecordWorld(string text)
    {
        writer.WriteLine("");
        writer.WriteLine("Entered " + text);
        writer.WriteLine("----------------------");
    }
}
