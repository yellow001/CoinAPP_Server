using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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



            SpotApi api = new SpotApi("", "", "");
            DateTime t_start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);

            DateTime t_end = DateTime.Now;
            int length = 5;
            while (t_start.AddMinutes(length * 200) < t_end)
            {
                JContainer con = await api.getCandlesAsync("BTC-USDT", t_start, t_start.AddMinutes(length * 200), length * 60);

                List<KLine> d = KLine.GetListFormJContainer(con);

                d.AddRange(data);

                data.Clear();

                data.AddRange(d);

                //Console.WriteLine(d.Count);

                t_start = t_start.AddMinutes(length * 200);
            }

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

            int winCount = 0;
            float allMoney = 0;

            for (int loss = -10; loss >= -150; loss -= 10)
            {
                for (int win = 10; win <= 150; win += 10)
                {
                    run = new MATacticsTest(cache);

                    run.ma_helper.SetStopPercent(loss, win);
                    run.data_all = data;

                    float money = run.Run();
                    if (money > 5) {
                        allMoney += money;
                        winCount++;
                    }
                }
            }
            Console.WriteLine("winCount{0}  avg{1}",winCount,allMoney/winCount);

            //run = new MATacticsTest(cache);
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
