using System;
using System.Collections.Generic;
using System.Text;

public class BaseTaticsTestRunner
{
    /// <summary>
    /// 初始模拟金额
    /// </summary>
    protected float Init_Money = 5f;

    /// <summary>
    /// 当前金额
    /// </summary>
    protected float V_CurMoney = 5f;

    /// <summary>
    /// 当前持仓
    /// </summary>
    protected Position Position;

    /// <summary>
    /// 所有模拟数据
    /// </summary>
    protected List<KLine> Data_All;

    /// <summary>
    /// 当前计算数据缓存
    /// </summary>
    protected KLineCache Cur_Cache;

    /// <summary>
    /// 策略处理类
    /// </summary>
    protected BaseTaticsHelper helper;

    int curentIndex = 0;

    int count = 150;

    public virtual void SetHistoryData(List<KLine> data)
    {
        Data_All = data;
    }

    public virtual float Run()
    {
        if (count + curentIndex < Data_All.Count)
        {
            List<KLine> testData = new List<KLine>();
            testData.AddRange(Data_All.GetRange(Data_All.Count - 1 - count - curentIndex, count));
            Handle(testData);
            curentIndex++;
            return Run();
        }
        else
        {
            return V_CurMoney;
        }
    }

    public virtual void Handle(List<KLine> data)
    {

        KLine line = data[0];
        Cur_Cache.RefreshData(data);
        helper.V_Cache = Cur_Cache;

        if (Position == null)
        {
            //cd 中 ，不开单
            long leave = helper.GetCoolDown();
            if (leave < 0)
            {
                return;
            }

            int o = helper.MakeOrder();

            if (o > 0)
            {
                //多单
                OpenOrder(1, Cur_Cache.V_KLineData[0]);
            }
            else if (o < 0)
            {
                //空单
                OpenOrder(-1, Cur_Cache.V_KLineData[0]);
            }
        }
        else
        {
            //有单就算下是否需要平仓
            float v = Position.GetPercentTest(data[0]);
            if (helper.ShouldCloseOrder(Position.V_Dir, v))
            {
                CloseOrder(data[0]);
            }
        }
    }

    /// <summary>
    /// 开单
    /// </summary>
    public virtual void OpenOrder(int dir, KLine kline)
    {
        if (Position != null) { return; }

        Position = new Position("btc", dir, V_CurMoney * 0.2f, V_CurMoney * 0.2f, kline.V_ClosePrice, helper.V_Leverage, kline.V_Timestamp);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    public virtual void CloseOrder(KLine kline)
    {
        if (Position == null) { return; }

        float p = Position.GetPercentTest(kline);
        p = p < helper.V_LossPercent ? helper.V_LossPercent : p;

        float temp = 0;
        temp = p * 0.01f * Position.V_AllVol;
        V_CurMoney += temp;

        Position = null;
    }
}
