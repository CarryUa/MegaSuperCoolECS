namespace ECS.Logs;

public static class Logger
{
    public static void LogInfo(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }
    public static void LogError(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[ERR] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }

    private static void PrintFancy(string msg, ConsoleColor color)
    {
        foreach (var str in msg.Split(" "))
        {
            if (str.Any(char.IsDigit))
            {
                Console.ForegroundColor = color;
                Console.Write($"{str} ");
                Console.ResetColor();
            }
            else
            {
                Console.Write($"{str} ");
            }

        }
        Console.Write("\n");
    }
}
