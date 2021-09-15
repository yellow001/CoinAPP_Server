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
using System.Threading.Tasks;

namespace CoinAPP_Server.App
{
    public class SpotData {

        public string coin;

        public string SpotName;

        public KLineCache hourData = new KLineCache();

        public KLineCache sixHourData = new KLineCache();

        public KLineCache dayData = new KLineCache();

        public float V_CurPrice = 0;

        public float V_OpenPrice = 0;

        public float V_AllPrice = 0;

        public int recommandValue;

        public Dictionary<int, KLineCache> kLineDataDic = new Dictionary<int, KLineCache>();

        List<int> V_CycleList = new List<int> { 7, 14, 120 };

        public SpotData() { }

        public SpotData(string c,string name) {
            coin = c;
            SpotName = name;
        }


        public void RefreshCommandValue(bool debug=true) {
            V_CurPrice = dayData.V_KLineData[0].V_ClosePrice;
            V_OpenPrice = dayData.V_KLineData[0].V_OpenPrice;

            V_AllPrice = GetAllPrice();

            recommandValue = GetCommandValue();

            if (debug)
            {
                Debugger.Log(coin + "  "+SpotName + "  推荐值：" + recommandValue);
            }
        }

        public float GetAllPrice() {

            int length = Math.Min(5, dayData.V_KLineData.Count);

            float p = MA.GetMA(length, dayData.V_KLineData);

            List<float> vol = dayData.V_KLineData.Select((data) => data.V_Vol).ToList();

            float v = MA.GetMA(length, vol);

            return p * v;
        }


        public int GetCommandValue(bool debug=true) {
            List<int> tempList = new List<int>();

            float doLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot,MatchItemActionType.DoLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float doShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.DoShort, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.CloseLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.CLoseShort, kLineDataDic, 1, V_CycleList, ref tempList);


            float result = doLongValue + closeShortValue - doShortValue - closeLongValue;

            //if (result<=0)
            //{
            //    return 0;
            //}

            //float hourMA25 = MA.GetMA(25, hourData.V_KLineData);

            //float hourKMA25 = hourMA25 - MA.GetMA(25, hourData.V_KLineData.GetRange(6, 50));


            //float result_hour = 0;
            //if (V_CurPrice >= hourMA25)
            //{
            //    result_hour += 1;
            //}
            //if (hourKMA25 > 0)
            //{
            //    result_hour += 2;
            //}


            //float sixHourMA10 = MA.GetMA(10, sixHourData.V_KLineData);

            //float sixHourMA30 = MA.GetMA(30, sixHourData.V_KLineData);

            //float sixHourKMA30 = sixHourMA30 - MA.GetMA(30, sixHourData.V_KLineData.GetRange(6, 50));


            //float result_sixhour = 0;
            //if (V_CurPrice >= sixHourMA10)
            //{
            //    result_sixhour += 1;
            //}
            //if (V_CurPrice >= sixHourMA30)
            //{
            //    result_sixhour += 1;
            //}
            //if (sixHourKMA30 > 0)
            //{
            //    result_sixhour += 2;
            //}

            //float per = Math.Abs((V_CurPrice - sixHourMA30) / sixHourMA30) * 100;

            //if (V_CurPrice >= sixHourMA30 && per < 5 && per > 0)
            //{
            //    result_sixhour += 8 - per;
            //}

            //float dayMA7 = MA.GetMA(7, dayData.V_KLineData);
            //float dayMA25 = MA.GetMA(25, dayData.V_KLineData);


            //float result_day = 0;
            //if (V_CurPrice >= dayMA7)
            //{
            //    result_day += 1;
            //}

            //per = Math.Abs((V_CurPrice - dayMA25) / dayMA25) * 100;
            //if (V_CurPrice >= dayMA25 && per < 5 && per > 0)
            //{
            //    result_day += 10 - per;
            //}
            //else if (V_CurPrice >= dayMA25 && per < 10 && per > 0)
            //{
            //    result_day += 5 - per;
            //}

            //result += (result_hour + result_sixhour + result_day * 2) / 4;

