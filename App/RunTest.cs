using System;
using System.Collections.Generic;
using System.Text;

public class RunTest
{
    public float money = 5f;

    public DateTime lastTime;

    public Order order;

    public float stopLossValue = -50f;

    public float stopWinValue = 150f;

    public long cd = 60*5*9;//秒

    bool init = true;


    KLineCache cache;

    MA ma;

    /// <summary>
    /// 是否是止损平仓
    /// </summary>
    bool isLoss = false;

    public RunTest() {
        cache = new KLineCache();
        ma = new MA();
    }

    public void Handle(List<KLine> data) {
        cache.SetData(data);
        ma.SetCache(cache);

        float result = MAHelper.GetResult(ma, 8);
        Console.WriteLine("result " + result);

        if (order == null)
        {
            //cd 中 ，不开单
            if (!init)
            {
                long leave = (cache.kLineData[0].timestamp - lastTime).Ticks - cd * 10000 * 1000;
                if (leave < 0&&!isLoss)
                {
                    Console.WriteLine("冷却中 cd " + leave);
                    return;
                }
            }

            if (result >= 3)
            {
                //多单
                OpenOrder(1, cache.kLineData[0]);
            }
            else if (result <= -3)
            {
                //空单
                OpenOrder(-1, cache.kLineData[0]);
            }
        }
        else
        {
            //有单就算下是否需要平仓
            float v = order.GetPercent(data[0].closePrice);
            Console.WriteLine("当前价格 {0}，开仓价{1}，盈利率 {2}", data[0].closePrice, order.price, v);
            if (ShouldCloseOrder(result))
            {
                CloseOrder(data[0]);
            }
        }
    }
    

    public bool ShouldCloseOrder(float result) {

        if (order == null) { return false; }


        //计算 盈利率
        float v = order.GetPercent(cache.kLineData[0].closePrice);

        if (v <= stopLossValue)
        {
            //无条件止损
            return true;
        }
        else {

            //if (v <= stopLossValue * 0.5f)
            //{
            //    //亏损率达止损的50%，判断继续持有还是平仓
            //    if (order.dir > 0)
            //    {
            //        //多单 亏损
            //        if (result < -3)
            //        {
            //            //有较为强烈的空头排列信号
            //            isLoss = true;
            //            return true;
            //        }
            //    }
            //    else
            //    {
            //        //空单 亏损
            //        if (result > 3)
            //        {
            //            //有较为强烈的多头排列信号
            //            isLoss = true;
            //            return true;
            //        }
            //    }
            //}

            if (v >= stopWinValue) {
                //达到 止盈后，判断继续持有还是平仓
                if (order.dir > 0)
                {
                    //多单盈利
                    if (result < 2)
                    {
                        //多头排列信号弱
                        return true;
                    }
                }
                else {
                    //空单盈利
                    if (result >-2)
                    {
                        //空头排列信号弱
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 开单
    /// </summary>
    void OpenOrder(int dir,KLine kline) {
        if (order != null) { return; }

        order = new Order(dir, money * 0.2f, kline.closePrice, 50, kline.timestamp);

        //lastTime = kline.timestamp;
        isLoss = false;

        Console.WriteLine("{0}: price {1}", dir > 0 ? "long" : "short", kline.closePrice);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    void CloseOrder(KLine kline) {
        if (order == null) { return; }

        float p = order.GetPercent(kline.closePrice);

        money += order.GetWin(kline.closePrice);

        lastTime = kline.timestamp;

        if (init)
        {
            init = false;
        }

        Console.WriteLine("平仓: price {0}，方向：{1}，盈利率{2},盈利{3}，剩余 {4}",
            kline.closePrice,
            order.dir>0?"long":"short",
            p, 
            order.GetWin(kline.closePrice),
            money);

        order = null;
    }

    public float GetMoney() {
        return money;
    }
}
