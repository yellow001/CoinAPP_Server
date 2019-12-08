using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class KLine
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime V_Timestamp;
    /// <summary>
    /// 开盘价格
    /// </summary>
    public float V_OpenPrice;
    /// <summary>
    /// 最高价
    /// </summary>
    public float V_HightPrice;
    /// <summary>
    /// 最低价
    /// </summary>
    public float V_LowPrice;
    /// <summary>
    /// 收盘价
    /// </summary>
    public float V_ClosePrice;
    /// <summary>
    /// 交易量
    /// </summary>
    public float V_Vol;

    public KLine() { }

    public void SetData(DateTime d, float oPrice, float hPrice, float lPrice, float cPrice, float v) {
        V_Timestamp = d;
        V_OpenPrice = oPrice;
        V_HightPrice = hPrice;
        V_LowPrice = lPrice;
        V_ClosePrice = cPrice;
        V_Vol = v;
    }

    public void SetData(string d, string oPrice, string hPrice, string lPrice, string cPrice, string v) {
        DateTime date = DateTime.Parse(d);
        SetData(date, float.Parse(oPrice), float.Parse(hPrice), float.Parse(lPrice), float.Parse(cPrice), float.Parse(v));
    }

    public void SetData(List<string> content) {
        if (content != null && content.Count >= 6) {
            SetData(content[0], content[1], content[2], content[3], content[4], content[5]);
        }
    }

    public static List<KLine> GetListFormJContainer(JContainer jcontainer)
    {
        List<KLine> result = new List<KLine>();
        JToken temp = jcontainer.First;
        while (temp!=null)
        {
            List<string> content = temp.ToObject<List<string>>();
            KLine line = new KLine();
            line.SetData(content);
            result.Add(line);
            temp = temp.Next;
        }
        return result;
    }

    /// <summary>
    /// 获取均价
    /// </summary>
    /// <returns></returns>
    public float GetAvg() {
        return MathF.Abs(V_OpenPrice - V_ClosePrice) / 2+(V_OpenPrice>V_ClosePrice?V_ClosePrice:V_OpenPrice);
    }

    /// <summary>
    /// 获取波动 百分比值（开盘 与 收盘 价差）
    /// </summary>
    /// <returns></returns>
    public float GetPercent() {
        return Math.Abs(V_HightPrice - V_LowPrice) / GetAvg() * 100;
    }
}