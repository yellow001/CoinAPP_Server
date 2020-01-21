using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 修改策略状态
/// </summary>
[Serializable]
[ProtoContract]
public class ReqChangeTacticsStateMessage : BaseMessage
{
    public static int V_Pid = 100007;

    /// <summary>
    /// 币种
    /// </summary>
    [ProtoMember(1)]
    public string coin;

    /// <summary>
    /// 操作类型 （ 1:平空   -1:平多   0:全平   2:对冲）
    /// </summary>
    [ProtoMember(2)]
    public int type;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqChangeTacticsStateMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}