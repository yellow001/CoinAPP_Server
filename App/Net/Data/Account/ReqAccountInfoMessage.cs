using NetFrame.EnDecode;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
[ProtoContract]
public class ReqAccountInfoMessage : BaseMessage
{
    /// <summary>
    /// 合约id，不填表示请求所有
    /// </summary>
    [ProtoMember(1)]
    public string V_Instrument_id;

    public static int V_Pid=100001;

    public override BaseMessage ReadData(byte[] msgBytes)
    {
        return AbsCoding.Ins.MsgDecoding<ReqAccountInfoMessage>(msgBytes);
    }

    public override byte[] WriteData()
    {
        return AbsCoding.Ins.MsgEncoding(this);
    }
}
