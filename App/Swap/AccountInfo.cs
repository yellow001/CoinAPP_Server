using Newtonsoft.Json.Linq;
using OKExSDK;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

[Serializable]
[ProtoContract]
/// <summary>
/// 永续账号信息
/// </summary>
public class AccountInfo
{
    /// <summary>
    /// 所属合约
    /// </summary>
    [ProtoMember(1)]
    public string V_Instrument_id;
    /// <summary>
    /// 动态权益（可用+已用+挂单冻结 资金）
    /// </summary>
    [ProtoMember(2)]
    public float V_Equity;
    /// <summary>
    /// 已用保证金
    /// </summary>
    [ProtoMember(3)]
    public float V_Margin;
    /// <summary>
    /// 开仓冻结保证金
    /// </summary>
    [ProtoMember(4)]
    public float V_Margin_frozen;
    /// <summary>
    /// 更新时间戳
    /// </summary>
    [ProtoMember(5)]
    public DateTime V_TimeStamp;
    /// <summary>
    /// 当前价格
    /// </summary>
    [ProtoMember(6)]
    public float V_CurPrice;

    /// <summary>
    /// 持仓信息（多 空）
    /// </summary>
    [ProtoMember(7)]
    public List<Position> V_Positions;

    /// <summary>
    /// 当前持仓
    /// </summary>
    public Position V_Position {
        get {
            if (V_Positions != null && V_Positions.Count >= 1) {
                return V_Positions[0];
            }
            return null;
        }
    }

    public AccountAPIKey V_APIKey;

    /// <summary>
    /// 杠杆倍数
    /// </summary>
    [ProtoMember(8)]
    public float V_Leverage;

    /// <summary>
    /// 合约面值
    /// </summary>
    public float V_Contract_val;

    public AccountInfo() { }

    public AccountInfo(string json)
    {
        RefreshData(json);
    }

    public void RefreshData(string json) {
        JObject obj = JObject.Parse(json);
        V_Instrument_id = obj["instrument_id"].ToString();
        V_Equity = float.Parse(obj["equity"].ToString());
        V_Margin = float.Parse(obj["margin"].ToString());
        V_Margin_frozen = float.Parse(obj["margin_frozen"].ToString());
        V_TimeStamp = DateTime.Parse(obj["timestamp"].ToString());
        if (V_Positions == null)
        {
            V_Positions = new List<Position>();
        }
        V_Positions.Clear();
    }

    /// <summary>
    /// 设置持仓信息
    /// </summary>
    /// <param name="positionList"></param>
    public void RefreshPositions(List<Position> positionList) {
        V_Positions.Clear();
        V_Positions.AddRange(positionList);
    }

    /// <summary>
    /// 对手价平仓
    /// </summary>
    public async Task ClearPositions() {
        if (V_Positions == null || V_Positions.Count == 0) {
            return;
        }

        Console.WriteLine("{0} {1}:  平仓: price {2}，方向：{3}，盈利率{4},剩余 {5}",
            DateTime.Now,
            V_Instrument_id,
            V_CurPrice,
            V_Position.V_Dir > 0 ? "平多" : "平空",
            V_Position.GetPercent(V_CurPrice),
            GetAvailMoney());

        for (int i = 0; i < V_Positions.Count; i++)
        {
            Position p = V_Positions[i];
            SwapApi api = CommonData.Ins.V_SwapApi;
            await api.makeOrderAsync(V_Instrument_id, p.V_Dir > 0 ? "3" : "4",(decimal)V_CurPrice, (int)p.V_AvailVol, "",0, "1");
        }

        JObject obj = await CommonData.Ins.V_SwapApi.getAccountsByInstrumentAsync(V_Instrument_id);
        RefreshData(obj["info"].ToString());
    }

    /// <summary>
    /// 对手价开仓
    /// </summary>
    /// <param name="dir">方向 >0 多</param>
    /// <param name="vol">金额</param>
    /// <returns></returns>
    public async Task<bool> MakeOrder(int dir,float vol) {
        if (vol > GetAvailMoney())
        {
            return false;
        }
        else {
            //获取张数(BTC 1张=100USD EOS 1张=10USD)
            int v = (int)((vol * V_CurPrice* V_Leverage) / V_Contract_val);
            SwapApi api = CommonData.Ins.V_SwapApi;
            await api.makeOrderAsync(V_Instrument_id, dir > 0 ? "1" : "2", (decimal)V_CurPrice, v, "", 0, "1");
            Console.WriteLine("{0}  {1}:  开仓:{2} 价格:{3} 张数:{4}",DateTime.Now,V_Instrument_id,dir > 0 ? "多" : "空", V_CurPrice,v);
            return true;
        }
    }

    /// <summary>
    /// 撤销所有未成交订单
    /// </summary>
    public async Task ClearOrders() {
        //获取所有未成交订单
        SwapApi api = CommonData.Ins.V_SwapApi;
        JObject obj = await api.getOrdersAsync(V_Instrument_id,"6",null,null,null);
        string s = obj["order_info"].ToString();
        List<Order> orders = Order.GetOrderList(s);
        foreach (var item in orders)
        {
            await api.cancelOrderAsync(V_Instrument_id, item.Order_id);
        }
    }

    /// <summary>
    /// 获取可用金额
    /// </summary>
    /// <returns></returns>
    public float GetAvailMoney()
    {
        return V_Equity - V_Margin - V_Margin_frozen;
    }

    public static AccountInfo GetAccount(string json)
    {
        return new AccountInfo(json);
    }
}
