namespace ECS.Logs;

public static class Logger
{
    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
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

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogDebug(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write("[DEBUG] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{msg}", color);
        else
            Console.Write(msg + "\n");
    }

    /// <summary>
    /// Logs a Error message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
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

    /// <summary>
    /// Logs a Fatal error message to the console.
    /// </summary>
    /// <remarks>
    /// This will terminate the program after logging the message!
    /// </remarks>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogFatal(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("[FATAL] ");
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