            return (int)result;
        }
    }

    public class SpotHandle: SpotHandleInterface
    {
        public Dictionary<string, SpotData> m_USDTList = new Dictionary<string, SpotData>();

        public Dictionary<string, SpotData> m_ResultDic = new Dictionary<string, SpotData>();

        public static float eth_usdt = 0;

        public List<string> ignoreList = new List<string>();

        bool running = false;

        DateTime updateTime = DateTime.Now;

        float longShortRatio = 0;

        bool init = true;

        public List<int> MinList = new List<int>() { 60, 2*60, 4*60, 6*60, 12*60, 24*60};

        public string htmlPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "index.html";

        MatchItemHandler matchItemHandler = MatchItemHandler.Ins;

        public SpotHandle()
        {
            string str = AppSetting.Ins.GetValue("Ignore");

            ignoreList.AddRange(str.Split(';'));

            InitData();
            Handle();

            TimeEventHandler.Ins.AddEvent(new TimeEventModel(60 * 60, -1, () => {
                if (!running)
                {
                    try
                    {
                        Handle();
                    }
                    catch (Exception ex)
                    {
                        Debugger.Error(ex.ToString());
                    }
                }
            }));
        }

        async void InitData()
        {
            longShortRatio = await OnGetLongShortRatio(DateTime.UtcNow);


            var result = await CommonData.Ins.V_SpotApi.getInstrumentsAsync();

            JToken temp = result.First;

            DataTable t = JsonConvert.DeserializeObject<DataTable>(result.ToString());
            foreach (DataRow dr in t.Rows)
            {
                string key = dr["instrument_id"].ToString();

                string coin = dr["base_currency"].ToString();

                if (ignoreList.Contains(coin))
                {
                    continue;
                }

                if (key.Contains("-USDT"))
                {
                    if (!m_USDTList.ContainsKey(coin))
                    {
                        m_USDTList.Add(coin, new SpotData(coin,key));
                    }
                }

                if (!m_ResultDic.ContainsKey(coin))
                {
                    m_ResultDic.Add(coin, new SpotData(coin,""));
                }
            }

            await GetKLineValue(true);

            init = false;

        }

        async Task GetKLineValue(bool debug=true)
        {

            foreach (var item in m_USDTList.Keys)
            {
                //获取K线数据
                //获取近200条K线

                try
                {
                    SpotData spotData = m_USDTList[item];

                    //Debugger.Log("获取K线数据：" + spotData.SpotName);

                    JContainer con = await CommonData.Ins.V_SpotApi.getCandlesAsync(spotData.SpotName, DateTime.Now.AddMinutes(-24 * 60 * 200), DateTime.Now, 24 * 60 * 60);
                    spotData.dayData.RefreshData(con);

                    for (int i = 0; i < MinList.Count; i++)
                    {
                        int value = MinList[i];
                        con = await CommonData.Ins.V_SpotApi.getCandlesAsync(spotData.SpotName, DateTime.Now.AddMinutes(-value * 200), DateTime.Now, 60 * value);

                        if (spotData.kLineDataDic.ContainsKey(value))
                        {
                            spotData.kLineDataDic[value].RefreshData(con);
                        }
                        else
                        {
                            KLineCache kLineCache = new KLineCache();
                            kLineCache.RefreshData(con);
                            spotData.kLineDataDic.Add(value, kLineCache);
                        }
                    }

                    spotData.hourData = spotData.kLineDataDic[60];
                    spotData.sixHourData = spotData.kLineDataDic[360];

                    //计算推荐值
                    spotData.RefreshCommandValue(debug);
                }
                catch (Exception ex)
                {
                    Debugger.Error(ex.ToString());
                }
            }
            

            foreach (var key in m_ResultDic.Keys)
            {
                if (m_USDTList.ContainsKey(key))
                {
                    m_ResultDic[key].V_CurPrice = m_USDTList[key].V_CurPrice;
                    m_ResultDic[key].V_AllPrice = m_USDTList[key].V_AllPrice;
                    m_ResultDic[key].recommandValue = m_USDTList[key].recommandValue;
                }
            }

            Debugger.Log("");

            try
            {
                //排序 
                List<SpotData> result = m_ResultDic.Values.ToList();

                if (result != null && result.Count > 0)
                {
                    result.Sort((a, b) =>
                    {
                        if (a.recommandValue != b.recommandValue)
                        {
                            return b.recommandValue - a.recommandValue > 0 ? 1 : -1;
                        }

                        if (b.V_AllPrice != a.V_AllPrice)
                        {
                            return b.V_AllPrice - a.V_AllPrice > 0 ? 1 : -1;
                        }

                        if (b.V_CurPrice != a.V_CurPrice)
                        {
                            return b.V_CurPrice - a.V_CurPrice > 0 ? 1 : -1;
                        }

                        return 1;
                    });
                }
            }
            catch (Exception ex)
            {
                Debugger.Error(ex.ToString());
            }
        }

        async void Handle() {
            running = true;


            await GetKLineValue(false);
            longShortRatio = await OnGetLongShortRatio(DateTime.UtcNow);
            
            updateTime = DateTime.Now;
            running = false;
        }

        public string GetResult() {

            string resultStr = "";

            if (File.Exists(htmlPath))
            {
                using (StreamReader sr = new StreamReader(htmlPath))
                {
                    resultStr = sr.ReadToEnd();
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("推荐币种（前16）<br>");

            if (m_ResultDic.Count > 16 && !init)
            {
                sb.AppendLine("(o゜▽゜)o☆<br><br>");
                List<SpotData> result = m_ResultDic.Values.ToList();
                result.Sort((a, b) =>
                {
                    if (a.recommandValue != b.recommandValue)
                    {
                        return b.recommandValue - a.recommandValue > 0 ? 1 : -1;
                    }

                    if (b.V_AllPrice != a.V_AllPrice)
                    {
                        return b.V_AllPrice - a.V_AllPrice > 0 ? 1 : -1;
                    }

                    if (b.V_CurPrice != a.V_CurPrice)
                    {
                        return b.V_CurPrice - a.V_CurPrice > 0 ? 1 : -1;
                    }

                    return 1;
                });

                for (int i = 0; i < 16; i++)
                {
                    if (i < result.Count)
                    {
                        sb.AppendLine(result[i].coin + "  " + result[i].recommandValue+ "<br>");
                    }
                }
            }
            else {
                sb.AppendLine("还没初始化完呢。。。看jb   ╮(╯_╰)╭<br><br>");
            }

            sb.AppendLine("<br>大饼多空比：" + longShortRatio+ "<br>");

            sb.AppendLine("<br>更新时间：" + updateTime.ToString());

            resultStr = string.Format(resultStr, sb.ToString());

            return resultStr;
        }


        /// <summary>
        /// 获取多空比(只有btc的有参考性)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task<float> OnGetLongShortRatio(DateTime time)
        {
            return await CommonData.Ins.V_InformationApi.F_GetLongShortRatio("btc", time, 50);
        }
    }
}
