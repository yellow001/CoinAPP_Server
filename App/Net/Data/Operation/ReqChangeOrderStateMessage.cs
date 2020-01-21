using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 改变下单策略状态
/// </summary>
[Serializable]
[ProtoContract]
public class ReqChangeOrderStateMessage : BaseMessage
{
    public static int V_Pid = 100006;

    /// <summary>
    /// 币种
    /// </summary>
    [ProtoMember(1)]
    public string coin;

    /// <summary>
    /// 操作类型 （ 1:平空   -1:平多   0:全平   2:对冲）
    /// </summary>
    [ProtoMember(2)]
    public int state;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqChangeOrderStateMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}
