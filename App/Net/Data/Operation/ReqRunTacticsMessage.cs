using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 运行策略
/// </summary>
[Serializable]
[ProtoContract]
public class ReqRunTacticsMessage : BaseMessage
{
    public static int V_Pid = 100003;

    /// <summary>
    /// 币种
    /// </summary>
    [ProtoMember(1)]
    public string coin; 

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqRunTacticsMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}
