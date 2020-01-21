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
    Dictionary<int,Action<BaseToken,BaseMessage>> m_MsgEventDic = new Dictionary<int, Action<BaseToken,BaseMessage>>();

    SingleSender SingleSender = new SingleSender();

    List<BaseToken> tokenList = new List<BaseToken>();

    public NetCenter()
    {
        TaticsManager.GetIns();
    }

    public void AddMsgEvent<T>(int pid,Action<BaseToken,BaseMessage> cb)where T:BaseMessage{
        m_MsgTypeDic[pid] = typeof(T);
        m_MsgEventDic[pid] = cb;
    }

    public override void OnClientClose(BaseToken token, string error)
    {
        //throw new NotImplementedException();
        Console.WriteLine("{0}  client connect :{1}",DateTime.Now,token.socket.RemoteEndPoint);
        if (tokenList.Contains(token)) {
            tokenList.Remove(token);
        }
    }

    public override void OnClientConnent(BaseToken token)
    {
        Console.WriteLine("{0}  client close :{1}", DateTime.Now, token.socket.RemoteEndPoint);
        //throw new NotImplementedException();
    }

    public override void OnMsgReceive<T>(BaseToken token, T model)
    {
        //throw new NotImplementedException();

        if (!tokenList.Contains(token)) {
            try
            {
                string key = Convert.ToBase64String(Encoding.UTF8.GetBytes("yellowzhong"));
                if (model.GetMsg<string>().Equals(key)) {
                    tokenList.Add(token);
                }
            }
            catch (Exception ex)
            {
                token.Close();
                Console.WriteLine("非法连接  "+token.socket.RemoteEndPoint);
                return;
            }
        }

        if (!tokenList.Contains(token)) { return; }

        try
        {
            if (m_MsgEventDic.ContainsKey(model.pID) && m_MsgTypeDic.ContainsKey(model.pID))
            {
                BaseMessage msg = Activator.CreateInstance(m_MsgTypeDic[model.pID]) as BaseMessage;
                m_MsgEventDic[model.pID](token,msg.ReadData(model.msgBytes));
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

    public void Send<T>(BaseToken token,int pid, T msg,int area=0) {
        SingleSender.Send(token, pid, area, msg);
    }

    public void SendTips(BaseToken token, string tips) {
        ResTipsMessage msg = new ResTipsMessage();
        msg.tips = tips;
        SingleSender.Send(token, ResTipsMessage.V_Pid, 0, msg);
    }
}