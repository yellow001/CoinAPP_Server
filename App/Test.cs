using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoinAPP_Server.App
{
    public class Test
    {
        WebSocketor web;

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
            DateTime t_start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            JContainer con = await api.getCandlesAsync("BTC-USDT", t_start, DateTime.Now, 300);

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

            KLineCache cache = new KLineCache();
            cache.SetData(con);

            MA ma = new MA();
            ma.SetCache(cache);

            Console.WriteLine(ma.GetMAValue(5) + "  " + ma.GetMAValue(10) + "  " + ma.GetMAValue(15) + "  " + ma.GetMAValue(30));

            //Console.WriteLine(con.ToString());
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
    }
}
