using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.AbsClass
{
    public abstract class AbsHandlerCenter
    {
        public abstract void OnClientConnent(BaseToken token);
        public abstract void OnMsgReceive<T>(BaseToken token, T model)where T:TransModel;
        public abstract void OnClientClose(BaseToken token, string error);
    }
}
