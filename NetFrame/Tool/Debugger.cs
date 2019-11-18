//using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Tool
{
    public class Debugger
    {
        //static Logger Log;

        static Debugger() {
            //Log = LogManager.GetCurrentClassLogger();
        }

        public static void Trace(string msg) {
            //Log.Trace(msg);
        }

        public static void Warn(string msg) {
            //Log.Warn(msg);
        }

        public static void Error(string msg) {
            //Log.Error(msg);
        }
    }
}
