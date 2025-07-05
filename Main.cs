using System.Windows;

namespace sambar;

public class Program
{
    [STAThread]
    static void Main()
    {
        Application app = new();
        Sambar sambar = new(); 
        app.Run(sambar);
    }
}
