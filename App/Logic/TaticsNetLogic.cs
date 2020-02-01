using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsNetLogic
{
    public void Init() {
        NetCenter.Ins.AddMsgEvent<ReqTacticsInfoMessage>(ReqTacticsInfoMessage.V_Pid, ReqAccountInfoMessage_CB);

        //NetCenter.Ins.AddMsgEvent<ReqRunTacticsMessage>(ReqRunTacticsMessage.V_Pid, ReqRunTacticsMessage_CB);

        //NetCenter.Ins.AddMsgEvent<ReqOrderTacticsMessage>(ReqOrderTacticsMessage.V_Pid, ReqOrderTacticsMessage_CB);

        NetCenter.Ins.AddMsgEvent<ReqChangeOrderStateMessage>(ReqChangeOrderStateMessage.V_Pid, ReqChangeOrderStateMessage_CB);

        NetCenter.Ins.AddMsgEvent<ReqChangeTacticsStateMessage>(ReqChangeTacticsStateMessage.V_Pid, ReqChangeTacticsStateMessage_CB);
    }

    /// <summary>
    /// 客户端请求账户信息
    /// </summary>
    /// <param name="token"></param>
    /// <param name="msg"></param>
    public void ReqAccountInfoMessage_CB(BaseToken token, BaseMessage msg) {
        if (msg is ReqTacticsInfoMessage) {

            ResTacticsListMessage res = new ResTacticsListMessage();
            res.V_AccountInfoList = TaticsManager.GetIns().V_Model.GetTacticsInfo();

            NetCenter.Ins.Send(token, ResTacticsListMessage.V_Pid, res);
        }
    }

    ///// <summary>
    ///// 客户端请求账户信息
    ///// </summary>
    ///// <param name="token"></param>
    ///// <param name="msg"></param>
    //public void ReqRunTacticsMessage_CB(BaseToken token, BaseMessage msg)
    //{
    //    ReqRunTacticsMessage info = msg as ReqRunTacticsMessage;
    //    if (info != null) {
    //        int state = TaticsManager.GetIns().V_Model.F_ReqRunTactics(info.coin);
    //        string tip = state == 1 ? "成功" : "失败";
    //        NetCenter.Ins.SendTips(token, tip);
    //    }
    //}

    ///// <summary>
    ///// 客户端请求操作
    ///// </summary>
    ///// <param name="token"></param>
    ///// <param name="msg"></param>
    //public void ReqOrderTacticsMessage_CB(BaseToken token, BaseMessage msg)
    //{
    //    ReqOrderTacticsMessage info = msg as ReqOrderTacticsMessage;
    //    if (info != null)
    //    {
    //        int state = TaticsManager.GetIns().V_Model.F_ReqRunTactics(info.coin);
    //        string tip = state == 1 ? "成功" : "失败";
    //        NetCenter.Ins.SendTips(token, tip);
    //    }
    //}

    /// <summary>
    /// 客户端请求改变策略下单状态
    /// </summary>
    /// <param name="token"></param>
    /// <param name="msg"></param>
    public void ReqChangeOrderStateMessage_CB(BaseToken token, BaseMessage msg)
    {
        ReqChangeOrderStateMessage info = msg as ReqChangeOrderStateMessage;
        if (info != null)
        {
            int state = TaticsManager.GetIns().V_Model.F_ReqChangeOrderState(info.coin,info.state);
            string tip = state <=0 ? "失败":"成功";
            NetCenter.Ins.SendTips(token, tip);
        }
    }

    /// <summary>
    /// 客户端请求改变策略运行状态
    /// </summary>
    /// <param name="token"></param>
    /// <param name="msg"></param>
    public void ReqChangeTacticsStateMessage_CB(BaseToken token, BaseMessage msg)
    {
        ReqChangeTacticsStateMessage info = msg as ReqChangeTacticsStateMessage;
        if (info != null)
        {
            int state = TaticsManager.GetIns().V_Model.F_ReqOperation(info.coin, info.type);
            string tip = state <=0 ?  "失败": "成功";
            NetCenter.Ins.SendTips(token, tip);
        }
    }
}
