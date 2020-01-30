using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Tool
{
    public class Debugger
    {
        static Logger Logger;

        static Debugger()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public static void Log(string msg)
        {
            Logger.Trace(msg);
        }

        public static void Warn(string msg)
        {
            Logger.Warn(msg);
        }

        public static void Error(string msg)
        {
            Logger.Error(msg);
        }
    }
}
