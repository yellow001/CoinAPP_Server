using CoinAPP_Server.App;
using NetFrame.Tool;
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

    MatchItemHandler matchItemHandler = MatchItemHandler.Ins;


    Dictionary<int, KLineCache> kLineDataDic = new Dictionary<int, KLineCache>();

    public List<int> MinList = new List<int>() {30,60,240,360};

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

        Console.WriteLine(V_Instrument_id + ":分析");
        Debugger.Warn(V_Instrument_id + ":分析");

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
        if (isTest)
        {
            if (kLineDataDic.Count <= 0)
            {
                kLineDataDic.Add(60, V_Cache);

                //KLineCache kLineCache60 = new KLineCache();
                //kLineCache60.RefreshData(V_Cache.GetMergeKLine(1));
                //kLineDataDic.Add(60, kLineCache60);

                KLineCache kLineCache240 = new KLineCache();
                kLineCache240.RefreshData(V_Cache.GetMergeKLine(4));
                kLineDataDic.Add(240, kLineCache240);

                KLineCache kLineCache360 = new KLineCache();
                kLineCache360.RefreshData(V_Cache.GetMergeKLine(6));
                kLineDataDic.Add(360, kLineCache360);
            }
            else {
                kLineDataDic[60] = V_Cache;
                //kLineDataDic[60].RefreshData(V_Cache.GetMergeKLine(1));
                kLineDataDic[240].RefreshData(V_Cache.GetMergeKLine(4));
                kLineDataDic[360].RefreshData(V_Cache.GetMergeKLine(6));
            }
        }

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
            //int order = GetValue(true, 0, isTest);

            int result = GetValue(false, dir, isTest);

            //return result > 0;

            maxPercent = maxPercent < percent ? percent : maxPercent;

            if (percent >= winPercent)
            {
                return result > 0;
            }


            if (V_MaxAlready)
            {
                return result > 0;
            }


            if (percent > winPercent)
            {
                V_MaxAlready = true;
            }


            if (percent < lossPercent * V_Length)
            {
                return result > 0;
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

        float midValue = (highValue + lowValue) * 0.5f;

        KLine LastKLine = V_Cache.V_KLineData[1];


        List<int> tempList = new List<int>();

        float doLongValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoLong, kLineDataDic, V_LongShortRatio, ref tempList);

        float doShortValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoShort, kLineDataDic, V_LongShortRatio, ref tempList);

        float closeLongValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CloseLong, kLineDataDic, V_LongShortRatio, ref tempList);

        float closeShortValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CLoseShort, kLineDataDic, V_LongShortRatio, ref tempList);


        #region 4.0


        if (isOrder)
        {

            if (doShortValue>doLongValue*2)
            {
                return -1;
            }

            if (doLongValue>doShortValue*2)
            {
                return 1;
            }
        }
        else
        {
            if (orderDir > 0)
            {
                if (closeLongValue > doLongValue)
                {
                    return 1;
                }
            }

            if (orderDir < 0)
            {

                if (closeShortValue > doShortValue)
                {
                    return 1;
                }
            }
        }
        #endregion

        return 0;
    }

    public override async Task F_HandleOrder(AccountInfo info)
    {
        for (int i = 0; i < MinList.Count; i++)
        {
            int value = MinList[i];
            JContainer con = await CommonData.Ins.V_SwapApi.getCandlesDataAsync(V_Instrument_id, DateTime.Now.AddMinutes(-value * 200), DateTime.Now, 60 * value);

            if (kLineDataDic.ContainsKey(value))
            {
                kLineDataDic[value].RefreshData(con);
            }
            else {

                KLineCache kLineCache = new KLineCache();
                kLineCache.RefreshData(con);

                kLineDataDic.Add(value,kLineCache);
            }
        }
        await base.F_HandleOrder(info);
    }


    public void SetCycle(string setting)
    {
        MinList.Clear();
        string[] cycles = setting.Split('_');

        for (int i = 0; i < cycles.Length; i++)
        {
            MinList.Add(int.Parse(cycles[i]));
        }
    }

    #endregion
}
