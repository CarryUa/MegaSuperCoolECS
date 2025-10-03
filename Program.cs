using System.Diagnostics;
using System.Threading.Tasks;
using ECS.Logs;
using ECS.Systems;
using MyOpenTKWindow;
class Program
{
    public static void Main()
    {
        try
        {

            // Instantiate EntSys
            var sysMan = new EntSysManager();
            sysMan.InitAllSystems().GetAwaiter().GetResult();

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