using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FourPriceHelper : BaseTaticsHelper
{
    #region 重载

    /// <summary>
    /// 初始化设置  合约;K线时长;最高价采样点_最低价采样点;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 3)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            V_Leverage = float.Parse(strs[2]);
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
    public override int MakeOrder(bool isTest=false)
    {
        return GetValue(true, 0,isTest);
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
            //int result = GetMAPriceResult(dir);

            //if (percent <= lossPercent * 0.1f && result > 0)
            //{
            //    long time = DateTime.UtcNow.Ticks - V_LastOpTime.Ticks;
            //    if (isTest)
            //    {
            //        time = V_Cache.V_KLineData[0].V_Timestamp.Ticks - V_LastOpTime.Ticks;
            //    }

            //    bool shouldReset = time - V_Min * Util.Minute_Ticks <= 0;

            //    return shouldReset;
            //}


            int sign = GetValue(false, dir, isTest);

            if (percent >= winPercent)
            {
                maxAlready = true;
                return sign > 0;
            }

            DateTime t = DateTime.UtcNow;

            if (isTest)
            {
                t = V_Cache.V_KLineData[0].V_Timestamp;
            }
            if (percent < 0 && (t - V_LastOpTime).TotalMinutes > AppSetting.Ins.GetInt("ForceOrderTime") * V_Min)
            {
                //持仓时间有点久了，看机会溜吧
                return sign > 0;
            }

        }
        return false;
    }

    float F_GetMA(int length)
    {
        return MA.GetMA(length, V_Cache.V_KLineData);
    }

    #endregion


    #region 策略方法


    int GetValue(bool isOrder, int orderDir,bool isTest=false)
    {
        if (!isTest)
        {
            DateTime t = DateTime.UtcNow;
            if ((V_Min - t.Minute % V_Min) != 1)
            {
                return 0;
            }
        }

        if (isOrder)
        {
            if (V_Cache.V_KLineData[0].V_HightPrice >= V_Cache.V_KLineData[1].V_HightPrice)
            {
                //if (V_Cache.V_KLineData[0].V_HightPrice >= MA60)
                //{
                return 1;
                //}
            }
            else if (V_Cache.V_KLineData[0].V_LowPrice <= V_Cache.V_KLineData[1].V_LowPrice)
            {
                //if (V_Cache.V_KLineData[0].V_LowPrice <= MA60)
                //{
                return -1;
                //}
            }
        }
        else {
            if (orderDir > 0)
            {
                //跌破开盘 止损
                if (V_Cache.V_KLineData[0].V_ClosePrice <= V_Cache.V_KLineData[0].V_OpenPrice)
                {
                    return 1;
                }
            }
            else if (orderDir < 0)
            {
                //涨破开盘 止损
                if (V_Cache.V_KLineData[0].V_ClosePrice >= V_Cache.V_KLineData[0].V_OpenPrice)
                {
                    return 1;
                }
            }
        }

        return 0;
    }

    public int GetMAPriceResult(int orderDir, bool isTest = false) {
        float MAPrice = F_GetMA(20);

        if (orderDir > 0)
        {
            //跌破均线 考虑走人
            if (V_Cache.V_KLineData[0].V_ClosePrice <= MAPrice)
            {
                return 1;
            }
        }
        else if (orderDir < 0)
        {
            //涨破均线 考虑走人
            if (V_Cache.V_KLineData[0].V_ClosePrice >= MAPrice)
            {
                return 1;
            }
        }

        return 0;
    }

    #endregion
}
