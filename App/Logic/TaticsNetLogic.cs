using System;
using System.Collections.Generic;
using System.Text;

public class TaticsNetLogic
{
    public void Init() {
        NetCenter.Ins.AddMsgEvent<ReqAccountInfoMessage>(ReqAccountInfoMessage.V_Pid, ReqAccountInfoMessage_CB);
    }

    public void ReqAccountInfoMessage_CB(BaseMessage msg) { 
        
    }
}
