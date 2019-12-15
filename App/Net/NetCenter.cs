using NetFrame.AbsClass;
using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;

public class NetCenter : AbsHandlerCenter
{
    public override void OnClientClose(BaseToken token, string error)
    {
        throw new NotImplementedException();
    }

    public override void OnClientConnent(BaseToken token)
    {
        throw new NotImplementedException();
    }

    public override void OnMsgReceive<T>(BaseToken token, T model)
    {
        throw new NotImplementedException();
    }
}