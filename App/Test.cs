using NetFrame.Base;
using NetFrame.EnDecode;
using NetFrame.EnDecode.Extend;
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

        public Test()
        {
            web = new WebSocketor();
            Start();
        }

        async void Start()
        {


            List<KLine> history_data = new List<KLine>();

            OKExV5APi api = CommonData.Ins.V5pApi;


            //AccountAPIKey api = new AccountAPIKey(keys);
            //web.WebSocketPush += Result;

            //await web.ConnectAsync();

            //await web.LoginAsync(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase);



            //SpotApi api = new SpotApi("", "", "");
            DateTime t_start = DateTime.Now.AddMinutes(-100);


            DateTime t_end = DateTime.Now;

            TimeSpan timeSpan = t_end - new DateTime(1970, 1, 1);

            Console.WriteLine(timeSpan.TotalSeconds);

            timeSpan = TimeZoneInfo.ConvertTimeToUtc(t_end) - new DateTime(1970, 1, 1);

            Console.WriteLine(timeSpan.TotalSeconds);

            //return;

            int length = 1;
            while (t_start.AddMinutes(length * 100) < t_end)
            {
                JContainer con = await api.getCandlesDataAsyncV5("BTC-USDT", t_start, t_start.AddMinutes(length * 100), length);

                List<KLine> d = KLine.GetListFormJContainer(con);

                t_start = t_start.AddMinutes(length * 100);
            }

            //SwapApi api = new SwapApi("", "", "");
            //DateTime t_start = DateTime.Now.AddMinutes(-5 * 181);
            //JContainer con = await api.getCandlesDataAsync("BTC-USD-SWAP", t_start, DateTime.Now, 300);

            //data = KLine.GetListFormJContainer(con);

            //float f_10 = EMA.GetEMA(10, data);
            //float f_30 = EMA.GetEMA(30, data);
            //float f_180 = EMA.GetEMA(180, data);
            //float f_120 = EMA.GetEMA(120, data);
            //float f_60 = EMA.GetEMA(60, data);

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

            //KLineCache cache = new KLineCache();
            //cache.RefreshData(data);

            //MATaticsHelper helper = new MATaticsHelper();
            //helper.Init(AppSetting.Ins.GetValue("MA_ETH"));
            //await helper.RunHistory();

            //MATaticsHelper2 helper = new MATaticsHelper2();
            //helper.Init(AppSetting.Ins.GetValue("MA_BTC"));
            //await helper.RunHistory();

            ////TurtleTaticsHelper helper3 = new TurtleTaticsHelper();
            ////helper3.Init(AppSetting.Ins.GetValue("Turtle_ETH"));



            //EMATaticsHelper helper2 = new EMATaticsHelper();
            //helper2.Init(AppSetting.Ins.GetValue("EMA_BTC"));

            //await helper2.RunHistory();

            //EMATaticsHelper2 helper3 = new EMATaticsHelper2();
            //helper3.Init(AppSetting.Ins.GetValue("EMA_BTC"));

            //await helper3.RunHistory();

            //await helper3.RunHistory();

            //int winCount = 0;
            //float allMoney = 0;

            //Dictionary<int, int> lossCountDic = new Dictionary<int, int>();

            //Dictionary<int, List<int>> lossWinDic = new Dictionary<int, List<int>>();

            //Dictionary<int, int> winDic = new Dictionary<int, int>();

            //Dictionary<int, Dictionary<int, float>> all_ResultDic = new Dictionary<int, Dictionary<int, float>>();

            //float t = await CommonData.Ins.V_InformationApi.F_GetLongShortRatio("BTC", DateTime.Now, 5);

            //var result = await CommonData.Ins.V_SpotApi.getInstrumentsAsync();

            int runHelper = AppSetting.Ins.GetInt("RunHelper");
            string[] coins = AppSetting.Ins.GetValue("Run").Split(';');
            Console.WriteLine(AppSetting.Ins.GetValue("Run"));
            for (int i = 0; i < coins.Length; i++)
            {
                string item = coins[i];
                if (runHelper == 1)
                {
                    MATaticsHelper m_helper = new MATaticsHelper();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("MA_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 2)
                {
                    MATaticsHelper2 m_helper = new MATaticsHelper2();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("MA_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 3)
                {
                    EMATaticsHelper m_helper = new EMATaticsHelper();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("EMA_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 4)
                {
                    EMATaticsHelper2 m_helper = new EMATaticsHelper2();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("EMA2_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 5)
                {
                    EMAHelper3 m_helper = new EMAHelper3();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("EMA3_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 6)
                {
                    FourPriceHelper m_helper = new FourPriceHelper();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("FOUR_{0}", item)));

                    await m_helper.RunHistory();
                }
                else if (runHelper == 7)
                {
                    TurtleTaticsHelper m_helper = new TurtleTaticsHelper();
                    m_helper.Init(AppSetting.Ins.GetValue(string.Format("Turtle_{0}", item)));

                }
            }

            void WriteNext(JToken con)
            {
                if (con != null)
                {
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
                if (msg.Contains("success"))
                {
                    List<string> list = new List<string>();
                    list.Add("swap/account:BTC-USD-SWAP");
                    await web.Subscribe(list);
                }

                Console.WriteLine(msg);
            }
        }
    }
}
