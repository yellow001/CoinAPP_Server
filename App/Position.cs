using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

/// <summary>
/// 永续持仓信息
/// </summary>
public class Position
{
    /// <summary>
    /// 所属合约
    /// </summary>
    public string V_Instrument_id;
    /// <summary>
    /// 均价
    /// </summary>
    public float V_Avg_Price;
    /// <summary>
    /// 持仓量
    /// </summary>
    public float V_AllVol;
    /// <summary>
    /// 可平仓量
    /// </summary>
    public float V_AvailVol;
    /// <summary>
    /// 方向（ 大于0 多）
    /// </summary>
    public int V_Dir;
    /// <summary>
    /// 杠杆倍数
    /// </summary>
    public float V_Leverage;
    /// <summary>
    /// 下单时间
    /// </summary>
    public DateTime V_TimeStamp;

    public Position() { }

    public Position(string n, int d, float vol, float aVol, float p, float leverage, DateTime t)
    {
        V_Instrument_id = n;
        V_Dir = d;
        V_AllVol = vol;
        V_AvailVol = aVol;
        V_Avg_Price = p;
        V_Leverage = leverage;
        V_TimeStamp = t;
    }

    public Position(DataRow data) {
        RefreshData(data);
    }

    public void RefreshData(DataRow data) {
        V_Instrument_id = data["instrument_id"].ToString();
        V_Avg_Price = float.Parse(data["avg_cost"].ToString());
        V_AllVol = float.Parse(data["position"].ToString());
        V_AvailVol = float.Parse(data["avail_position"].ToString());
        V_Dir = data["side"].ToString().Equals("long") ? 1 : -1;
        V_Leverage = float.Parse(data["leverage"].ToString());
        V_TimeStamp = DateTime.Parse(data["timestamp"].ToString());
    }

    public float GetPercent(float price) {
        if (V_Dir > 0)
        {
            return ((price - V_Avg_Price) / price) * V_Leverage * 100;
        }
        else {
            return ((V_Avg_Price - price) / price) * V_Leverage * 100;
        }
    }

    public float GetPercentTest(KLine line)
    {
        if (V_Dir > 0)
        {
            if (V_Avg_Price > line.V_LowPrice) { 
                return ((line.V_LowPrice - V_Avg_Price) / line.V_LowPrice) * V_Leverage * 100;
            }
            return ((line.V_ClosePrice - V_Avg_Price) / line.V_ClosePrice) * V_Leverage * 100;
        }
        else
        {
            if(V_Avg_Price < line.V_HightPrice){
                return ((V_Avg_Price - line.V_HightPrice) / line.V_HightPrice) * V_Leverage * 100;
            }
            return ((V_Avg_Price - line.V_ClosePrice) / line.V_ClosePrice) * V_Leverage * 100;
        }
    }


    public static List<Position> GetPositionList(string json) {
        List<Position> list = new List<Position>();
        DataTable t = JsonConvert.DeserializeObject<DataTable>(json);
        foreach (DataRow dr in t.Rows)
        {
            Position p = new Position(dr);
            if (p.V_AllVol > 0) {
                list.Add(new Position(dr));
            }
        }

        return list;
    }
}
