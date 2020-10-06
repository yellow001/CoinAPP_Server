using System;
using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class SimpleMAHelper : BaseTaticsHelper
{
    int MaLength = 5;
    int EmaLength = 10;

    int LongMaLength = 20;
    #region 重载

    /// <summary>
    /// 初始化设置  合约;时长;MA参考线;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 6)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            MaLength = int.Parse(strs[2]);
            EmaLength = int.Parse(strs[3]);
            LongMaLength = int.Parse(strs[4]);
            V_Leverage = float.Parse(strs[5]);
        }
        //Console.WriteLine(V_Instrument_id + ":合约 " + V_Instrument_id);
        base.Init(setting);
    }

    /// <summary>
    /// 分析历史数据
    /// </summary>
    /// <returns></returns>
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
    /// 是否需要平仓
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="percent"></param>
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

            //if (percent <= lossPercent * 0.5f && result > 0)
            //{
            //    long time = DateTime.UtcNow.Ticks - V_LastOpTime.Ticks;
            //    if (isTest)
            //    {
            //        time = V_Cache.V_KLineData[0].V_Timestamp.Ticks - V_LastOpTime.Ticks;
            //    }

            //    bool shouldReset = time - V_Min * Util.Minute_Ticks <= 0;

            //    return shouldReset;
            //}


            if (percent >= winPercent)
            {
                maxAlready = true;
                return result > 0;
            }

            //if (maxAlready && result > 1)
            //{
            //    return true;
            //}

            DateTime t = DateTime.UtcNow;

            if (isTest)
            {
                t = V_Cache.V_KLineData[0].V_Timestamp;
            }

            ////5个K线内，指标反向，溜
            //if ((t - V_LastOpTime).TotalMinutes < V_Min * 5 && result > 0)
            //{
            //    return true;
            //}

            //if (F_IsWeekend(t))
            //{
            //    //周末当他是震荡行情
            //    return result > 0;
            //}


            if (percent < 0 && (t - V_LastOpTime).TotalMinutes > AppSetting.Ins.GetInt("ForceOrderTime") * V_Min)
            {
                //持仓时间有点久了，看机会溜吧
                return result > 0;
            }

        }
        return false;
    }

    float F_GetMA(int length)
    {
        return MA.GetMA(length, V_Cache.V_KLineData);
    }

    float F_GetEMA(int length)
    {
        return EMA.GetEMA(length, V_Cache.V_KLineData);
    }

    #endregion


    #region 策略方法


    int GetValue(bool isOrder, int orderDir, bool isTest = false)
    {
        if (!isTest)
        {
            DateTime t = DateTime.UtcNow;

            int hourValue = (int)Math.Ceiling((t.Hour + (t.Minute / 60f)) * 100f);

            int v = (int)((V_Min / 60f) * 100f);

            if ((v - hourValue % v) > 4 || (V_LastOpTime.Day == t.Day && V_LastOpTime.Hour == t.Hour))
            {
                return 0;
            }
        }

        float MaValue = F_GetMA(MaLength);
        float EmaValue = F_GetEMA(EmaLength);
        float LongMaValue = F_GetMA(LongMaLength);

        float MaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[MaLength - 1].V_ClosePrice;
        float EmaKValue = EmaValue - EMA.GetEMA(EmaLength, V_Cache.V_KLineData.GetRange(1, EmaLength));
        float LongMaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[LongMaLength - 1].V_ClosePrice;

        bool isLongKline = V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice;

        //float longValue = isTest ? V_Cache.V_KLineData[0].V_HightPrice : V_Cache.V_KLineData[0].V_ClosePrice;
        //float shortValue = isTest? V_Cache.V_KLineData[0].V_LowPrice : V_Cache.V_KLineData[0].V_ClosePrice;

        float curValue = V_Cache.V_KLineData[0].V_ClosePrice;
        float highValue = V_Cache.V_KLineData[0].V_HightPrice;
        float lowValue = V_Cache.V_KLineData[0].V_LowPrice;

        if (isOrder)
        {
            //if (curValue >= MaValue && V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice && MaKValue >= 0)
            //{
            //    return 1;
            //}
            //else if (curValue <= MaValue && V_Cache.V_KLineData[0].V_ClosePrice < V_Cache.V_KLineData[0].V_OpenPrice && MaKValue <= 0)
            //{
            //    return -1;
            //}
            if ((MaKValue > 0 && EmaKValue > 0) && (curValue > LongMaValue || LongMaKValue > 0))
            {
                return 1;
            }
            else if ((MaKValue < 0 && EmaKValue < 0) && (curValue < LongMaValue || LongMaKValue < 0))
            {
                return -1;
            }
        }
        else
        {
            //if (orderDir > 0)
            //{
            //    if (curValue <= MaValue)
            //    {
            //        if (MaKValue < 0 && V_Cache.V_KLineData[0].V_ClosePrice < V_Cache.V_KLineData[0].V_OpenPrice)
            //        {
            //            return 2;
            //        }
            //        return 1;
            //    }
            //}
            //else if (orderDir < 0)
            //{
            //    if (curValue >= MaValue)
            //    {
            //        if (MaKValue > 0 && V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice)
            //        {
            //            return 2;
            //        }
            //        return 1;
            //    }
            //}

            if (orderDir > 0)
            {
                //1.0
                //if (curValue < MaValue && curValue < EmaValue)
                //{
                //    if (MaKValue < 0 && EmaKValue < 0)
                //    {
                //        return 2;
                //    }
                //    return 1;
                //}

                //2.0
                //if ((MaKValue < 0 && EmaKValue < 0) && (!isLongKline))
                //{
                //    return 1;
                //}

                //3.0
                if ((curValue < MaValue || curValue < EmaValue))
                {
                    return 1;
                }
            }
            else if (orderDir < 0)
            {
                //1.0
                //if (curValue > MaValue && curValue > EmaValue)
                //{
                //    if (MaKValue > 0 && EmaKValue > 0)
                //    {
                //        return 2;
                //    }
                //    return 1;
                //}

                //2.0
                //if ((MaKValue > 0 && EmaKValue > 0) && (isLongKline))
                //{
                //    return 1;
                //}

                //3.0
                if ((curValue > MaValue || curValue > EmaValue))
                {
                    return 1;
                }
            }
        }

        return 0;
    }

    #endregion
}