using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 海龟交易测试
/// </summary>
public class TurtleTaticTestRunner : BaseTaticsTestRunner
{
    public static void TestRun(TurtleTaticsHelper helper)
    {
        Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

        Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

        Dictionary<int, int> winDic = new Dictionary<int, int>();

        Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();

        Dictionary<int, Dictionary<int, float>> all_CountDic = new Dictionary<int, Dictionary<int, float>>();

        for (int loss = -10; loss >= -150; loss -= 5)
        {
            for (int win = 10; win <= 150; win += 5)
            {
                TurtleTaticTestRunner run = new TurtleTaticTestRunner();
                run.Cur_Cache = new KLineCache();
                helper.SetStopPercent(loss, win);
                run.Data_All = helper.V_HistoryCache.V_KLineData;
                run.helper = helper;

                float money = run.Run();
                if (money > run.Init_Money)
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


        foreach (var loss in all_ResultDic)
        {
            foreach (var win in loss.Value)
            {
                Console.WriteLine("止损 {0} 止盈 {1} 开单次数 {2}  模拟剩余 {3}", loss.Key, win.Key, all_CountDic[loss.Key][win.Key], win.Value);
            }
        }

        Console.WriteLine("最佳止盈止损百分比值: {0} {1} 开单次数 {2} 模拟剩余资金: {3}", loss_final, win_final, all_CountDic[loss_final][win_final], all_ResultDic[loss_final][win_final]);
    }
}