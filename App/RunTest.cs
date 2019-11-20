using System;
using System.Collections.Generic;
using System.Text;

public class RunTest
{
    public float money = 5f;

    public DateTime lastTime;

    public Order order;

    public float stopLossValue = -60f;

    public float stopWinValue = 150f;

    public long cd = 60*5*12;//秒

    bool init = true;


    KLineCache cache;

    MA ma;

    public RunTest() {
        cache = new KLineCache();
        ma = new MA();
    }

    public void Handle(List<KLine> data) {
        cache.SetData(data);
        ma.SetCache(cache);

        //有单就算下是否需要平仓
        if (order != null) {
            float v = order.GetPercent(data[0].closePrice);
            Console.WriteLine("当前价格 {0}，开仓价{1}，盈利率 {2}", data[0].closePrice,order.price, v);
            if (ShouldCloseOrder(v)) {
                CloseOrder(data[0]);
            }
        }

        OnHandle();

        //if (init)
        //{
        //    OnHandle();
        //}
        //else {
        //    if ((data[0].timestamp - lastTime).Ticks >= cd*10000 *1000)
        //    {
        //        OnHandle();
        //    }
        //    else {
        //        Console.WriteLine("冷却中 剩余 {0}", (data[0].timestamp - lastTime).Ticks - cd * 10000 * 1000);
        //    }
        //}
    }

    void OnHandle() {
        //是否有单

        //无

        ////获取结果
        /////大于等于3
        /////多

        /////小于等于-3
        /////空



        //有
        ////获取结果
        /////大于等于3
        /////多

        /////小于等于-3
        /////空

        float result = MAHelper.GetResult(ma, 5);
        Console.WriteLine("result " + result);

        if (order == null)
        {
            //cd 中 ，不开单
            if (!init) {
                long leave = (cache.kLineData[0].timestamp - lastTime).Ticks - cd * 10000 * 1000;
                if (leave<0) {
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
        //else
        //{
        //    if (result >= 3)
        //    {
                
        //        if (order.dir > 0)
        //        {
        //            //有多单
        //        }
        //        else {
        //            //有空单

        //        }


        //    }
        //    else if (result <= -3)
        //    {
        //        if (order.dir > 0)
        //        {
        //            //有多单
        //            //判断止损还是继续等待
        //        }
        //        else
        //        {
        //            //有空单

        //        }
        //    }
        //}

    }

    public bool ShouldCloseOrder(float v) {

        bool result = false;

        if (v >= stopWinValue) {
            result = true;
        }

        if (v <= stopLossValue) {
            result = true;
        }

        return result;
    }

    /// <summary>
    /// 开单
    /// </summary>
    void OpenOrder(int dir,KLine kline) {
        if (order != null) { return; }

        order = new Order(dir, money * 0.2f, kline.closePrice, 50, kline.timestamp);

        //lastTime = kline.timestamp;

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
