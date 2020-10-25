/**
 * EMA 多空头排列
 * V1.0 2019-11-27
 * 
 * 
 * **/

using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// EMA 多空头排列
/// </summary>
public class EMATaticsHelper : BaseTaticsHelper, ICycleTatics
{
    /// <summary>
    /// 采样点
    /// </summary>
    public float V_Length = 0.2f;

    /// <summary>
    /// 周期
    /// </summary>
    public List<int> V_CycleList = new List<int>() { 5, 10, 20 };

    #region 重载
    /// <summary>
    /// 初始化设置 合约;K线时长;采样点;周期(小_中_大);倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 4)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            V_Length = float.Parse(strs[2]);
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

        Console.WriteLine(V_Instrument_id + "  分析结果");
        Debugger.Warn(V_Instrument_id + "  分析结果");

        TaticsTestRunner.TestRun(this);

        Console.WriteLine(V_Instrument_id + ":分析历史数据完毕");

        Debugger.Warn(V_Instrument_id + ":分析历史数据完毕");
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder(bool isTest = false)
    {
        return GetValue(true, 0, isTest);
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    protected override bool OnShouldCloseOrder(int dir, float percent, bool isTest = false)
    {
        //if (percent <= lossPercent)
        //{
        //    //无条件止损
        //    return true;
        //}
        //else
        //{
        //    int result = GetValue(false, dir, isTest);

        //    if (percent >= winPercent)
        //    {
        //        return result > 0;
        //    }

        //    DateTime t = DateTime.UtcNow;
        //    if (isTest)
        //    {
        //        t = V_Cache.V_KLineData[0].V_Timestamp;
        //    }

        //    if (percent >= winPercent)
        //    {
        //        return true;
        //    }

        //    //指标反向，溜
        //    if (result > 0 && percent >= winPercent * 0.25f)
        //    {
        //        return true;
        //    }

        //    //if (result > 1 && percent < 0)
        //    //{
        //    //    return true;
        //    //}

        //    //if (F_IsWeekend(t) && result > 0 && percent >= winPercent * 0.2f)
        //    //{
        //    //    //周末当他是震荡行情
        //    //    return true;
        //    //}

        //    //if (percent < 0 && result > 0) {
        //    //    return true;
        //    //}


        //    if (percent < 0 && (t - V_LastOpTime).TotalMinutes > AppSetting.Ins.GetInt("ForceOrderTime") * V_Min)
        //    {
        //        //持仓时间有点久了，看机会溜吧
        //        return result > 0;
        //    }

        //}
        //return false;

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            int result = GetValue(false, dir, isTest);

            int orderResult = GetValue(true, dir, isTest);

            if (percent >= winPercent * V_Length)
            {
#if !DEBUG
                Debugger.Log(percent + "  " + winPercent + "  " + V_Length);
#endif
                //return result > 0;
                return result > 0|| orderResult == -dir;
            }

            if (percent <= 0)
            {
#if !DEBUG
                Debugger.Log(percent + "  " + lossPercent + "  " + V_Length);
#endif
                //return orderResult == -dir;
                return orderResult == -dir|| result > 0;
            }

            //if (percent >= winPercent)
            //{
            //    V_MaxAlready = true;
            //    return true;
            //}

        }
        return false;
    }

    #endregion

