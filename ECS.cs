
public static class Logger
{
    public static void LogInfo(object? msg, bool fancy = false, ConsoleColor color = new ConsoleColor())
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[INFO] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }
    public static void LogError(object? msg, bool fancy = false, ConsoleColor color = new ConsoleColor())
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
            try
            {
                Convert.ToDouble($"{str} ");
                Console.ForegroundColor = color;
                Console.Write($"{str} ");
                Console.ResetColor();
            }
            catch
            {
                Console.Write($"{str} ");
            }

        }
        Console.Write("\n");
    }
}
