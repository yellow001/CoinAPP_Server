using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TaticsTestRunner
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

    public int V_OrderCount = 0;

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
            ////cd 中 ，不开单
            //long leave = helper.GetCoolDown();
            //if (leave < 0)
            //{
            //    return;
            //}

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

        V_OrderCount++;
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

    public static void TestRun(BaseTaticsHelper helper)
    {

        Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

        Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

        Dictionary<int, int> winDic = new Dictionary<int, int>();

        Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();

        Dictionary<int, Dictionary<int, float>> all_CountDic = new Dictionary<int, Dictionary<int, float>>();


        int allWinCount = 0;
        int allCount = 0;

        for (int loss = -15; loss >= -150; loss -= 5)
        {
            for (int win = 25; win <= 150; win += 5)
            {
                allCount++;
                TaticsTestRunner run = new TaticsTestRunner();
                run.Cur_Cache = new KLineCache();
                helper.SetStopPercent(loss, win);
                run.Data_All = helper.V_HistoryCache.V_KLineData;
                run.helper = helper;

                float money = run.Run();
                if (money > run.Init_Money)
                {
                    allWinCount++;

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

                if (!all_CountDic.ContainsKey(loss))
                {
                    Dictionary<int, float> temp = new Dictionary<int, float>();
                    all_CountDic[loss] = temp;
                }
                all_CountDic[loss][win] = run.V_OrderCount;

            }
        }

        Dictionary<int, int> final_Dic = new Dictionary<int, int>();
        if (lossCountDic.Count > 0)
        {
            int max_loss = lossCountDic.Values.Max();
            List<int> result_loss = lossCountDic.Where(q => q.Value == max_loss).Select(q => q.Key).ToList();

            foreach (var item in result_loss)
            {
                float max = 0;
                int win_temp = 0;
                if (lossWinDic.ContainsKey(item))
                {
                    foreach (var winItem in lossWinDic[item])
                    {
                        //if (winDic.ContainsKey(winItem))
                        //{
                        //    int count = winDic[winItem];
                        //    if (max < count)
                        //    {
                        //        max = count;
                        //        win_temp = winItem;
                        //    }
                        //}
                        float money = all_ResultDic[item][winItem];
                        if (max < money)
                        {
                            max = money;
                            win_temp = winItem;
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

        float allWinMoney = 0;
        foreach (var loss in all_ResultDic)
        {
            foreach (var win in loss.Value)
            {
                if (win.Value > 5) {
                    allWinMoney += win.Value;
                }
                Console.WriteLine("止损 {0} 止盈 {1} 开单次数 {2}  模拟剩余 {3}", loss.Key, win.Key, all_CountDic[loss.Key][win.Key], win.Value);
            }
        }

        Console.WriteLine("盈利情况：{0}/{1}   盈利平均值：{2}", allWinCount, allCount, allWinMoney / allWinCount);

        Console.WriteLine("最佳止盈止损百分比值: {0} {1} 开单次数 {2} 模拟剩余资金: {3}", loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]);
    }
}
