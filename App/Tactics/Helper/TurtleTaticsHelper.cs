using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/**
 * 海龟交易法则 简化版
 * V1.0 2019-11-26
 * 
 * 
 * 
 * **/
public class TurtleTaticsHelper : BaseTaticsHelper
{
    /// <summary>
    /// 买入采样点
    /// </summary>
    public int V_BuyLength = 5;

    /// <summary>
    /// 卖出采样点
    /// </summary>
    public int V_SellLength = 5;


    #region 重载

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {

    }

    /// <summary>
    /// 分析历史数据
    /// </summary>
    /// <returns></returns>
    public override async Task RunHistory()
    {
        await base.RunHistory();

        Console.WriteLine("分析结果");

        Console.WriteLine("分析历史数据完毕");
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder()
    {
        return base.MakeOrder();
    }

    /// <summary>
    /// 是否需要平仓
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public override bool ShouldCloseOrder(int dir, float percent)
    {
        return base.ShouldCloseOrder(dir, percent);
    }
    #endregion
}
