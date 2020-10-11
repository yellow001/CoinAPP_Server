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

            if (percent >= winPercent)
            {
                return true;
            }

            if (percent >= winPercent * V_Length)
            {
                return result > 0;
            }
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


        //#region 点 计算

        //float p1 = F_GetEMA(V_CycleList[0], 0);
        //float p2 = F_GetEMA(V_CycleList[1], 0);
        //float p3 = F_GetEMA(V_CycleList[2], 0);

        //float k1 = p1 - F_GetEMA(V_CycleList[0], 1);
        //float k2 = p2 - F_GetEMA(V_CycleList[1], 1);
        //float k3 = p3 - F_GetEMA(V_CycleList[2], 1);


        //#endregion

        //float closeValue = V_Cache.V_KLineData[0].V_ClosePrice;
        //float openValue = V_Cache.V_KLineData[0].V_OpenPrice;
        //float highValue = V_Cache.V_KLineData[0].V_HightPrice;
        //float lowValue = V_Cache.V_KLineData[0].V_LowPrice;

        //bool isGreenKline = closeValue > openValue;

        //bool isPLong = false;
        //bool isPShort = false;

        //bool isKLong = false;
        //bool isKShort = false;

        //#region 1. 计算 EMA 排列

        //if (p1 >= p2 && p2 >= p3)
        //{
        //    //符合多头排列
        //    isPLong = true;
        //}
        //else
        //{
        //    isPLong = false;
        //}

        //if (p1 <= p2 && p2 <= p3)
        //{
        //    //符合空头排列
        //    isPShort = true;
        //}
        //else
        //{
        //    isPShort = false;
        //}


        //for (int i = 0; i < V_Length; i++)
        //{
        //    if (k1 >= 0 && k2 >= 0 && k3 >= 0)
        //    {
        //        //符合多头排列
        //        isKLong = true;
        //    }
        //    else
        //    {
        //        isKLong = false;
        //    }

        //    if (k1 <= 0 && k2 <= 0 && k3 <= 0)
        //    {
        //        //符合空头排列
        //        isKShort = true;
        //    }
        //    else
        //    {
        //        isKShort = false;
        //    }
        //}

        //int longValue = 0;
        //int shortValue = 0;

        //if (isPLong) {
        //    longValue++;
        //}
        //if (isKLong) {
        //    longValue++;
        //}

        //if (isPShort)
        //{
        //    shortValue++;
        //}
        //if (isKShort)
        //{
        //    shortValue++;
        //}

        //#endregion

        //if (isOrder)
        //{
        //    if (k1 > 0 && k2 > 0)
        //    {
        //        return 1;
        //    }
        //    else if (k1 < 0 && k2 < 0)
        //    {
        //        return -1;
        //    }
        //}
        //else
        //{
        //    //返回>0就是要平仓
        //    if (orderDir < 0)
        //    {
        //        if (k1 > 0 || k2 > 0) {
        //            return 1;
        //        }
        //    }
        //    if (orderDir > 0)
        //    {
        //        if (k1 < 0 || k2 < 0)
        //        {
        //            return 1;
        //        }
        //    }
        //    //return 1;
        //}

        //return 0;


        float MaValue = F_GetEMA(V_CycleList[0], 0);

        float boll_MidValue, boll_UpValue, boll_LowValue;

        boll_MidValue = Boll.GetBoll(V_CycleList[1], V_Cache.V_KLineData, out boll_UpValue, out boll_LowValue);

        //float MaValue2 = F_GetMA(MaLength2);
        //float LongMaValue = F_GetMA(LongMaLength);

        float MaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[0]].V_ClosePrice;
        float MaKValue2 = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[1]].V_ClosePrice;
        //float LongMaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[LongMaLength].V_ClosePrice;

        bool isGreenKLine = V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice;

        //float longValue = isTest ? V_Cache.V_KLineData[0].V_HightPrice : V_Cache.V_KLineData[0].V_ClosePrice;
        //float shortValue = isTest? V_Cache.V_KLineData[0].V_LowPrice : V_Cache.V_KLineData[0].V_ClosePrice;

        float closeValue = V_Cache.V_KLineData[0].V_ClosePrice;
        float openValue = V_Cache.V_KLineData[0].V_OpenPrice;
        float highValue = V_Cache.V_KLineData[0].V_HightPrice;
        float lowValue = V_Cache.V_KLineData[0].V_LowPrice;

        KLine LastKLine = V_Cache.V_KLineData[1];

        bool isLong = false;
        bool isShort = false;

        //V_CycleList[2]敏感度

        if (V_CycleList[2] > 1)
        {
            if (MaKValue > 0 && isGreenKLine)
            {
                isLong = true;
            }

            if (MaKValue < 0 && !isGreenKLine)
            {
                isShort = true;
            }
        }
        else if (V_CycleList[2] > 0)
        {
            if (MaKValue > 0 && highValue > boll_UpValue && isGreenKLine)
            {
                isLong = true;
            }

            if (MaKValue < 0 && lowValue < boll_LowValue && !isGreenKLine)
            {
                isShort = true;
            }
        }
        else
        {
            if (MaKValue > 0 && closeValue > MaValue && highValue > boll_UpValue && isGreenKLine)
            {
                isLong = true;
            }

            if (MaKValue < 0 && closeValue < MaValue && lowValue < boll_LowValue && !isGreenKLine)
            {
                isShort = true;
            }
        }

        //if (MaKValue > 0 && highValue > boll_UpValue && isGreenKLine)
        //{
        //    isLong = true;
        //}

        //if (MaKValue < 0 && lowValue < boll_LowValue && !isGreenKLine)
        //{
        //    isShort = true;
        //}

        if (isOrder)
        {
            if (isShort)
            {
                return -1;
            }
            if (isLong)
            {
                return 1;
            }
        }
        else
        {
            if (orderDir > 0)
            {
                //V_CycleList[2]敏感度，越高越容易返回1

                if (V_CycleList[2] > 1)
                {
                    if (MaKValue < 0 || closeValue <= MaValue || MaKValue2 < 0 || lowValue < boll_MidValue)
                    {
                        return 1;
                    }
                }
                else if (V_CycleList[2] > 0)
                {
                    if ((MaKValue < 0 && closeValue <= MaValue) || MaKValue2 < 0 || lowValue < boll_MidValue)
                    {
                        return 1;
                    }
                }
                else {
                    if (MaKValue < 0 && closeValue <= MaValue && MaKValue2 < 0)
                    {
                        return 1;
                    }
                }
            }

            if (orderDir < 0)
            {
                if ((MaKValue > 0 && closeValue >= MaValue) || highValue > boll_MidValue)
                {
                    return 1;
                }

                if (V_CycleList[2] > 1)
                {
                    if (MaKValue > 0 || closeValue >= MaValue || MaKValue2 > 0 || highValue > boll_MidValue)
                    {
                        return 1;
                    }
                }
                else if (V_CycleList[2] > 0)
                {
                    if ((MaKValue > 0 && closeValue >= MaValue) || MaKValue2 > 0 || highValue < boll_MidValue)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (MaKValue > 0 && closeValue >= MaValue && MaKValue2 > 0)
                    {
                        return 1;
                    }
                }
            }
        }

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

    float F_GetMA(int length)
    {
        return MA.GetMA(length, V_Cache.V_KLineData);
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
