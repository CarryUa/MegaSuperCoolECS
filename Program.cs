using ECS.Logs;
using ECS.System;
using MyOpenTKWindow;

[NeedDependencies]
class Program
{
    public static void Main()
    {
        try
        {
            MyWindow window = new();
            // Instantiate EntitySystemManager
            var sysMan = new EntitySystemManager(window);
            sysMan.InitAllSystems(true).GetAwaiter().GetResult();

            window.Init(1000, 1000);

            // Force print messages from initializing
            Logger.PrintQueue();

            using (window)
            {
                window.Run();
            }
        }
        catch (Exception ex)
        {
            Logger.LogFatal(ex, true, ConsoleColor.DarkRed);
            throw;
        }
    }
}