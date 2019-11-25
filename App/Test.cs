using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CoinAPP_Server.App
{
    public class Test
    {
        WebSocketor web;

        TimeEventModel timeEvent;

        List<KLine> data=new List<KLine>();

        int count = 150;

        int curentIndex = 0;

        MATacticsTest run;

        public Test()
        {
            web = new WebSocketor();
            Start();
        }

        async void Start()
        {
            //string[] keys = AppSetting.Ins.GetValue("api").Split(';');

            //AccountAPIKey api = new AccountAPIKey(keys);
            //web.WebSocketPush += Result;

            //await web.ConnectAsync();

            //await web.LoginAsync(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase);



            //SpotApi api = new SpotApi("", "", "");
            //DateTime t_start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);

            //DateTime t_end = DateTime.Now;
            //int length = 5;
            //while (t_start.AddMinutes(length * 200) < t_end)
            //{
            //    JContainer con = await api.getCandlesAsync("BTC-USDT", t_start, t_start.AddMinutes(length * 200), length * 60);

            //    List<KLine> d = KLine.GetListFormJContainer(con);

            //    d.AddRange(data);

            //    data.Clear();

            //    data.AddRange(d);

            //    //Console.WriteLine(d.Count);

            //    t_start = t_start.AddMinutes(length * 200);
            //}

            //JContainer con = await api.getCandlesAsync("BTC-USDT", t_start, DateTime.Now, 300);

            //data = KLine.GetListFormJContainer(con);

            //Console.WriteLine(data.Count);

            //Console.WriteLine(con.First);
            //Console.WriteLine("next");
            //WriteNext(con.First);

            //Console.WriteLine("last");
            //WriteLast(con);



            //KLine k = new KLine("111", "1", "2", "3", "4","5");

            //Console.WriteLine(JToken.FromObject(k).ToString());

            //List<KLine> list = KLine.GetListFormJContainer(con);

            //foreach (var item in list)
            //{
            //    Console.WriteLine(item.closePrice);
            //}

            //KLineCache cache = new KLineCache();
            //cache.SetData(con);

            //MA ma = new MA();
            //ma.SetCache(cache);

            //Console.WriteLine(ma.GetMAValue(5) + "  " + ma.GetMAValue(10) + "  " + ma.GetMAValue(15) + "  " + ma.GetMAValue(30));

            //Console.WriteLine(con.ToString());

            //curentIndex = 0;

            KLineCache cache = new KLineCache();
            cache.RefreshData(data);

            //int winCount = 0;
            //float allMoney = 0;

            //Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

            //Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

            //Dictionary<int, int> winDic = new Dictionary<int, int>();

            //Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();


            //for (int loss = -10; loss >= -150; loss -= 5)
            //{
            //    for (int win = 10; win <= 150; win += 5)
            //    {
            //        run = new MATacticsTest(cache);

            //        run.ma_helper.SetStopPercent(loss, win);
            //        run.data_all = data;

            //        float money = run.Run();
            //        if (money > 5) {
            //            allMoney += money;
            //            winCount++;
            //            if (!lossCountDic.ContainsKey(loss)) { lossCountDic[loss] = 0; }
            //            lossCountDic[loss]++;

            //            if (!winDic.ContainsKey(win)) { winDic[win] = 0; }
            //            winDic[win]++;

            //            if (!lossWinDic.ContainsKey(loss)) {
            //                List<int> temp = new List<int>();
            //                lossWinDic[loss] = temp;
            //            }
            //            if (!lossWinDic[loss].Contains(win)) {
            //                lossWinDic[loss].Add(win);
            //            }
            //        }

            //        if (!all_ResultDic.ContainsKey(loss)) {
            //            Dictionary<int, float> temp = new Dictionary<int, float>();
            //            all_ResultDic[loss] = temp;
            //        }
            //        all_ResultDic[loss][win] = money;

            //    }
            //}

            //Dictionary<int, int> final_Dic = new Dictionary<int, int>();
            //if (lossCountDic.Count > 0) {
            //    int max_loss = lossCountDic.Values.Max();
            //    List<int> result_loss = lossCountDic.Where(q => q.Value == max_loss).Select(q => q.Key).ToList();

            //    foreach (var item in result_loss)
            //    {
            //        int max = 0;
            //        int win_temp = 0;
            //        if (lossWinDic.ContainsKey(item))
            //        {
            //            foreach (var winItem in lossWinDic[item])
            //            {
            //                if (winDic.ContainsKey(winItem))
            //                {
            //                    int count = winDic[winItem];
            //                    if (max < count)
            //                    {
            //                        max = count;
            //                        win_temp = winItem;
            //                    }
            //                }
            //            }
            //        }
            //        final_Dic[item] = win_temp;
            //    }
            //}

            //int loss_final = 0, win_final = 0;
            //float maxMoney = 0;
            //foreach (var item in final_Dic)
            //{
            //    float value = all_ResultDic[item.Key][item.Value];
            //    if (maxMoney < value) {
            //        maxMoney = value;
            //        loss_final = item.Key;
            //        win_final = item.Value;
            //    }
            //}



            //Console.WriteLine("best loss {0}  best win {1}", loss_final, win_final);

            //Console.WriteLine("winCount{0}  avg{1}",winCount,allMoney/winCount);

            run = new MATacticsTest(cache);
            //run.data_all = data;
            //run.ma_helper.SetStopPercent(-20, 10);
            //run.Start();

            //timeEvent = new TimeEventModel(0.001f, -1, Run);

            //TimeEventHandler.Ins.AddEvent(timeEvent);

            //SwapApi swapApi = new SwapApi(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase);
            //JObject con = await swapApi.getAccountsByInstrumentAsync("BTC-USD-SWAP");
            //AccountInfo info = AccountInfo.GetAccount(con["info"].ToString());
            //info.V_APIKey = api;
            //info.V_Leverage = 50;
            //await info.MakeOrder(0,info.GetAvailMoney()*0.2f);

            //SwapApi swapApi = new SwapApi(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase);
            //JObject con = await swapApi.getOrdersAsync("BTC-USD-SWAP", "6", null, null, null);
            //string s = con["order_info"].ToString();
            //Console.WriteLine(s);
            //DataTable t = JsonConvert.DeserializeObject<DataTable>(s);
            //foreach (DataRow dr in t.Rows)
            //{
            //    Console.WriteLine("{0}", dr["equity"]);
            //}

            //JContainer con =await CommonData.Ins.V_SwapApi.getInstrumentsAsync();
            //DataTable t = JsonConvert.DeserializeObject<DataTable>(con.ToString());
            //foreach (DataRow dr in t.Rows)
            //{
            //    Console.WriteLine("{0}", dr["instrument_id"]);
            //}

        }

        void WriteNext(JToken con) {
            if (con!=null) {
                Console.WriteLine(con);
                WriteNext(con.Next);
            }
        }

        void WriteLast(JToken con)
        {
            if (con != null)
            {
                Console.WriteLine(con);
                WriteNext(con.Last);
            }
        }

        async void Result(string msg)
        {
            if (msg.Contains("success")) {
                List<string> list = new List<string>();
                list.Add("swap/account:BTC-USD-SWAP");
                await web.Subscribe(list);
            }
            
            Console.WriteLine(msg);
        }

        void Run() {
            if (count + curentIndex < data.Count)
            {
                List<KLine> testData = new List<KLine>();
                testData.AddRange(data.GetRange(data.Count - 1 - count - curentIndex, count));
                run.Handle(testData);
                curentIndex++;
            }
            else {
                run.Over();

                StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);
                Console.WriteLine("over");
                TimeEventHandler.Ins.RemoveEvent(timeEvent);
            }
        }
    }
}
