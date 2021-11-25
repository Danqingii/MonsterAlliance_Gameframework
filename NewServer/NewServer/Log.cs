using System;
using System.Collections.Generic;

public static class Log
{
    public static void Debug(string log)
    {
        Console.WriteLine(string.Format("Server {0}: {1}",DateTime.Now,log));
    }

    public static void Error(string log)
    {
        Console.WriteLine(string.Format("Server {0}: Error:{1}", DateTime.Now, log));
    }
}
