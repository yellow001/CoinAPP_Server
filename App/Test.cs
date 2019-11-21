using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
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

        RunTest run;

        public Test()
        {
            web = new WebSocketor();
            Start();
        }

        async void Start()
        {

            //web.WebSocketPush += Result;

            //await web.ConnectAsync();

            //List<string> list = new List<string>();
            //list.Add("spot/candle300s:BTC-USDT");
            //await web.Subscribe(list);

            SpotApi api = new SpotApi("", "", "");
            DateTime t_start = new DateTime(DateTime.Now.Year, DateTime.Now.Month,14, 0, 0, 0);

            DateTime t_end = DateTime.Now;

            while (t_start.AddMinutes(5*200)<t_end)
            {
                JContainer con = await api.getCandlesAsync("EOS-USDT", t_start, t_start.AddMinutes(5 * 200), 300);

                List<KLine> d = KLine.GetListFormJContainer(con);

                d.AddRange(data);

                data.Clear();

                data.AddRange(d);

                //Console.WriteLine(d.Count);

                t_start = t_start.AddMinutes(5 * 200);
            }

            //JContainer con = await api.getCandlesAsync("BTC-USDT", t_start, DateTime.Now, 300);

            //data = KLine.GetListFormJContainer(con);

            Console.WriteLine(data.Count);

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

            curentIndex = 0;

            for (int loss = -20; loss > -130; loss -= 10) {
                for (int win = 20; win < 150; win+=10)
                {
                    run = new RunTest();

                    run.stopLossValue = loss;
                    run.stopWinValue = win;

                    run.data_all = data;

                    run.Start();
                }
            }

            //run = new RunTest();

            //timeEvent = new TimeEventModel(0.001f, -1, Run);

            //TimeEventHandler.Ins.AddEvent(timeEvent);
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

        void Result(string msg)
        {
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
