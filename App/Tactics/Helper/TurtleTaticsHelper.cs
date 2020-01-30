/**
 * 海龟交易法则 简化版
 * V1.0 2019-11-26
 * 
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
/// 海龟交易 简化版
/// </summary>
public class TurtleTaticsHelper : BaseTaticsHelper
{
    /// <summary>
    /// 买入采样点
    /// </summary>
    public int V_BuyLength;

    /// <summary>
    /// 卖出采样点
    /// </summary>
    public int V_SellLength;


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
            V_Min = int.Parse(strs[1]);

            string[] samples = strs[2].Split('_');
            if (samples.Length >= 2)
            {
                V_BuyLength = int.Parse(samples[0]);
                V_SellLength = int.Parse(samples[1]);
            }
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

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder()
    {
        return GetResult();
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

        int result = GetResult();
        if (percent > winPercent) {
            if (dir > 0)
            {
                //多单盈利
                if (result < 0) {
                    return true;
                }
            }
            else {
                //空单盈利
                if (result > 0) {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion


    #region 策略方法


    int GetResult() {

        //买入？
        if (V_Cache != null && V_Cache.V_KLineData != null && V_Cache.V_KLineData.Count > V_BuyLength + 1)
        {
            //最新的一条k线要剔除
            List<KLine> data = V_Cache.V_KLineData.GetRange(1, V_BuyLength);
            List<float> list = data.Select(q => q.V_HightPrice).ToList();
            float avg = Util.GetAvg(list);
            if (V_Cache.V_KLineData[0].V_ClosePrice > avg)
            {
                return 1;
            }
        }

        //卖出？
        if (V_Cache != null && V_Cache.V_KLineData != null && V_Cache.V_KLineData.Count > V_SellLength + 1)
        {
            //最新的一条k线要剔除
            List<KLine> data = V_Cache.V_KLineData.GetRange(1, V_SellLength);
            List<float> list = data.Select(q => q.V_LowPrice).ToList();
            float avg = Util.GetAvg(list);
            if (V_Cache.V_KLineData[0].V_ClosePrice < avg)
            {
                return -1;
            }
        }

        return 0;
    }
    
    #endregion
}
