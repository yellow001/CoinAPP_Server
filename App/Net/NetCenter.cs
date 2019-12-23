using NetFrame.AbsClass;
using NetFrame.Base;
using NetFrame.EnDecode;
using System;
using System.Collections.Generic;
using System.Text;

public class NetCenter : AbsHandlerCenter
{
    static NetCenter ins;

    public static NetCenter Ins {
        get {
            if (ins == null)
            {
                ins = new NetCenter();
            }
            return ins;
        }
    }

    Dictionary<int, Type> m_MsgTypeDic = new Dictionary<int, Type>();
    Dictionary<int,Action<BaseMessage>> m_MsgEventDic = new Dictionary<int, Action<BaseMessage>>();

    public NetCenter()
    {
        TaticsManager.GetIns();
    }

    public void AddMsgEvent<T>(int pid,Action<BaseMessage> cb)where T:BaseMessage{
        m_MsgTypeDic[pid] = typeof(T);
        m_MsgEventDic[pid] = cb;
    }

    public override void OnClientClose(BaseToken token, string error)
    {
        //throw new NotImplementedException();
        Console.WriteLine("{0}  client connect :{1}",DateTime.Now,token.socket.RemoteEndPoint);
    }

    public override void OnClientConnent(BaseToken token)
    {
        Console.WriteLine("{0}  client close :{1}", DateTime.Now, token.socket.RemoteEndPoint);
        //throw new NotImplementedException();
    }

    public override void OnMsgReceive<T>(BaseToken token, T model)
    {
        //throw new NotImplementedException();
        try
        {
            if (m_MsgEventDic.ContainsKey(model.pID) && m_MsgTypeDic.ContainsKey(model.pID))
            {
                BaseMessage msg = Activator.CreateInstance(m_MsgTypeDic[model.pID]) as BaseMessage;
                m_MsgEventDic[model.pID](msg.ReadData(model.msgBytes));
            }
            else
            {
                //Console.WriteLine("");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        
    }
}