using System.Collections.Concurrent;

namespace ECS.Logs;

internal enum LogType : byte
{
    Info,
    Debug,
    Error,
    // Fatal is printed without the queue, because it kills the program.
}

internal struct LogMessage(object? msg, bool fancy, ConsoleColor color, LogType type)
{
    public object? Message = msg;
    public bool Fancy = fancy;
    public ConsoleColor FancyColor = color;
    public LogType LogType = type;
}

public static class Logger
{
    private static readonly ConcurrentQueue<LogMessage> _logQueue = new();

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogInfo(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        _logQueue.Enqueue(new(msg, fancy, color, LogType.Info));
    }

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogDebug(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        _logQueue.Enqueue(new(msg, fancy, color, LogType.Debug));
    }

    /// <summary>
    /// Logs a Error message to the console.
    /// </summary>
    /// <param name="msg">The message to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogError(object? msg, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        _logQueue.Enqueue(new(msg, fancy, color, LogType.Error));
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
        throw new Exception(msg?.ToString());
    }

    /// <summary>
    /// Logs a Fatal error message to the console.
    /// </summary>
    /// <remarks>
    /// This will terminate the program after logging the message!
    /// </remarks>
    /// <param name="msg">The exception to be printed</param>
    /// <param name="fancy">If true, any numeric values and trailing strings will be printed in the specified color</param>
    /// <param name="color">The color to print numeric values and trailing strings. Defaults to DarkBlue.</param>
    public static void LogFatal(Exception ex, bool fancy = false, ConsoleColor color = ConsoleColor.DarkBlue)
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write("[FATAL] ");
        Console.ResetColor();
        if (fancy)
            PrintFancy($"{ex.Message}", color);
        else
            Console.Write(ex.Message + "\n");
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

    public static void PrintQueue()
    {
        void PrintWithPrefix(string prefix, LogMessage log)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[INFO] ");
            Console.ResetColor();
            if (log.Fancy)
                PrintFancy($"{log.Message}", log.FancyColor);
            else
                Console.Write(log.Message + "\n");
        }

        // print all the messages in queue
        while (_logQueue.TryDequeue(out var log))
        {
            // Null check to prevent empty messages
            if (string.IsNullOrWhiteSpace($"{log.Message}"))
            {
                Logger.LogError("Tried to print empty object!");
                continue;
            }


            // Print different color depending on type.
            switch (log.LogType)
            {
                case LogType.Info:
                    {
                        PrintWithPrefix("[INFO]", log);
                        break;
                    }
                case LogType.Debug:
                    {
                        PrintWithPrefix("[DEBUG]", log);
                        break;
                    }
                case LogType.Error:
                    {
                        PrintWithPrefix("[ERROR]", log);
                        break;
                    }
            }
        }
    }
}
