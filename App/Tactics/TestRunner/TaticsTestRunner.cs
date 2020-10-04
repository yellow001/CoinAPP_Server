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
    protected List<Position> V_Positions=new List<Position>();

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

    int count = 300;

    float orderPercent = 0.3236f;

    public virtual void SetHistoryData(List<KLine> data)
    {
        Data_All = data;
    }

    public virtual float Run()
    {
        orderPercent = float.Parse(AppSetting.Ins.GetValue("OrderValue"));
        if (count + curentIndex < Data_All.Count)
        {
            List<KLine> testData = new List<KLine>();
            testData.AddRange(Data_All.GetRange(Data_All.Count - 1 - count - curentIndex, count));

            if (curentIndex == 0)
            {
                helper.V_LastOpTime = Data_All[Data_All.Count - 1].V_Timestamp;
            }

            Handle(testData);
            curentIndex++;
            return Run();
        }
        else
        {
            List<int> closeList = V_Positions.Select((item) => item.V_Dir).ToList();

            foreach (var item in closeList)
            {
                CloseOrder(Data_All[0], item);
            }

            return V_CurMoney;
        }
    }

    public virtual void Handle(List<KLine> data)
    {

        KLine line = data[0];
        Cur_Cache.RefreshData(data);
        helper.V_Cache = Cur_Cache;

        bool hasLong = false, hasShort = false;
        float longPercent = 0, shortPrecent = 0;

        //if (V_Positions.Count==0)
        //{
        //    //cd 中 且上次是盈利平仓，不开单
        //    long leave = helper.GetCoolDownTest();
        //    if (leave < 0 && helper.winClose)
        //    {
        //        return;
        //    }

        //    int o = helper.MakeOrder();

        //    if (o > 0)
        //    {
        //        //多单
        //        OpenOrder(1, Cur_Cache.V_KLineData[0]);
        //    }
        //    else if (o < 0)
        //    {
        //        //空单
        //        OpenOrder(-1, Cur_Cache.V_KLineData[0]);
        //    }
        //}
        //else
        //{
        //    //有单就算下是否需要平仓
        //    float v = V_Positions[0].GetPercentTest(data[0]);
        //    if (helper.ShouldCloseOrderTest(V_Positions[0].V_Dir, v, Cur_Cache.V_KLineData[0]))
        //    {
        //        CloseOrder(data[0], V_Positions[0].V_Dir);
        //    }
        //}



        if (V_Positions != null && V_Positions.Count > 0)
        {
            List<int> closeList = new List<int>();
            foreach (var item in V_Positions)
            {
                if (item.V_Dir > 0)
                {
                    hasLong = true;
                }
                else
                {
                    hasShort = true;
                }

                //有单就算下是否需要平仓
                float v = item.GetPercentTest(data[0],helper.V_LossPercent);

                if (item.V_Dir > 0)
                {
                    longPercent = v;
                }
                else
                {
                    shortPrecent = v;
                }

                if (helper.ShouldCloseOrderTest(item.V_Dir, v, Cur_Cache.V_KLineData[0]))
                {
                    if (item.V_Dir > 0)
                    {
                        hasLong = false;
                    }
                    else
                    {
                        hasShort = false;
                    }
                    closeList.Add(item.V_Dir);
                }
            }

            foreach (var item in closeList)
            {
                CloseOrder(data[0], item);
            }

        }

        bool makeOrder = false;
        //bool Double = AppSetting.Ins.GetInt("DoubleDir") > 0;
        bool Double = false;
        if (Double)
        {
            makeOrder = !hasShort || !hasLong;
        }
        else
        {
            makeOrder = !hasShort && !hasLong;
        }


        if (makeOrder)
        {
            long leave = helper.GetCoolDownTest();
            if (leave < 0)
            {
                return;
            }

            int o = helper.MakeOrder(true);

            if (o > 0 && !hasLong)
            {
                if (hasShort && shortPrecent > 0)
                {
                    return;
                }
                //多单
                helper.V_LastOpTime = helper.V_Cache.V_KLineData[0].V_Timestamp;
                OpenOrder(1, Cur_Cache.V_KLineData[0]);
            }
            else if (o < 0 && !hasShort)
            {
                if (hasLong && longPercent > 0)
                {
                    return;
                }
                //空单
                helper.V_LastOpTime = helper.V_Cache.V_KLineData[0].V_Timestamp;
                OpenOrder(-1, Cur_Cache.V_KLineData[0]);
            }
        }

        #region 旧逻辑
        //if (Position == null)
        //{
        //    //cd 中 且上次是盈利平仓，不开单
        //    long leave = helper.GetCoolDownTest();
        //    if (leave < 0 && helper.winClose)
        //    {
        //        return;
        //    }

        //    int o = helper.MakeOrder();

        //    if (o > 0)
        //    {
        //        //多单
        //        OpenOrder(1, Cur_Cache.V_KLineData[0]);
        //    }
        //    else if (o < 0)
        //    {
        //        //空单
        //        OpenOrder(-1, Cur_Cache.V_KLineData[0]);
        //    }
        //}
        //else
        //{
        //    //有单就算下是否需要平仓
        //    float v = Position.GetPercentTest(data[0]);
        //    if (helper.ShouldCloseOrderTest(Position.V_Dir, v, Cur_Cache.V_KLineData[0]))
        //    {
        //        CloseOrder(data[0]);
        //    }
        //}
        #endregion


    }

    /// <summary>
    /// 开单
    /// </summary>
    public virtual void OpenOrder(int dir, KLine kline)
    {
        V_OrderCount++;
        //Console.WriteLine("{0}  :  开仓:{1} 价格:{2} 资金：{3}", kline.V_Timestamp, dir > 0 ? "多" : "空", kline.V_ClosePrice, V_CurMoney);

        float price = kline.V_ClosePrice;
        //if (dir > 0)
        //{
        //    price = kline.V_HightPrice;
        //}
        //else {
        //    price = kline.V_LowPrice;
        //}
        Position position = new Position("btc", dir, V_CurMoney * orderPercent, V_CurMoney * orderPercent, price, helper.V_Leverage, kline.V_Timestamp);
        V_Positions.Add(position);
    }

    /// <summary>
    /// 平仓
    /// </summary>
    public virtual void CloseOrder(KLine kline,int dir)
    {

        if (V_Positions == null || V_Positions.Count <= 0) {
            return;
        }

        Position removeItem = null;

        foreach (var item in V_Positions)
        {
            if (item.V_Dir == dir) {
                removeItem = item;
                break;
            }
        }

        if (removeItem == null) {
            return;
        }

        float p = removeItem.GetPercentTest(kline, helper.V_LossPercent);
        p = p < helper.V_LossPercent ? helper.V_LossPercent : p;

        float temp = 0;
        temp = p * 0.01f * removeItem.V_AllVol;
        V_CurMoney += temp;

        //Console.WriteLine("{0}  :  平仓价格:{1}  盈利：{2}  资金：{3}", kline.V_Timestamp, kline.V_ClosePrice,temp,V_CurMoney);

        V_Positions.Remove(removeItem);
    }

    public virtual void Clear() {
        V_OrderCount = 0;
        V_CurMoney = Init_Money;
        helper.ClearRunData();
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

                if (avg_win < avg_win_max)
                {
                    maxMoney = temp;
                    best_Cycle = cycleList[i];
                    loss = temp_loss;
                    win = temp_win;
                    avg_win = avg_win_max;
                    count = temp_count;
                }

                //if (temp > TaticsTestRunner.Init_Money)
                //{
                //    if (count > temp_count)
                //    {
                //        maxMoney = temp;
                //        best_Cycle = cycleList[i];
                //        loss = temp_loss;
                //        win = temp_win;
                //        avg_win = avg_win_max;
                //        count = temp_count;
                //    }
                //}


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
        Dictionary<int, List<float>> winResultDic = new Dictionary<int, List<float>>();
        Dictionary<int, List<float>> lossResultDic = new Dictionary<int, List<float>>();


        Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();

        Dictionary<int, Dictionary<int, int>> all_CountDic = new Dictionary<int, Dictionary<int, int>>();

        int allWinCount = 0;
        int allCount = 0;

        int minLoss = -30;
        int maxLoss = -60;
        int minWin = 30;
        int maxWin = 100;
        string[] stopValue = AppSetting.Ins.GetValue("StopValue").Split('_');
        if (stopValue.Length >= 4) {
            minLoss = int.Parse(stopValue[0]);
            maxLoss = int.Parse(stopValue[1]);
            minWin = int.Parse(stopValue[2]);
            maxWin = int.Parse(stopValue[3]);
        }

        for (int loss = minLoss; loss >= maxLoss; loss -= 5)
        {
            for (int win = minWin; win <= maxWin; win += 5)
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
                }

                if (!winResultDic.ContainsKey(win)) {
                    winResultDic[win] = new List<float>();
                }
                winResultDic[win].Add(money);

                if (!lossResultDic.ContainsKey(loss))
                {
                    lossResultDic[loss] = new List<float>();
                }
                lossResultDic[loss].Add(money);


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

        int loss_final = 0, win_final = 0,orderCount_final=0;
        float maxMoney = 0;
        foreach (var item in winResultDic)
        {
            List<float> resultList = item.Value;
            float value = resultList.Sum();
            if (maxMoney < value)
            {
                maxMoney = value;
                win_final = item.Key;
            }
        }

        maxMoney = 0;
        foreach (var item in lossResultDic)
        {
            List<float> resultList = item.Value;
            float value = resultList.Sum();
            if (maxMoney < value)
            {
                maxMoney = value;
                loss_final = item.Key;
            }
        }

        orderCount_final = all_CountDic[loss_final][win_final];

        helper.SetStopPercent(loss_final, win_final);
        helper.ClearTempData();

        float allWinMoney = 0;
        foreach (var loss in all_ResultDic)
        {
            foreach (var win in loss.Value)
            {
                allWinMoney += win.Value;
                //Console.WriteLine("止损 {0} 止盈 {1} 开单次数 {2}  模拟剩余 {3}", loss.Key, win.Key, all_CountDic[loss.Key][win.Key], win.Value);
            }
        }

        try
        {
            if (!(helper is ICycleTatics))
            {
                Console.WriteLine("{0}:盈利情况：{1}/{2}   盈利平均值：{3}", helper.V_Instrument_id, allWinCount, allCount, allWinMoney / allCount);

                Debugger.Warn(string.Format("{0}:盈利情况：{1}/{2}   盈利平均值：{3}", helper.V_Instrument_id, allWinCount, allCount, allWinMoney / allCount));

                Console.WriteLine("{0}:最佳止盈止损百分比值: {1} {2} 开单次数 {3} \n模拟剩余资金: {4}", helper.V_Instrument_id, loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]);

                Debugger.Warn(string.Format("{0}:最佳止盈止损百分比值: {1} {2} 开单次数 {3} \n模拟剩余资金: {4}", helper.V_Instrument_id, loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]));
            }

            avg_win = allWinMoney / allCount;

            loss_result = loss_final;
            win_result = win_final;
            orderCount = orderCount_final;
        }
        catch (Exception ex)
        {
            avg_win = 5;

            loss_result = -80;
            win_result = 120;
            orderCount = 0;
            Debugger.Error("没有合适的止损止盈，用默认值 -80 120");
        }

        

        if (loss_final == 0 || win_final == 0) {
            return 0;
        }
        return all_ResultDic[loss_final][win_final];
    }
}
