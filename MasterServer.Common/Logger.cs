using System;

namespace MasterServer.Common
{
    public class Logger
    {
        public delegate void Log(string text);

        public static Log DefaultLogger = Console.WriteLine;
    }
}