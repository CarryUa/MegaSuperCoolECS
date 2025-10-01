using System.Diagnostics;
using ECS.Systems;
using MyOpenTKWindow;
class Program
{
    public static void Main()
    {
        // Instantiate EntSys
        var sysMan = new EntSysManager();
        sysMan.InitAllSystems(true);

        // Start timer
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lastTicks = stopwatch.ElapsedTicks;
        double tickFrequency = Stopwatch.Frequency;

        using (MyWindow window = new(1300, 700, sysMan.UpdateAll))
        {
            window.Run();
        }
    }
}