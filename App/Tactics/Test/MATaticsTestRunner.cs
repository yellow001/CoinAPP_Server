using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MATaticsTestRunner
{
    /// <summary>
    /// 初始模拟金额
    /// </summary>
    float init_Money = 5f;

    /// <summary>
    /// 当前金额
    /// </summary>
    float money = 5f;

    /// <summary>
    /// 当前持仓
    /// </summary>
    Position position;

    /// <summary>
    /// 所有模拟数据
    /// </summary>
    List<KLine> data_all;

    /// <summary>
    /// 当前计算数据缓存
    /// </summary>
    KLineCache cache;

    /// <summary>
    /// 策略处理类
    /// </summary>
    MATaticsHelper ma_helper;

    int curentIndex = 0;

    int count = 150;

    void SetHistoryData(List<KLine> data) {
        data_all = data;
    }

    float Run()
    {
        if (count + curentIndex < data_all.Count)
        {
            List<KLine> testData = new List<KLine>();
            testData.AddRange(data_all.GetRange(data_all.Count - 1 - count - curentIndex, count));
            Handle(testData);
            curentIndex++;
            return Run();
        }
        else
        {
            return money;
        }
    }

    void Handle(List<KLine> data)
    {

        KLine line = data[0];
        cache.RefreshData(data);
        ma_helper.V_Cache = cache;
        float result = ma_helper.GetResult();

        if (position == null)
        {
            //cd 中 ，不开单
            long leave = ma_helper.GetCoolDown();
            if (leave < 0)
            {
                return;
            }

            int o = ma_helper.MakeOrder();

            if (o > 0)
            {
                //多单
                OpenOrder(1, cache.V_KLineData[0]);
            }
            else if (o < 0)
            {
                //空单
                OpenOrder(-1, cache.V_KLineData[0]);
            }
        }
        else
        {
            //有单就算下是否需要平仓
            float v = position.GetPercentTest(data[0]);
            if (ma_helper.ShouldCloseOrder(position.V_Dir, v))
            {
                CloseOrder(data[0]);
            }
        }
    }

    /// <summary>
    /// 开单
    /// </summary>
    void OpenOrder(int dir, KLine kline)
    {
        if (position != null) { return; }

        position = new Position("btc", dir, money * 0.2f, money * 0.2f, kline.V_ClosePrice, ma_helper.V_Leverage, kline.V_Timestamp);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    void CloseOrder(KLine kline)
    {
        if (position == null) { return; }

        float p = position.GetPercentTest(kline);
        p = p < ma_helper.V_LossPercent ? ma_helper.V_LossPercent : p;

        float temp = 0;
        temp = p * 0.01f * position.V_AllVol;
        money += temp;

        position = null;
    }


    public static void TestRun(MATaticsHelper helper) {

        Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

        Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

        Dictionary<int, int> winDic = new Dictionary<int, int>();

        Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();


        for (int loss = -30; loss >= -150; loss -= 5)
        {
            for (int win = 30; win <= 150; win += 5)
            {
                MATaticsTestRunner run = new MATaticsTestRunner();
                run.cache = new KLineCache();
                run.ma_helper = helper;
                run.ma_helper.SetStopPercent(loss, win);
                run.data_all = helper.V_HistoryCache.V_KLineData;

                float money = run.Run();
                if (money > run.init_Money)
                {
                    if (!lossCountDic.ContainsKey(loss)) { lossCountDic[loss] = 0; }
                    lossCountDic[loss]++;

                    if (!winDic.ContainsKey(win)) { winDic[win] = 0; }
                    winDic[win]++;

                    if (!lossWinDic.ContainsKey(loss))
                    {
                        List<int> temp = new List<int>();
                        lossWinDic[loss] = temp;
                    }
                    if (!lossWinDic[loss].Contains(win))
                    {
                        lossWinDic[loss].Add(win);
                    }
                }

                if (!all_ResultDic.ContainsKey(loss))
                {
                    Dictionary<int, float> temp = new Dictionary<int, float>();
                    all_ResultDic[loss] = temp;
                }
                all_ResultDic[loss][win] = money;

            }
        }

        Dictionary<int, int> final_Dic = new Dictionary<int, int>();
        if (lossCountDic.Count > 0)
        {
            int max_loss = lossCountDic.Values.Max();
            List<int> result_loss = lossCountDic.Where(q => q.Value == max_loss).Select(q => q.Key).ToList();

            foreach (var item in result_loss)
            {
                int max = 0;
                int win_temp = 0;
                if (lossWinDic.ContainsKey(item))
                {
                    foreach (var winItem in lossWinDic[item])
                    {
                        if (winDic.ContainsKey(winItem))
                        {
                            int count = winDic[winItem];
                            if (max < count)
                            {
                                max = count;
                                win_temp = winItem;
                            }
                        }
                    }
                }
                final_Dic[item] = win_temp;
            }
        }

        int loss_final = 0, win_final = 0;
        float maxMoney = 0;
        foreach (var item in final_Dic)
        {
            float value = all_ResultDic[item.Key][item.Value];
            if (maxMoney < value)
            {
                maxMoney = value;
                loss_final = item.Key;
                win_final = item.Value;
            }
        }
        helper.SetStopPercent(loss_final, win_final);
        helper.ClearTempData();
        Console.WriteLine("最佳止盈止损百分比值: {0} {1}  模拟剩余资金: {2}", loss_final, win_final,all_ResultDic[loss_final][win_final]);
    }
}
