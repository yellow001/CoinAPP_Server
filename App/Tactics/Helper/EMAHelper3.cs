﻿using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EMAHelper3 : BaseTaticsHelper, ICycleTatics
{
    /// <summary>
    /// 采样点
    /// </summary>
    public float V_Length = 0.5f;

    /// <summary>
    /// 周期
    /// </summary>
    public List<int> V_CycleList = new List<int>() { 5, 10, 20 };

    float maxPercent = 0;

    KLineCache hourCache = new KLineCache();

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

        Console.WriteLine(V_Instrument_id + ":分析结果");
        Debugger.Warn(V_Instrument_id + ":分析结果");

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

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            int result = GetValue(false, dir, isTest);

            int orderResult = GetValue(true, dir, isTest);

            maxPercent = maxPercent < percent ? percent : maxPercent;

            if (percent >= winPercent)
            {
                return result > 0 || orderResult == -dir;
            }


            if (V_MaxAlready)
            {
                return result > 0 || orderResult == -dir;
            }


            if (percent > winPercent)
            {
                V_MaxAlready = true;
            }


            if (percent < lossPercent * V_Length)
            {
                if ((dir > 0 && V_LongShortRatio < CommonData.Ins.ShortRatio) || (dir < 0 && V_LongShortRatio > CommonData.Ins.LongRatio))
                {
                    return false;
                }
                else
                {
                    return result > 0;
                }
            }
        }
        return false;
    }

    #endregion

    #region 策略方法

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

    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <returns></returns>
    int GetValue(bool isOrder, int orderDir, bool isTest = false)
    {
        if (!isTest&& (V_LongShortRatio > CommonData.Ins.ShortMaxRatio && V_LongShortRatio < CommonData.Ins.LongMaxRatio))
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

        float midValue = (highValue + lowValue) * 0.5f;

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
        List<float> perList = new List<float>();
        for (int i = 0; i < V_Cache.V_KLineData.Count; i++)
        {
            KLine line = V_Cache.V_KLineData[i];
            perList.Add((line.V_HightPrice - line.V_LowPrice) / line.V_HightPrice * 100);
            volList.Add(line.V_Vol);
        }
        float vol_avg = volList.Average();
        float per_avg = perList.Average();


        float a = F_GetMA(V_CycleList[0]);
        float b = F_GetMA(V_CycleList[0], 1);

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

        float EMaValue = F_GetEMA(V_CycleList[0]);
        float EMaValue2 = F_GetEMA(V_CycleList[1]);
        float LongEMaValue = F_GetEMA(V_CycleList[2]);

        float EMaKValue = EMaValue-F_GetEMA(V_CycleList[0],2);

        float boll_MidValue, boll_UpValue, boll_LowValue;

        boll_MidValue = Boll.GetBoll(V_CycleList[1], V_Cache.V_KLineData, out boll_UpValue, out boll_LowValue);

        float MaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[0]].V_ClosePrice;
        float MaKValue2 = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[1]].V_ClosePrice;
        float LongMaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[V_CycleList[2]].V_ClosePrice;

        float vol = V_Cache.V_KLineData[0].V_Vol;

        bool isLong = false;
        bool isShort = false;

        float per1 = (closeValue - MaValue) / closeValue * 100;
        float per2 = (closeValue - MaValue2) / closeValue * 100;
        float per3 = (closeValue - LongMaValue) / closeValue * 100;

        float allVol = 0;
        bool bigVol = false;
        bool bigBigVol = false;
        for (int i = 0; i < V_CycleList[0]; i++)
        {
            if (V_Cache.V_KLineData[i].V_Vol >= vol_avg * 2)
            {
                bigVol = true;
            }
            if (V_Cache.V_KLineData[i].V_Vol >= vol_avg * 6)
            {
                bigBigVol = true;
            }
        }

        for (int i = 0; i < V_CycleList[1]; i++)
        {
            allVol += V_Cache.V_KLineData[i].V_OpenPrice >= V_Cache.V_KLineData[i].V_ClosePrice ? -V_Cache.V_KLineData[i].V_Vol : V_Cache.V_KLineData[i].V_Vol;
        }

        float hourValue = 0;

        if (hourCache!=null && hourCache.V_KLineData!=null &&hourCache.V_KLineData.Count>0)
        {
            float hourMA1 = MA.GetMA(V_CycleList[0], hourCache.V_KLineData.GetRange(0, V_CycleList[0]));
            float hourMA2 = MA.GetMA(V_CycleList[0], hourCache.V_KLineData.GetRange(V_CycleList[0], V_CycleList[0]));

            hourValue = hourMA1 - hourMA2;
        }

        


        #region 4.0

        if (MaKValue>0 && (per2 < 5|| per2 < -10))
        {
            isLong = true;
        }

        if (MaKValue<0 && (per2 > -5 || per2 < 10))
        {
            isShort = true;
        }

        if (isOrder)
        {

            if (isShort && V_LongShortRatio > CommonData.Ins.ShortRatio)
            {
                return -1;
            }

            if (isLong && V_LongShortRatio < CommonData.Ins.LongRatio)
            {
                return 1;
            }
        }
        else
        {

            if (orderDir > 0)
            {
                return isLong&&hourValue>=0 ? 0 : 1;
            }

            if (orderDir < 0)
            {

                return isShort&&hourValue<=0 ? 0 : 1;
            }
        }

        #endregion



        return 0;
    }

    public override async Task F_HandleOrder(AccountInfo info)
    {
        //获取 1h 数据

        //获取近200条K线
        JContainer con = await CommonData.Ins.V_SwapApi.getCandlesDataAsync(V_Instrument_id, DateTime.Now.AddMinutes(-60 * 200), DateTime.Now, 60 * 60);

        hourCache.RefreshData(con);

        await base.F_HandleOrder(info);
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
