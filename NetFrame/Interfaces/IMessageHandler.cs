using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Interfaces
{
    public interface IMessageHandler
    {
        void OnClientConnected(BaseToken token);
        void OnClientClose(BaseToken token, string error);
    }
}
