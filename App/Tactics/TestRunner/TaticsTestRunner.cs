using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TaticsTestRunner
{
    /// <summary>
    /// 初始模拟金额
    /// </summary>
    static float Init_Money = 5f;

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

    float orderPercent = 0.2f;

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

            if (curentIndex == 0) {
                helper.V_LastOpTime = testData[0].V_Timestamp;
            }

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
            //cd 中 且上次是盈利平仓，不开单
            long leave = helper.GetCoolDownTest();
            if (leave < 0 && helper.winClose)
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
            if (helper.ShouldCloseOrderTest(Position.V_Dir, v,Cur_Cache.V_KLineData[0]))
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
        //Console.WriteLine("{0}  :  开仓:{1} 价格:{2}", kline.V_Timestamp, dir > 0 ? "多" : "空", kline.V_ClosePrice);
        Position = new Position("btc", dir, V_CurMoney * orderPercent, V_CurMoney * orderPercent, kline.V_ClosePrice, helper.V_Leverage, kline.V_Timestamp);
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

        //Console.WriteLine("{0}  :  平仓价格:{1}  盈利：{2}", kline.V_Timestamp, kline.V_ClosePrice,temp);

        Position = null;
    }

    public virtual void Clear() {
        V_OrderCount = 0;
    }

    public static void TestRun(BaseTaticsHelper helper)
    {
        int win=0, loss=0,count=999999;
        float avg_win = 0;
        if (helper is ICycleTatics)
        {
            float maxMoney = 0;
            string best_Cycle="";
            float avg_win_max = 0;

            string[] cycleList = AppSetting.Ins.GetValue("CycleList").Split(';');
            for (int i = 0; i < cycleList.Length; i++)
            {
                ((ICycleTatics)helper).SetCycle(cycleList[i]);
                int temp_loss = 0, temp_win = 0, temp_count=0;
                float temp = OnTestRun(helper,ref temp_loss,ref temp_win,ref avg_win_max, ref temp_count);

                //if (maxMoney < temp)
                //{
                //    maxMoney = temp;
                //    best_Cycle = cycleList[i];
                //    loss = temp_loss;
                //    win = temp_win;
                //    avg_win = avg_win_max;
                //    count = temp_count;
                //}

                //if (avg_win < avg_win_max)
                //{
                //    maxMoney = temp;
                //    best_Cycle = cycleList[i];
                //    loss = temp_loss;
                //    win = temp_win;
                //    avg_win = avg_win_max;
                //    count = temp_count;
                //}

                if (temp > TaticsTestRunner.Init_Money) {
                    if (count > temp_count)
                    {
                        maxMoney = temp;
                        best_Cycle = cycleList[i];
                        loss = temp_loss;
                        win = temp_win;
                        avg_win = avg_win_max;
                        count = temp_count;
                    }
                }
                

                //Console.WriteLine(helper.V_Instrument_id+": 周期 " + cycleList[i]);
            }

            if (string.IsNullOrEmpty(best_Cycle)) {
                best_Cycle = cycleList[0];
                loss = -50;
                win = 100;
                Console.WriteLine("{0}:无盈利方案，将采用默认值",helper.V_Instrument_id);
                Debugger.Warn(string.Format("{0}:无盈利方案，将采用默认值", helper.V_Instrument_id));
            }

            ((ICycleTatics)helper).SetCycle(best_Cycle);
            helper.SetStopPercent(loss, win);
            Console.WriteLine("{0}:最佳周期 {1} 止损 {2}  止盈{3}  剩余{4}  盈利平均值 {5}  开单次数{6}", helper.V_Instrument_id ,best_Cycle, loss, win, maxMoney,avg_win,count);

            Debugger.Warn(string.Format("{0}:最佳周期 {1} 止损 {2}  止盈{3}  剩余{4}  盈利平均值 {5}  开单次数{6}", helper.V_Instrument_id, best_Cycle, loss, win, maxMoney, avg_win, count));
        }
        else {
            OnTestRun(helper,ref loss,ref win,ref avg_win, ref count);
        }
    }

    private static float OnTestRun(BaseTaticsHelper helper,ref int loss_result,ref int win_result,ref float avg_win,ref int orderCount)
    {
        Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

        Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

        Dictionary<int, int> winDic = new Dictionary<int, int>();

        Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();

        Dictionary<int, Dictionary<int, int>> all_CountDic = new Dictionary<int, Dictionary<int, int>>();

        int allWinCount = 0;
        int allCount = 0;

        //int minLoss = (int)MathF.Floor(helper.V_Leverage * -0.618f);
        //int maxLoss = (int)MathF.Ceiling(helper.V_Leverage * -1.618f);
        //minLoss = minLoss > -25 ? -25 : minLoss;
        //maxLoss = maxLoss > minLoss ? minLoss * 2 : maxLoss;

        //int maxWin = (int)Math.Floor(helper.V_Leverage * 3.618f);
        //maxWin = maxWin < 40 ? 40 : maxWin;

        int minLoss = -60;
        int maxLoss = -80;
        int maxWin = 120;

        for (int loss = minLoss; loss >= maxLoss; loss -= 5)
        {
            int start = Math.Abs(loss) - 20;
            start = start < 60 ? 60 : start;
            for (int win = start; win <= maxWin; win += 5)
            {
                allCount++;
                TaticsTestRunner run = new TaticsTestRunner();
                run.Cur_Cache = new KLineCache();
                helper.SetStopPercent(loss, win);
                run.Data_All = helper.V_HistoryCache.V_KLineData;
                run.helper = helper;

                run.Clear();
                float money = run.Run();
                if (money > TaticsTestRunner.Init_Money)
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
                    Dictionary<int, int> temp = new Dictionary<int, int>();
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
                        //    int money = winDic[winItem];
                        //    if (max < money)
                        //    {
                        //        max = money;
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

        int loss_final = 0, win_final = 0,orderCount_final=0;
        float maxMoney = 0;
        foreach (var item in final_Dic)
        {
            float value = all_ResultDic[item.Key][item.Value];
            if (maxMoney < value)
            {
                maxMoney = value;
                loss_final = item.Key;
                win_final = item.Value;
                orderCount_final = all_CountDic[item.Key][item.Value];
            }
        }
        helper.SetStopPercent(loss_final, win_final);
        helper.ClearTempData();

        float allWinMoney = 0;
        foreach (var loss in all_ResultDic)
        {
            foreach (var win in loss.Value)
            {
                if (win.Value > 5)
                {
                    allWinMoney += win.Value;
                }
                //Console.WriteLine("止损 {0} 止盈 {1} 开单次数 {2}  模拟剩余 {3}", loss.Key, win.Key, all_CountDic[loss.Key][win.Key], win.Value);
            }
        }
        if (!(helper is ICycleTatics)) {
            Console.WriteLine("{0}:盈利情况：{1}/{2}   盈利平均值：{3}", helper.V_Instrument_id, allWinCount, allCount, allWinMoney / allWinCount);

            Debugger.Warn(string.Format("{0}:盈利情况：{1}/{2}   盈利平均值：{3}", helper.V_Instrument_id, allWinCount, allCount, allWinMoney / allWinCount));

            Console.WriteLine("{0}:最佳止盈止损百分比值: {1} {2} 开单次数 {3} \n模拟剩余资金: {4}", helper.V_Instrument_id, loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]);

            Debugger.Warn(string.Format("{0}:最佳止盈止损百分比值: {1} {2} 开单次数 {3} \n模拟剩余资金: {4}", helper.V_Instrument_id, loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]));
        }

        avg_win = allWinMoney / allWinCount;

        loss_result = loss_final;
        win_result = win_final;
        orderCount = orderCount_final;

        if (loss_final == 0 || win_final == 0) {
            return 0;
        }
        return all_ResultDic[loss_final][win_final];
    }
}
