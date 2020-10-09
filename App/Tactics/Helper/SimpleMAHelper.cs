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
    int MaLength2 = 10;

    int LongMaLength = 20;
    #region 重载

    /// <summary>
    /// 初始化设置  合约名;时长;MA参考线1;MA参考线2;长期MA参考线;止盈冷却;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 7)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            MaLength = int.Parse(strs[2]);
            MaLength2 = int.Parse(strs[3]);
            LongMaLength = int.Parse(strs[4]);
            cooldown = int.Parse(strs[5]);
            V_Leverage = float.Parse(strs[6]);
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

            if (percent >= winPercent)
            {
                return true;
            }

            if (percent >= winPercent*0.42f)
            {
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
            if (!F_CanHanleOrder()) {
                return 0;
            }
        }

        float MaValue = F_GetMA(MaLength);
        float MaValue2 = F_GetMA(MaLength2);
        float LongMaValue = F_GetMA(LongMaLength);

        float MaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[MaLength].V_ClosePrice;
        float MaKValue2 = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[MaLength2].V_ClosePrice;
        float LongMaKValue = V_Cache.V_KLineData[0].V_ClosePrice - V_Cache.V_KLineData[LongMaLength].V_ClosePrice;

        bool isGreenKLine = V_Cache.V_KLineData[0].V_ClosePrice > V_Cache.V_KLineData[0].V_OpenPrice;

        //float longValue = isTest ? V_Cache.V_KLineData[0].V_HightPrice : V_Cache.V_KLineData[0].V_ClosePrice;
        //float shortValue = isTest? V_Cache.V_KLineData[0].V_LowPrice : V_Cache.V_KLineData[0].V_ClosePrice;

        float closeValue = V_Cache.V_KLineData[0].V_ClosePrice;
        float openValue = V_Cache.V_KLineData[0].V_OpenPrice;
        float highValue = V_Cache.V_KLineData[0].V_HightPrice;
        float lowValue = V_Cache.V_KLineData[0].V_LowPrice;

        KLine LastKLine = V_Cache.V_KLineData[1];

        if (isOrder)
        {
            if (MaKValue > 0 && openValue <= MaValue && closeValue >= MaValue)
            {
                return 1;
            }
            if (MaKValue < 0 && openValue >= MaValue && closeValue <= MaValue)
            {
                return -1;
            }
        }
        else
        {
            if (orderDir > 0)
            {
                if (MaKValue < 0 || (closeValue <= MaValue && !isGreenKLine))
                {
                    return 1;
                }
            }

            if (orderDir < 0)
            {
                if (MaKValue > 0 || (closeValue >= MaValue && isGreenKLine))
                {
                    return 1;
                }
            }
        }

        return 0;
    }

    #endregion
}