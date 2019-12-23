using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

[Serializable]
[ProtoContract]
public abstract class BaseMessage
{
    public abstract BaseMessage ReadData(byte[] msgBytes);

    public abstract byte[] WriteData();
}
