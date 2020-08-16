using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class EMAHelper3 : BaseTaticsHelper, ICycleTatics
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
    public override int MakeOrder()
    {
        return GetValue(true, 0);
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

            int sign = GetValue(false, dir);

            if (percent >= winPercent)
            {
                maxAlready = true;
                return sign > 0;
            }

            if (percent < lossPercent * 0.5f && sign > 0)
            {
                //指标反向+亏损，溜吧
                return true;
            }

            if (maxAlready && sign > 0)
            {
                //如果曾经到达过最高而指标反向，止盈一下吧
                return true;
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

    #endregion

    #region 策略方法

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

    /// <summary>
    /// 获取倍数值的 EMA 值
    /// </summary>
    /// <param name="length"></param>
    /// <param name="per">倍数</param>
    /// <param name="index"></param>
    /// <returns></returns>
    float GetEMAValue(int length,int per, int index = 0)
    {
        if (V_Cache == null)
        {
            return 0;
        }

        if (V_Cache.V_KLineData.Count < (length  + index)* per)
        {
            return 0;
        }

        return EMA.GetEMA(length * per, V_Cache.V_KLineData.GetRange(index * per, length * per));
    }

    float GetEMA_KValue(int value)
    {
        if (GetEMAValue(value, 0) > GetEMAValue(value, 1) && GetEMAValue(value, 1) > GetEMAValue(value, 2))
        {
            return 1;
        }
        else if (GetEMAValue(value, 0) < GetEMAValue(value, 1) && GetEMAValue(value, 1) < GetEMAValue(value, 2))
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <returns></returns>
    int GetValue(bool isOrder, int orderDir)
    {
        #region 点 计算

        Dictionary<int, float> pDic1 = new Dictionary<int, float>();
        Dictionary<int, float> pDic2 = new Dictionary<int,float>();
        Dictionary<int, float> pDic3 = new Dictionary<int, float>();

        for (int i = 0; i < V_CycleList.Count; i++)
        {
            float p1 = GetEMAValue(V_CycleList[i], i);
            float p2 = GetEMAValue(V_CycleList[i],(i+1)*2, i);
            float p3 = GetEMAValue(V_CycleList[i], (i + 2) * 4, i);

            pDic1[V_CycleList[i]] = p1;
            pDic2[V_CycleList[i]] = p2;
            pDic3[V_CycleList[i]] = p3;
        }
        #endregion

        if (isOrder)
        {
            //多头排列三连或空头排列三连才开单
            if (pDic1[V_CycleList[0]] >= pDic1[V_CycleList[1]] && pDic1[V_CycleList[1]] >= pDic1[V_CycleList[2]]
                && pDic2[V_CycleList[0]] >= pDic2[V_CycleList[1]] && pDic2[V_CycleList[1]] >= pDic2[V_CycleList[2]]
                && pDic3[V_CycleList[0]] >= pDic3[V_CycleList[1]] && pDic3[V_CycleList[1]] >= pDic3[V_CycleList[2]]) {
                return 1;
            }

            if (pDic1[V_CycleList[0]] <= pDic1[V_CycleList[1]] && pDic1[V_CycleList[1]] <= pDic1[V_CycleList[2]]
                && pDic2[V_CycleList[0]] <= pDic2[V_CycleList[1]] && pDic2[V_CycleList[1]] <= pDic2[V_CycleList[2]]
                && pDic3[V_CycleList[0]] <= pDic3[V_CycleList[1]] && pDic3[V_CycleList[1]] <= pDic3[V_CycleList[2]])
            {
                return -1;
            }
        }
        else
        {
            //返回>0就是要平仓
            if (orderDir < 0)
            {
                if (pDic1[V_CycleList[0]] >= pDic1[V_CycleList[1]] && pDic1[V_CycleList[1]] >= pDic1[V_CycleList[2]]){
                    return 1;
                }
            }
            if (orderDir > 0)
            {
                if (pDic1[V_CycleList[0]] <= pDic1[V_CycleList[1]] && pDic1[V_CycleList[1]] <= pDic1[V_CycleList[2]]){
                    return 1;
                }
            }
        }


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
