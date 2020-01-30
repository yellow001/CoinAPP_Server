/**
 * EMA 多空头排列
 * V1.0 2019-11-27
 * 
 * 
 * **/

using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// EMA 多空头排列
/// </summary>
public class EMATaticsHelper2 : BaseTaticsHelper, ICycleTatics
{
    /// <summary>
    /// 采样点
    /// </summary>
    public int V_Length = 5;

    /// <summary>
    /// 周期
    /// </summary>
    public List<int> V_CycleList = new List<int>() { 5, 10, 20 };

    /// <summary>
    /// 最近K线缓存
    /// </summary>
    public List<KLine> kLineCache = new List<KLine>();

    #region 重载
    /// <summary>
    /// 初始化设置 合约;K线时长;采样点;周期(小_中_大);倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        Console.WriteLine(setting);
        Debugger.Warn(setting);
        string[] strs = setting.Split(';');
        if (strs.Length >= 4)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            V_Length = int.Parse(strs[2]);
            V_Leverage = float.Parse(strs[3]);
        }
        base.Init(setting);
    }

    /// <summary>
    /// 刷新历史数据
    /// </summary>
    public override async Task RunHistory()
    {
        await base.RunHistory();

        Console.WriteLine(V_Instrument_id+":分析结果");
        Debugger.Warn(V_Instrument_id + ":分析结果");

        TaticsTestRunner.TestRun(this);

        Console.WriteLine(V_Instrument_id+":分析历史数据完毕");
        Debugger.Warn(V_Instrument_id + ":分析历史数据完毕");
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder()
    {
        return GetSign(true);
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    protected override bool OnShouldCloseOrder(int dir, float percent,bool isTest=false)
    {
        int sign = GetSign();

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            if (percent >= winPercent)
            {
                //达到 止盈 或后亏损时，判断继续持有还是平仓
                if (dir > 0)
                {
                    if (sign < 0)
                    {
                        //有较为强烈的空头排列信号
                        //Console.WriteLine("result {0}", GetResult());
                        return true;
                    }
                }
                else
                {
                    if (sign > 0)
                    {
                        //有较为强烈的多头排列信号
                        //Console.WriteLine("result {0}", GetResult());
                        return true;
                    }
                }
            }

            DateTime t = DateTime.UtcNow;

            if (isTest) {
                t = V_Cache.V_KLineData[0].V_Timestamp;
            }
            if ((t - V_LastOpTime).TotalMinutes > AppSetting.Ins.GetInt("ForceOrderTime")*V_Min) {
                //持仓时间有点久了，看机会溜吧
                if (dir > 0)
                {
                    if (sign < 0)
                    {
                        //有较为强烈的空头排列信号
                        //Console.WriteLine("result {0}", GetResult());
                        return true;
                    }
                }
                else
                {
                    if (sign > 0)
                    {
                        //有较为强烈的多头排列信号
                        //Console.WriteLine("result {0}", GetResult());
                        return true;
                    }
                }
            }


        }
        return false;
    }

    #endregion

    #region 策略方法
    public float GetResult()
    {
        return GetValue(1) - GetValue(-1);
    }

    /// <summary>
    /// 获取信号
    /// </summary>
    /// <param name="order">下单 or 止损</param>
    /// <returns></returns>
    int GetSign(bool order = false)
    {
        //if (order)
        //{
        //    kLineCache.Clear();

        //    if (V_Cache.V_KLineData.Count < 10)
        //    {
        //        return 0;
        //    }

        //    kLineCache = V_Cache.V_KLineData.GetRange(0, 10);

        //    foreach (var item in kLineCache)
        //    {
        //        //其中一条k线波动大于 1.382% ,先不开单
        //        if (item.GetPercent() > 1.382)
        //        {
        //            return 0;
        //        }
        //    }
        //}

        float result = GetResult();
        int final = (MathF.Abs(result) > 0.01f?(result>0?1:-1):0);

        if (final > 0)
        {
            //要开多
            if (GetEMA60_KValue() < 0) {
                //ema60 还在下降 不开单吧
                final = 0;
            }

        }
        else if (final < 0) {
            //要开空
            if (GetEMA60_KValue() > 0)
            {
                //ema60 还在上升 不开单吧
                final = 0;
            }

        }

        return final;
    }

    /// <summary>
    /// 获取 EMA 值
    /// </summary>
    /// <param name="index">下标</param>
    /// <returns></returns>
    float GetEMAValue(int length, int index = 0)
    {
        if (V_Cache == null)
        {
            return 0;
        }

        if (V_Cache.V_KLineData.Count < length + index)
        {
            return 0;
        }

        return EMA.GetEMA(length, V_Cache.V_KLineData.GetRange(index, length));
    }

    float GetEMA60_KValue() {
        if (GetEMAValue(60, 0) > GetEMAValue(60, 1) && GetEMAValue(60, 1) > GetEMAValue(60, 2)) {
            return 1;
        }
        else if (GetEMAValue(60, 0) < GetEMAValue(60, 1) && GetEMAValue(60, 1) < GetEMAValue(60, 2)) {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <returns></returns>
    float GetValue(int dir = 1)
    {
        #region 点 计算
        List<float> pList_1 = new List<float>();
        List<float> pList_2 = new List<float>();
        List<float> pList_3 = new List<float>();

        List<float> pList60 = new List<float>();

        for (int i = 0; i < V_Length; i++)
        {
            float p1 = GetEMAValue(V_CycleList[0], i);
            float p2 = GetEMAValue(V_CycleList[1], i);
            float p3 = GetEMAValue(V_CycleList[2], i);

            float p60 = GetEMAValue(60, i);

            pList_1.Add(p1);
            pList_2.Add(p2);
            pList_3.Add(p3);

            pList60.Add(p60);
        }
        #endregion

        #region 1. 计算 EMA 排列

        for (int i = 0; i < V_Length; i++)
        {
            if (dir > 0)
            {
                if (pList_1[i] >= pList_2[i] && pList_2[i] >= pList_3[i])
                {
                    //符合多头排列
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                if (pList_1[i] <= pList_2[i] && pList_2[i] <= pList_3[i])
                {
                    //符合空头排列
                }
                else
                {
                    return 0;
                }
            }

        }
        #endregion
        return 1;
    }

    float GetKValue(List<float> kList)
    {

        //| k |< 0.008,加权值 p = k * 1
        //| k |> 0.008, 加权值 p = ((| k | -0.08) * 2 +| k | *1) * (k > 0 ? 1 : -1)

        float temp = 0;
        for (int i = 0; i < kList.Count; i++)
        {
            float value = kList[i];
            float abs = MathF.Abs(value);
            if (abs > 0.08f)
            {
                temp += ((abs - 0.08f) * 2 + abs) * (value > 0 ? 1 : -1);
            }
            else
            {
                temp += value;
            }
        }

        float result = temp / kList.Count * 100;
        return result * ((V_Leverage * 0.02f) + 1);
    }

    float GetMA60Value(List<float> pList60, int dir)
    {

        float temp = 0;

        for (int i = 0; i < pList60.Count; i++)
        {

            float m = pList60[i];
            float y = V_Cache.V_KLineData[i].V_ClosePrice;

            if (dir > 0)
            {
                temp += (y - m) / y;
            }
            else
            {
                temp += (m - y) / y;
            }
        }
        return temp / pList60.Count * 100;
    }

    public void SetCycle(string setting)
    {
        string[] cycles = setting.Split('_');
        if (cycles.Length >= 3)
        {
            V_CycleList[0] = int.Parse(cycles[0]);
            V_CycleList[1] = int.Parse(cycles[1]);
            V_CycleList[2] = int.Parse(cycles[2]);
        }
    }
    #endregion
}
