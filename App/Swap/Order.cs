using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

/// <summary>
/// 永续订单
/// </summary>
public class Order
{
    /// <summary>
    /// 所属合约
    /// </summary>
    public string V_Instrument_id;
    /// <summary>
    /// 客户端订单ID
    /// </summary>
    public string V_Client_oid;
    /// <summary>
    /// 真*订单ID(服务端存的)
    /// </summary>
    public string Order_id;
    /// <summary>
    /// 委托数量
    /// </summary>
    public float V_AllVol;
    /// <summary>
    /// 成交数量
    /// </summary>
    public float V_Filled_Vol;
    /// <summary>
    /// 委托价格
    /// </summary>
    public float V_Price;
    /// <summary>
    /// 成交均价
    /// </summary>
    public float V_Price_avg;
    /// <summary>
    /// 方向（1 开多  2 开空  3 平多  4 平空）
    /// </summary>
    public int V_Dir;
    /// <summary>
    /// 订单类型（0：普通委托  1：只做Maker（Post only）  2：全部成交或立即取消（FOK）  3：立即成交并取消剩余（IOC））
    /// </summary>
    public int V_OrderType;
    /// <summary>
    /// 状态（-2:失败  -1:撤单成功  0:等待成交  1:部分成交  2:完全成交  3:下单中  4:撤单中）
    /// </summary>
    public int V_State;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime V_TimeStamp;


    public Order() { }

    public Order(DataRow data) {
        RefreshData(data);
    }

    public void RefreshData(DataRow data) {
        V_Instrument_id = data["instrument_id"].ToString();
        V_Client_oid = data["client_oid"].ToString();
        Order_id = data["order_id"].ToString();
        V_AllVol = float.Parse(data["size"].ToString());
        V_Filled_Vol = float.Parse(data["filled_qty"].ToString());
        V_Price = float.Parse(data["price"].ToString());
        V_Price_avg = float.Parse(data["price_avg"].ToString());
        V_Dir = int.Parse(data["type"].ToString());
        V_OrderType = int.Parse(data["order_type"].ToString());
        V_State = int.Parse(data["state"].ToString());
        V_TimeStamp = DateTime.Parse(data["timestamp"].ToString());
    }

    public static List<Order> GetOrderList(string json)
    {
        List<Order> list = new List<Order>();
        DataTable t = JsonConvert.DeserializeObject<DataTable>(json);
        foreach (DataRow dr in t.Rows)
        {
            list.Add(new Order(dr));
        }

        return list;
    }
}
