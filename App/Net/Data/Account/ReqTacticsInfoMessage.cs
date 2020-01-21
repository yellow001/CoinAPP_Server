using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 请求账户信息
/// </summary>
[Serializable]
[ProtoContract]
public class ReqTacticsInfoMessage : BaseMessage
{
    public static int V_Pid=100001;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqTacticsInfoMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}
