using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class MATacticsTest
{
    public float money = 5f;

    

    public Position order;

    public int length = 5;

    

    bool init = true;


    KLineCache cache;


    List<float> resultList_add = new List<float>();
    List<float> resultList_mul = new List<float>();
    List<float> resultList_zero = new List<float>();

    List<float> winList_add = new List<float>();
    List<float> winList_mul = new List<float>();

    List<float> klineList_add = new List<float>();
    List<float> klineList_mul = new List<float>();
    List<float> klineList_zero = new List<float>();

    /// <summary>
    /// 是否是止损平仓
    /// </summary>
    bool isLoss = false;

    public int curentIndex = 0;

    int count = 150;

    int per = 20;

    public List<KLine> data_all;

    TimeEventModel timeEvent;

    int orderCount = 0;


    public MATaticsHelper ma_helper = new MATaticsHelper();

    public MATacticsTest(KLineCache history) {
        cache = new KLineCache();
        //K线时长;采样点;周期(小_中_大);倍数

        ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}",15, 5, 5, 15, 30, 50));
        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 3, 5, 5, 15, 30, 50));
        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 1, 5, 5, 15, 30, 50));


        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 5, 5, 5, 15, 30, 20));
        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 3, 5, 5, 15, 30, 20));



        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 5, 5, 5, 15, 30, 30));
        //ma_helper.Init(string.Format("{0};{1};{2}_{3}_{4};{5}", 3, 5, 5, 15, 30, 30));

        ma_helper.RefreshHistory(history);
        //ma_helper.SetCycle(5, 10, 20);
        //ma_helper.SetLeverage(per);
    }

    public void Handle(List<KLine> data) {

        KLine line = data[0];
        float p = (line.V_ClosePrice - line.V_OpenPrice) / line.V_OpenPrice * per * 100;
        if (MathF.Abs(p) < 0.1f)
        {
            klineList_zero.Add(p);
        }
        else {
            if (line.V_OpenPrice > line.V_ClosePrice)
            {
                //跌
                klineList_mul.Add(p);
            }
            else
            {
                //涨
                klineList_add.Add(p);
            }
        }
        

        cache.RefreshData(data);
        ma_helper.V_Cache = cache;
        float result = ma_helper.GetResult();
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
                long leave = ma_helper.GetCoolDown();
                if (leave < 0&&!isLoss)
                {
                    //Console.WriteLine("冷却中 cd " + leave);
                    return;
                }
            }
            int o = ma_helper.MakeOrder();

            if (o > 0)
            {
                //多单
                //Console.WriteLine("result " + result+"  多单 "+cache.V_KLineData[0].V_ClosePrice);
                OpenOrder(1, cache.V_KLineData[0]);
            }
            else if (o < 0)
            {
                //空单
                //Console.WriteLine("result " + result + "  空单 " + cache.V_KLineData[0].V_ClosePrice);
                OpenOrder(-1, cache.V_KLineData[0]);
            }
        }
        else
        {
            //有单就算下是否需要平仓
            float v = order.GetPercentTest(data[0]);

            if (v > 0)
            {
                winList_add.Add(v);
            }
            else {
                winList_mul.Add(v);
            }

            if (ma_helper.ShouldCloseOrder(order.V_Dir, v))
            {
                CloseOrder(data[0]);
            }
        }
    }

    /// <summary>
    /// 开单
    /// </summary>
    void OpenOrder(int dir,KLine kline) {
        if (order != null) { return; }

        orderCount++;

        order = new Position("btc", dir, money * 0.2f, money * 0.2f, kline.V_ClosePrice,ma_helper.V_Leverage, kline.V_Timestamp);

        //lastTime = kline.timestamp;
        isLoss = false;

        //Console.WriteLine("{0}: price {1}", dir > 0 ? "long" : "short", kline.closePrice);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    void CloseOrder(KLine kline) {
        if (order == null) { return; }

        float p = order.GetPercentTest(kline);
        p = p < ma_helper.V_LossPercent ? ma_helper.V_LossPercent : p;

        float temp = 0;
        temp = p * 0.01f*order.V_AllVol;
        money += temp;

        if (init)
        {
            init = false;
        }

        //Console.WriteLine("平仓: price {0}，方向：{1}，盈利率{2},盈利{3}，剩余 {4}",
        //    kline.V_ClosePrice,
        //    order.V_Dir > 0 ? "long" : "short",
        //    p,
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
        //Console.WriteLine("percent add count:" + klineList_add.Count);
        //Console.WriteLine("percent mul count:" + klineList_mul.Count);
        //Console.WriteLine("percent zero count:" + klineList_zero.Count);

        //Console.WriteLine("percent add 均值:" + GetAv(klineList_add));
        //Console.WriteLine("percent mul 均值:" + GetAv(klineList_mul));
        //Console.WriteLine("percent zero 均值:" + GetAv(klineList_zero));

        //List<float> all = new List<float>();
        //all.AddRange(klineList_add);
        //all.AddRange(klineList_mul);
        //all.AddRange(klineList_zero);

        //Console.WriteLine("result all 均值:" + GetAv(all));

        //Console.WriteLine("\n-----------------------------------------------------\n");

        Console.WriteLine("win add count:" + winList_add.Count);
        Console.WriteLine("win mul count:" + winList_mul.Count);

        Console.WriteLine("win add 均值:" + GetAv(winList_add));
        Console.WriteLine("win mul 均值:" + GetAv(winList_mul));

        List<float> winAll = new List<float>();
        winAll.AddRange(winList_add);
        winAll.AddRange(winList_mul);

        //Console.WriteLine("win all count:" + winAll.Count);
        if (GetAv(winAll) > 0) {
            Console.WriteLine("win all 均值:" + GetAv(winAll));

            Console.WriteLine("loss {0} win {1} money {2} orderCount {3}", ma_helper.V_LossPercent, ma_helper.V_WinPercent, money, orderCount);
        }
    }

    public void Start() {
        //timeEvent = new TimeEventModel(0.001f, -1, Run);

        //TimeEventHandler.Ins.AddEvent(timeEvent);

        Run();
    }


    void Run()
    {
        if (count + curentIndex < data_all.Count)
        {
            List<KLine> testData = new List<KLine>();
            testData.AddRange(data_all.GetRange(data_all.Count - 1 - count - curentIndex, count));
            Handle(testData);
            curentIndex++;
            Run();
        }
        else
        {
            Over();
            //TimeEventHandler.Ins.RemoveEvent(timeEvent);
        }
    }

    public float GetAv(List<float> list) {

        if (list == null || list.Count == 0) {
            return 0;
        }

        float temp = 0;
        foreach (var item in list)
        {
            temp += item;
        }
        return temp / list.Count;
    }
}
