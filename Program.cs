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
            // Instantiate EntitySystemManager
            var sysMan = new EntitySystemManager();
            sysMan.InitAllSystems(true).GetAwaiter().GetResult();

            // Force print messages from initializing
            Logger.PrintQueue();

            using (MyWindow window = new(1300, 700, sysMan.UpdateAll))
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