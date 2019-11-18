using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetFrame.Tool
{
    public class EventExcuteUtil
    {
        public Mutex mutex;

        private static EventExcuteUtil ins;

        public static EventExcuteUtil Ins {
            get {
                if (ins == null) {
                    ins = new EventExcuteUtil();
                }
                return ins;
            }
        }

        public EventExcuteUtil() {
            mutex = new Mutex();
        }


        public void Excute(Action de) {
            lock (this) {
                mutex.WaitOne();
                de();
                mutex.ReleaseMutex();
            }
        }
    }
}
