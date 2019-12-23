using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
[ProtoContract]
public class ResAccountInfoMessage : BaseMessage
{
    [ProtoMember(1)]
    public List<AccountInfo> V_AccountInfoList;

    public static int pid = 100002;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqAccountInfoMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}