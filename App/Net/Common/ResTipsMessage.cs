using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
[ProtoContract]
public class ResTipsMessage : BaseMessage
{
    public static int V_Pid = 100004;

    [ProtoMember(1)]
    public string tips;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ResTipsMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}
