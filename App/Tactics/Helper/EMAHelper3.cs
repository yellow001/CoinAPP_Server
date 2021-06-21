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

    public List<int> V_MinList = new List<int>() {30,60,240,360};

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

        V_MinList = AppSetting.Ins.GetIntList("MinList");
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
            UpdateTestData();
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

        if (isTest)
        {
            UpdateTestData();
        }

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            int result = GetValue(false, dir, isTest);
            int order = GetValue(true, dir, isTest);

            //maxPercent = maxPercent < percent ? percent : maxPercent;

            //if (percent >= winPercent)
            //{
            //    return result > 0;
            //}


            //if (V_MaxAlready)
            //{
            //    return result > 0;
            //}


            //if (percent > winPercent)
            //{
            //    V_MaxAlready = true;
            //}


            //if (percent < lossPercent * V_Length)
            //{
            //    return result > 0;
            //}

            DateTime t = DateTime.UtcNow;

            if (isTest)
            {
                t = V_Cache.V_KLineData[0].V_Timestamp;
            }
            //if ((t - V_LastOpTime).TotalMinutes > AppSetting.Ins.GetInt("ForceOrderTime") * V_Min)
            //{
            //    //持仓时间有点久了，看机会溜吧
            //    return percent > 0 && (result > 0 || order != dir);
            //}
            if ((t - V_LastOpTime).TotalMinutes <= AppSetting.Ins.GetInt("ForceOrderTimeShort") * V_Min)
            {
                //开仓没多久就亏损，溜
                return percent < -5;
            }

            //return percent < -5 && result > 0;

            return result > 0;
        }
        return false;
    }

    void UpdateTestData()
    {
        if (kLineDataDic.Count <= 0)
        {
            for (int i = 0; i < V_MinList.Count; i++)
            {
                KLineCache cache = new KLineCache();
                cache.RefreshData(V_Cache.GetMergeKLine(V_MinList[i]/60));
                kLineDataDic.Add(V_MinList[i], cache);
            }
        }
        else
        {
            for (int i = 0; i < V_MinList.Count; i++)
            {
                kLineDataDic[V_MinList[i]].RefreshData(V_Cache.GetMergeKLine(V_MinList[i] / 60));
            }
        }
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

        List<int> tempList1 = new List<int>();
        List<int> tempList2 = new List<int>();

        float doLongValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoLong, kLineDataDic, V_LongShortRatio,V_CycleList, ref tempList1);

        float doShortValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoShort, kLineDataDic, V_LongShortRatio, V_CycleList, ref tempList2);

        float closeLongValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CloseLong, kLineDataDic, V_LongShortRatio, V_CycleList, ref tempList);

        float closeShortValue = matchItemHandler.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CLoseShort, kLineDataDic, V_LongShortRatio, V_CycleList, ref tempList);


        #region 4.0


        if (isOrder)
        {

            if ((doShortValue+closeLongValue)>(doLongValue+closeShortValue)*2)
            {
                if (!isTest)
                {
                    string str = "匹配id: ";
                    for (int i = 0; i < tempList2.Count; i++)
                    {
                        str += tempList2[i] + "  ";
                    }

                    Debugger.Log(str);
                }
                

                return -1;
            }

            if ((doLongValue+closeShortValue)>(doShortValue+closeLongValue)*2)
            {
                if (!isTest)
                {
                    string str = "匹配id: ";
                    for (int i = 0; i < tempList1.Count; i++)
                    {
                        str += tempList1[i] + "  ";
                    }

                    Debugger.Log(str);
                }

                return 1;
            }
        }
        else
        {
            if (orderDir > 0)
            {
                if ((doShortValue + closeLongValue) > (doLongValue + closeShortValue) * 2)
                {
                    return 1;
                }
            }

            if (orderDir < 0)
            {

                if ((doLongValue + closeShortValue) > (doShortValue + closeLongValue) * 2)
                {
                    return 1;
                }
            }
        }
        #endregion

        return 0;
    }

    public override async Task F_AfterHandleOrder(AccountInfo info)
    {
        for (int i = 0; i < V_MinList.Count; i++)
        {
            int value = V_MinList[i];
            JContainer con = await CommonData.Ins.V_SwapApi.getCandlesDataAsync(V_Instrument_id, DateTime.Now.AddMinutes(-value * 200), DateTime.Now, 60 * value);

            if (kLineDataDic.ContainsKey(value))
            {
                kLineDataDic[value].RefreshData(con);
            }
            else
            {

                KLineCache kLineCache = new KLineCache();
                kLineCache.RefreshData(con);

                kLineDataDic.Add(value, kLineCache);
            }
        }
        await base.F_AfterHandleOrder(info);
    }


    public void SetCycle(string setting)
    {
        V_CycleList.Clear();
        string[] cycles = setting.Split('_');

        for (int i = 0; i < cycles.Length; i++)
        {
            V_CycleList.Add(int.Parse(cycles[i]));
        }
    }

    #endregion
}
