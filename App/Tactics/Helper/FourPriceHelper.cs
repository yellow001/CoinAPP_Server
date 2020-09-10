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
    KLine m_LastKLine;
    KLine m_CurKLine;

    int MergeHour = 4;

    #region 重载

    /// <summary>
    /// 初始化设置  合约;K线时长;最高价采样点_最低价采样点;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 4)
        {
            V_Instrument_id = strs[0];
            MergeHour = int.Parse(strs[1]);
            V_Min = int.Parse(strs[2]);
            V_Leverage = float.Parse(strs[3]);
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

    public override void ClearRunData()
    {
        m_CurKLine = null;
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder()
    {
        //Task.Run(async () => { await F_GetDayKLine(); }).Wait();
        GetMergeKLine(V_Cache.V_KLineData[0].V_Timestamp, ref m_CurKLine, ref m_LastKLine);

        if (m_CurKLine == null || m_LastKLine == null)
        {
            return 0;
        }

        return GetValue(true, 0);
    }

    /// <summary>
    /// 是否需要平仓
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    protected override bool OnShouldCloseOrder(int dir, float percent, bool isTest = false)
    {
        //Task.Run(async () => { await F_GetDayKLine(); }).Wait();

        GetMergeKLine(V_Cache.V_KLineData[0].V_Timestamp, ref m_CurKLine, ref m_LastKLine);


        if (m_CurKLine == null || m_LastKLine == null) {
            return false;
        }

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {

            int sign = GetValue(false, dir);

            if (percent >= winPercent)
            {
                maxAlready = true;
                return sign > 0;
            }

            //if (percent < lossPercent * 0.5f && sign > 0)
            //{
            //    //指标反向+亏损，溜吧
            //    return true;
            //}

            //if (maxAlready && sign > 0)
            //{
            //    //如果曾经到达过最高而指标反向，止盈一下吧
            //    return true;
            //}

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

    void GetMergeKLine(DateTime curDataTime,ref KLine curLine,ref KLine lastLine) {

        bool refresh = false;

        DateTime time = new DateTime(curDataTime.Year, curDataTime.Month, curDataTime.Day, curDataTime.Hour, 0, 0);

        if (m_LastKLine == null || m_CurKLine == null)
        {
            refresh = true;
        }
        else
        {
            if (m_CurKLine.V_Timestamp.Ticks < time.Ticks)
            {
                refresh = true;
            }
        }

        if (refresh) {
            curLine = new KLine();
            lastLine = new KLine();

            DateTime lastDataTime = time.AddHours(-MergeHour);
            DateTime nextDataTime = time.AddHours(MergeHour);

            curLine.V_Timestamp = time;
            lastLine.V_Timestamp = lastDataTime;

            List<KLine> curList = new List<KLine>();
            List<KLine> lastList = new List<KLine>();
            for (int i = 0; i < V_Cache.V_KLineData.Count; i++)
            {
                KLine data = V_Cache.V_KLineData[i];
                if (data.V_Timestamp <= nextDataTime && data.V_Timestamp >= time)
                {
                    curList.Add(data);
                }
                else if (data.V_Timestamp <= time && data.V_Timestamp >= lastDataTime)
                {
                    lastList.Add(data);
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < curList.Count; i++)
            {
                if (curList[i].V_Timestamp == curDataTime)
                {
                    curLine.V_OpenPrice = curList[i].V_OpenPrice;
                }
            }

            for (int i = 0; i < lastList.Count; i++)
            {
                if (lastLine.V_HightPrice == 0)
                {
                    lastLine.V_HightPrice = lastList[i].V_HightPrice;
                    lastLine.V_LowPrice = lastList[i].V_LowPrice;
                }

                if (lastList[i].V_LowPrice < lastLine.V_LowPrice)
                {
                    lastLine.V_LowPrice = lastList[i].V_LowPrice;
                }

                if (lastList[i].V_HightPrice > lastLine.V_HightPrice)
                {
                    lastLine.V_HightPrice = lastList[i].V_HightPrice;
                }
            }
        }
    }

    #endregion


    #region 策略方法


    int GetValue(bool isOrder, int orderDir)
    {
        if (isOrder)
        {
            float MA60 = F_GetMA(60);
            if (V_Cache.V_KLineData[0].V_ClosePrice >= m_LastKLine.V_HightPrice)
            {
                if (V_Cache.V_KLineData[0].V_ClosePrice >= MA60) {
                    return 1;
                }
            }
            else if (V_Cache.V_KLineData[0].V_ClosePrice <= m_LastKLine.V_LowPrice)
            {
                if (V_Cache.V_KLineData[0].V_ClosePrice <= MA60)
                {
                    return -1;
                }
            }
        }
        else {
            if (orderDir > 0) {
                //跌破开盘 止损
                if (V_Cache.V_KLineData[0].V_ClosePrice <= m_CurKLine.V_OpenPrice) {
                    return 1;
                }
            }
            else if (orderDir < 0) {
                //涨破开盘 止损
                if (V_Cache.V_KLineData[0].V_ClosePrice >= m_CurKLine.V_OpenPrice)
                {
                    return 1;
                }
            }
        }

        return 0;
    }

    #endregion
}