    #region 策略方法
    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <returns></returns>
    int GetValue(bool isOrder, int orderDir, bool isTest = false)
    {
        if (!isTest)
        {
            if (!F_CanHanleOrder())
            {
                return 0;
            }
        }

        bool isGreenKLine = V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice;

        float closeValue = V_Cache.V_KLineData[0].V_ClosePrice;
        float openValue = V_Cache.V_KLineData[0].V_OpenPrice;
        float highValue = V_Cache.V_KLineData[0].V_HightPrice;
        float lowValue = V_Cache.V_KLineData[0].V_LowPrice;

        KLine LastKLine = V_Cache.V_KLineData[1];

        float minValue = V_Cache.V_KLineData[V_CycleList[0]].V_HightPrice;
        float maxValue = V_Cache.V_KLineData[V_CycleList[0]].V_LowPrice;

        for (int i = V_CycleList[0]; i < V_CycleList[2]; i++)
        {
            if (minValue > V_Cache.V_KLineData[i].V_HightPrice)
            {
                minValue = V_Cache.V_KLineData[i].V_HightPrice;
            }

            if (maxValue < V_Cache.V_KLineData[i].V_LowPrice)
            {
                maxValue = V_Cache.V_KLineData[i].V_LowPrice;
            }
        }

        List<float> volList = new List<float>();
        for (int i = 0; i < V_Cache.V_KLineData.Count; i++)
        {
            volList.Add(V_Cache.V_KLineData[i].V_Vol);
        }
        float vol_avg = volList.Average();


        float a = F_GetMA(V_CycleList[0]);
        float b = F_GetMA(V_CycleList[0],1);

        float k1 = (a - b) / b * 100;
        //Console.WriteLine("0 "+a+"  "+b+"  "+v);

        a = F_GetMA(V_CycleList[1]);
        b = F_GetMA(V_CycleList[1], 1);

        float k2 = (a - b) / b * 100;
        //Console.WriteLine("1 " + a + "  " + b + "  " + v);

        a = F_GetMA(V_CycleList[2]);
        b = F_GetMA(V_CycleList[2], 1);

        float k3 = (a - b) / b * 100;
        //Console.WriteLine("2 " + a + "  " + b + "  " + v);

        float MaValue = F_GetMA(V_CycleList[0]);
        float MaValue2 = F_GetMA(V_CycleList[1]);
        float LongMaValue = F_GetMA(V_CycleList[2]);

        float boll_MidValue, boll_UpValue, boll_LowValue;

        boll_MidValue = Boll.GetBoll(V_CycleList[1], V_Cache.V_KLineData, out boll_UpValue, out boll_LowValue);

        float MaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[0]].V_ClosePrice;
        float MaKValue2 = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[1]].V_ClosePrice;
        float LongMaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[2]].V_ClosePrice;

        float vol = V_Cache.V_KLineData[0].V_Vol;

        bool isLong = false;
        bool isShort = false;

        float per1 = (closeValue - MaValue) / MaValue * 100;
        float per2 = (closeValue - MaValue2) / MaValue2 * 100;
        float per3 = (closeValue - LongMaValue) / LongMaValue * 100;

        bool bigVol = false;
        for (int i = 0; i < 20; i++)
        {
            if (V_Cache.V_KLineData[i].V_Vol >= vol_avg * 5f)
            {
                bigVol = true;
                break;
            }
        }


        #region 3.0

        ////空头排列
        //if (MaValue < MaValue2 && MaValue2 < LongMaValue)
        //{
        //    //均线向下
        //    if (MaKValue < 0 && MaKValue2 < 0 && LongMaKValue < 0)
        //    {
        //        bool kValueEnough = MathF.Abs(per2) < 1 || (k1 > 0.2f && k3 > 0.02f);

        //        if (closeValue < MaValue && closeValue < LongMaValue && kValueEnough)
        //        {
        //            isShort = true;
        //        }
        //    }
        //}

        //if (MaValue > LongMaValue && k3 < -0.02f && k1 < -0.4f)
        //{
        //    isShort = true;
        //}

        //if (MaValue < 0 && LongMaKValue > 0 && MaValue2 > LongMaValue)
        //{
        //    if (per3 > 2 && k1 < -0.2f)
        //    {
        //        isShort = true;
        //    }
        //}


        ////多头排列
        //if (MaValue > MaValue2 && MaValue2 > LongMaValue)
        //{
        //    //均线向上
        //    if (MaKValue > 0 && MaKValue2 > 0 && LongMaKValue > 0)
        //    {
        //        bool kValueEnough = MathF.Abs(per2) < 1 || (k1 < -0.2f && k3 < -0.02f);
        //        if (closeValue > MaValue && closeValue > LongMaValue && kValueEnough)
        //        {
        //            isLong = true;
        //        }
        //    }
        //}

        //if (MaValue < LongMaValue && k3 > 0.02f && k1 > 0.4f)
        //{
        //    isLong = true;
        //}

        //if (MaValue > 0 && LongMaKValue < 0 && MaValue2 < LongMaValue)
        //{
        //    if (MaValue > 0 && per3 > 2 && k1 > 0.2f)
        //    {
        //        isLong = true;
        //    }
        //}

        //if (isOrder)
        //{
        //    if (!bigVol)
        //    {
        //        //量能低，不管
        //        return 0;
        //    }

        //    if (isLong && !isShort)
        //    {
        //        return 1;
        //    }

        //    if (isShort && !isLong)
        //    {
        //        return -1;
        //    }

        //}
        //else
        //{
        //    if (orderDir > 0)
        //    {
        //        //return isShort ? 1 : 0;
        //        return (MaKValue < 0 && k1 < -0.1f) ? 1 : 0;
        //    }

        //    if (orderDir < 0)
        //    {
        //        //return isLong ? 1 : 0;
        //        return (MaKValue > 0 && k1 > 0.1f) ? 1 : 0;
        //    }
        //}

        #endregion

        #region 4.0

        if (MaValue < MaValue2 && MaValue2 < LongMaValue)
        {
            if (MaValue < 0 && MaValue2 < 0 && LongMaValue < 0)
            {
                if (per2 > -2 && per3 > -4&&closeValue<minValue)
                {
                    isShort = true;
                }
            }
        }

        if (MathF.Abs(per3) < 1.6f && MathF.Abs(k3) < 0.04f)
        {
            if (openValue > boll_UpValue && closeValue < maxValue)
            {
                isShort = true;
            }

            if (openValue < boll_LowValue && closeValue > minValue)
            {
                isLong = true;
            }
        }


        if (MaValue > MaValue2 && MaValue2 > LongMaValue)
        {
            if (MaValue > 0 && MaValue2 > 0 && LongMaValue > 0)
            {
                if (per2 < 2 && per3 < 4&&closeValue>maxValue)
                {
                    isLong = true;
                }
            }
        }


        if (isOrder)
        {
            if (!bigVol)
            {
                //量能低，不管
                return 0;
            }

            if (isLong && !isShort)
            {
                return 1;
            }

            if (isShort && !isLong)
            {
                return -1;
            }

        }
        else
        {
            if (orderDir > 0)
            {
                return isShort ? 1 : 0;
                //return (MaKValue < 0 && k1 < -0.1f) ? 1 : 0;
            }

            if (orderDir < 0)
            {
                return isLong ? 1 : 0;
                //return (MaKValue > 0 && k1 > 0.1f) ? 1 : 0;
            }
        }

        #endregion



        return 0;
    }

    /// <summary>
    /// 获取 EMA 值
    /// </summary>
    /// <param name="index">下标</param>
    /// <returns></returns>
    float F_GetEMA(int length, int index = 0)
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

    float F_GetMA(int length, int index = 0)
    {
        return MA.GetMA(length, V_Cache.V_KLineData.GetRange(index, length));
    }

    int ShouldOrderByOldAvg() {

        KLine curLine = V_Cache.V_KLineData[0];

        if (curLine.V_ClosePrice < V_Cache.V_KLineData[V_CycleList[2]].V_ClosePrice)
        {
            if (V_Cache.V_KLineData[V_CycleList[2] - V_CycleList[1]].V_ClosePrice > V_Cache.V_KLineData[V_CycleList[2]].V_ClosePrice)
            {
                return -1;
            }
        }

        if (curLine.V_ClosePrice > V_Cache.V_KLineData[V_CycleList[0]].V_ClosePrice) {
            if (V_Cache.V_KLineData[V_CycleList[2] - V_CycleList[1]].V_ClosePrice < V_Cache.V_KLineData[V_CycleList[2]].V_ClosePrice)
            {
                return 1;
            }
        }


        //if (V_Cache.V_KLineData[V_CycleList[1] - V_CycleList[0]].V_ClosePrice < V_Cache.V_KLineData[V_CycleList[1]].V_ClosePrice)
        //{
        //    return -1;
        //}

        //if (V_Cache.V_KLineData[V_CycleList[1] - V_CycleList[0]].V_ClosePrice > V_Cache.V_KLineData[V_CycleList[1]].V_ClosePrice)
        //{
        //    return 1;
        //}


        return 0;
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
