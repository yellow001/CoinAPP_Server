using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class RunTest
{
    public float money = 5f;

    public DateTime lastTime;

    public Order order;

    public float stopLossValue = -30f;

    public float stopWinValue = 20f;

    public int length = 5;

    public long cd = 60*5;//秒

    bool init = true;


    KLineCache cache;

    MA ma;


    List<float> resultList_add = new List<float>();
    List<float> resultList_mul = new List<float>();
    List<float> resultList_zero = new List<float>();

    List<float> winList_add = new List<float>();
    List<float> winList_mul = new List<float>();


    /// <summary>
    /// 是否是止损平仓
    /// </summary>
    bool isLoss = false;

    public int curentIndex = 0;

    int count = 150;

    public List<KLine> data_all;

    TimeEventModel timeEvent;


    public RunTest() {
        cache = new KLineCache();
        ma = new MA();
        cd *= length;
        //MAHelper.SetCycle(10, 15, 30);
    }

    public void Handle(List<KLine> data) {
        cache.SetData(data);
        ma.SetCache(cache);

        float result = MAHelper.GetResult(ma, length);
        //Console.WriteLine("result " + result);
        if ((MathF.Abs(result) - 0.01f) > 0)
        {
            if (result > 0)
            {
                resultList_add.Add(result);
            }
            else {
                resultList_mul.Add(result);
            }
        }
        else {
            resultList_zero.Add(result);
        }

        if (order == null)
        {
            //cd 中 ，不开单
            if (!init)
            {
                long leave = (cache.kLineData[0].timestamp - lastTime).Ticks - cd * 10000 * 1000;
                if (leave < 0&&!isLoss)
                {
                    //Console.WriteLine("冷却中 cd " + leave);
                    return;
                }
            }

            if (result >= 0.65f)
            {
                //多单
                OpenOrder(1, cache.kLineData[0]);
            }
            else if (result <= -0.52f)
            {
                //空单
                OpenOrder(-1, cache.kLineData[0]);
            }
        }
        else
        {
            //有单就算下是否需要平仓
            float v = order.GetPercent(data[0].hightPrice,data[0].lowPrice);
            //Console.WriteLine("当前价格 {0}，开仓价{1}，盈利率 {2}", data[0].closePrice, order.price, v);

            if (v > 0)
            {
                winList_add.Add(v);
            }
            else {
                winList_mul.Add(v);
            }

            if (ShouldCloseOrder(result))
            {
                CloseOrder(data[0]);
            }
        }
    }
    

    public bool ShouldCloseOrder(float result) {

        if (order == null) { return false; }


        //计算 盈利率
        float v = order.GetPercent(cache.kLineData[0].hightPrice,cache.kLineData[0].lowPrice);

        if (v <= stopLossValue)
        {
            //无条件止损
            return true;
        }
        else {

            if (v <= stopLossValue * 0.8f)
            {
                //亏损率达止损的80%，判断继续持有还是平仓
                if (order.dir > 0)
                {
                    //多单 亏损
                    if (result < -0.52f)
                    {
                        //有较为强烈的空头排列信号
                        //isLoss = true;
                        return true;
                    }
                }
                else
                {
                    //空单 亏损
                    if (result > 0.65f)
                    {
                        //有较为强烈的多头排列信号
                        //isLoss = true;
                        return true;
                    }
                }
            }

            if (v >= stopWinValue) {
                //达到 止盈后，判断继续持有还是平仓
                if (order.dir > 0)
                {
                    //多单盈利
                    if (result < 0.5f)
                    {
                        //多头排列信号弱
                        return true;
                    }
                }
                else {
                    //空单盈利
                    if (result >-0.48f)
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

        order = new Order(dir, money * 0.3f, kline.closePrice, 30, kline.timestamp);

        //lastTime = kline.timestamp;
        isLoss = false;

        //Console.WriteLine("{0}: price {1}", dir > 0 ? "long" : "short", kline.closePrice);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    void CloseOrder(KLine kline) {
        if (order == null) { return; }

        float p = order.GetPercent(kline.hightPrice,kline.lowPrice);

        float temp = 0;
        if (p < 0 && p < stopLossValue)
        {
            temp = stopLossValue * 0.01f * order.money;
        }
        else
        {
            temp = order.GetWin(kline.hightPrice, kline.lowPrice);
        }
        money += temp;

        lastTime = kline.timestamp;

        if (init)
        {
            init = false;
        }

        //Console.WriteLine("平仓: price {0}，方向：{1}，盈利率{2},盈利{3}，剩余 {4}",
        //    kline.closePrice,
        //    order.dir>0?"long":"short",
        //    p<stopLossValue?stopLossValue:p, 
        //    temp,
        //    money);

        order = null;
    }

    public float GetMoney() {
        return money;
    }

    public void Over() {
        //Console.WriteLine("result add count:" + resultList_add.Count);
        //Console.WriteLine("result mul count:" + resultList_mul.Count);
        //Console.WriteLine("result zero count:" + resultList_zero.Count);

        //Console.WriteLine("result add 均值:" + GetAv(resultList_add));
        //Console.WriteLine("result mul 均值:" + GetAv(resultList_mul));
        //Console.WriteLine("result zero 均值:" + GetAv(resultList_zero));

        //List<float> resultAll = new List<float>();
        //resultAll.AddRange(resultList_add);
        //resultAll.AddRange(resultList_mul);
        //resultAll.AddRange(resultList_zero);
        //Console.WriteLine("result all count:" + resultAll.Count);

        //Console.WriteLine("result all 均值:" + GetAv(resultAll));

        //Console.WriteLine("\n-----------------------------------------------------\n");

        //Console.WriteLine("win add count:" + winList_add.Count);
        //Console.WriteLine("win mul count:" + winList_mul.Count);

        //Console.WriteLine("win add 均值:" + GetAv(winList_add));
        //Console.WriteLine("win mul 均值:" + GetAv(winList_mul));

        //List<float> winAll = new List<float>();
        //winAll.AddRange(winList_add);
        //winAll.AddRange(winList_mul);

        //Console.WriteLine("win all count:" + winAll.Count);

        //Console.WriteLine("win all 均值:" + GetAv(winAll));

        Console.WriteLine("loss {0} win {1} money {2}",stopLossValue,stopWinValue,money);
    }

    public void Start() {
        timeEvent = new TimeEventModel(0.001f, -1, Run);

        TimeEventHandler.Ins.AddEvent(timeEvent);
    }


    void Run()
    {
        if (count + curentIndex < data_all.Count)
        {
            List<KLine> testData = new List<KLine>();
            testData.AddRange(data_all.GetRange(data_all.Count - 1 - count - curentIndex, count));
            Handle(testData);
            curentIndex++;
        }
        else
        {
            Over();
            TimeEventHandler.Ins.RemoveEvent(timeEvent);
        }
    }
}
