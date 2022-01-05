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
using System.Threading;
using System.Threading.Tasks;

namespace CoinAPP_Server.App
{
    public class SpotData
    {

        public string coin;

        public string SpotName;

        public KLineCache hourData = new KLineCache();

        public KLineCache sixHourData = new KLineCache();

        public KLineCache dayData = new KLineCache();

        public float V_CurPrice = 0;

        public float V_OpenPrice = 0;

        public float V_AllPrice = 0;

        public int recommandValue;

        public int shortRecommandValue;

        public Dictionary<int, KLineCache> kLineDataDic = new Dictionary<int, KLineCache>();

        List<int> V_CycleList = new List<int> { 7, 14, 120 };

        public SpotData() { }

        public SpotData(string c, string name)
        {
            coin = c;
            SpotName = name;
        }


        public void RefreshCommandValue(bool debug = true)
        {
            V_CurPrice = dayData.V_KLineData[0].V_ClosePrice;
            V_OpenPrice = dayData.V_KLineData[0].V_OpenPrice;

            V_AllPrice = GetAllPrice();

            recommandValue = GetCommandValue();
            shortRecommandValue = GetShortCommandValue();

            if (debug)
            {
                Debugger.Log(coin + "  " + SpotName + "  推荐值：" + recommandValue);
            }
        }

        public float GetAllPrice()
        {

            int length = Math.Min(5, dayData.V_KLineData.Count);

            float p = MA.GetMA(length, dayData.V_KLineData);

            List<float> vol = dayData.V_KLineData.Select((data) => data.V_Vol).ToList();

            float v = MA.GetMA(length, vol);

            return p * v;
        }


        public int GetCommandValue(bool debug = true)
        {
            List<int> tempList = new List<int>();

            float doLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.DoLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float doShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.DoShort, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.CloseLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Spot, MatchItemActionType.CLoseShort, kLineDataDic, 1, V_CycleList, ref tempList);


            float result = doLongValue + closeShortValue - doShortValue - closeLongValue;


            return (int)result;
        }

        public int GetShortCommandValue(bool debug = true)
        {
            List<int> tempList = new List<int>();

            float doLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float doShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Swap, MatchItemActionType.DoShort, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeLongValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CloseLong, kLineDataDic, 1, V_CycleList, ref tempList);

            float closeShortValue = MatchItemHandler.Ins.GetMatchValue(MatchItemType.Swap, MatchItemActionType.CLoseShort, kLineDataDic, 1, V_CycleList, ref tempList);


            float result = doLongValue + closeShortValue - doShortValue - closeLongValue;


            return (int)result;
        }
    }

    public class SpotHandle : SpotHandleInterface
    {
        public Dictionary<string, SpotData> m_USDTList = new Dictionary<string, SpotData>();

        public Dictionary<string, SpotData> m_ResultDic = new Dictionary<string, SpotData>();

        public static float eth_usdt = 0;

        public List<string> ignoreList = new List<string>();

        bool running = false;

        DateTime updateTime = DateTime.Now;

        float longShortRatio = 0;

        bool init = true;

        public List<int> MinList = new List<int>() { 60, 2 * 60, 4 * 60, 6 * 60, 12 * 60, 24 * 60, 7 * 24 * 60 };

        public string htmlPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "index.html";

        MatchItemHandler matchItemHandler = MatchItemHandler.Ins;

        int index = 0;

        public SpotHandle()
        {
            string str = AppSetting.Ins.GetValue("Ignore");

            ignoreList.AddRange(str.Split(';'));

            Handle();

            TimeEventHandler.Ins.AddEvent(new TimeEventModel(8 * 60 * 60, -1, () =>
              {
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

        async Task InitData()
        {
            init = true;

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
                        m_USDTList.Add(coin, new SpotData(coin, key));
                    }
                }

                if (!m_ResultDic.ContainsKey(coin))
                {
                    m_ResultDic.Add(coin, new SpotData(coin, ""));
                }
            }

            init = false;

        }

        async Task GetKLineValue(bool debug = true)
        {
            index = 0;
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

                        await Task.Run(delegate
                        {
                            Thread.Sleep(500);
                        });

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
                index++;
            }


            foreach (var key in m_ResultDic.Keys)
            {
                if (m_USDTList.ContainsKey(key))
                {
                    m_ResultDic[key].V_CurPrice = m_USDTList[key].V_CurPrice;
                    m_ResultDic[key].V_AllPrice = m_USDTList[key].V_AllPrice;
                    m_ResultDic[key].recommandValue = m_USDTList[key].recommandValue;
                    m_ResultDic[key].shortRecommandValue = m_USDTList[key].shortRecommandValue;
                }
            }
        }

        async void Handle()
        {
            running = true;

            await InitData();
#if DEBUG
            await GetKLineValue(true);
#else
              await GetKLineValue(false);
#endif
            longShortRatio = await OnGetLongShortRatio(DateTime.UtcNow);

            updateTime = DateTime.Now;

            TelegramBot.Ins.SendMsg(GetResult(false));

            running = false;
        }

        public string GetResult(bool useHtml = true)
        {
            string lineStr = "<br>";
            if (!useHtml)
            {
                lineStr = "\n";
            }
            string resultStr = "{0}";

            if (useHtml && File.Exists(htmlPath))
            {
                using (StreamReader sr = new StreamReader(htmlPath))
                {
                    resultStr = sr.ReadToEnd();
                }
            }

            StringBuilder sb = new StringBuilder();

            if (m_ResultDic.Count > 20 && !init)
            {
                sb.Append("推荐币种（前20）" + lineStr);

                sb.Append("(o゜▽゜)o☆" + lineStr + lineStr);
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

                for (int i = 0; i < 20; i++)
                {
                    if (i < result.Count)
                    {
                        sb.Append(result[i].coin + "  " + result[i].recommandValue + lineStr);
                    }
                }

                sb.Append(lineStr + lineStr);
                //sb.Append("短线多（前4）" + lineStr);
                //result.Sort((a, b) =>
                //{
                //    if (a.shortRecommandValue != b.shortRecommandValue)
                //    {
                //        return b.shortRecommandValue - a.shortRecommandValue > 0 ? 1 : -1;
                //    }

                //    if (b.V_AllPrice != a.V_AllPrice)
                //    {
                //        return b.V_AllPrice - a.V_AllPrice > 0 ? 1 : -1;
                //    }

                //    if (b.V_CurPrice != a.V_CurPrice)
                //    {
                //        return b.V_CurPrice - a.V_CurPrice > 0 ? 1 : -1;
                //    }

                //    return 1;
                //});

                //for (int i = 0; i < 4; i++)
                //{
                //    if (i < result.Count)
                //    {
                //        sb.Append(result[i].coin + "  " + result[i].shortRecommandValue + lineStr);
                //    }
                //}

                //sb.Append(lineStr + lineStr);
                //sb.Append("短线空（前4）" + lineStr);

                //result.Sort((a, b) =>
                //{
                //    if (a.shortRecommandValue != b.shortRecommandValue)
                //    {
                //        return a.shortRecommandValue - b.shortRecommandValue > 0 ? 1 : -1;
                //    }

                //    if (b.V_AllPrice != a.V_AllPrice)
                //    {
                //        return b.V_AllPrice - a.V_AllPrice > 0 ? 1 : -1;
                //    }

                //    if (b.V_CurPrice != a.V_CurPrice)
                //    {
                //        return b.V_CurPrice - a.V_CurPrice > 0 ? 1 : -1;
                //    }

                //    return 1;
                //});

                //for (int i = 0; i < 4; i++)
                //{
                //    if (i < result.Count)
                //    {
                //        sb.Append(result[i].coin + "  " + result[i].shortRecommandValue + lineStr);
                //    }
                //}

                sb.AppendFormat("当前更新 进度({0}/{1})" + lineStr + lineStr, index, m_USDTList.Count);
            }
            else
            {
                sb.AppendFormat("还没初始化完呢。。。看jb   进度({0}/{1})╮(╯_╰)╭" + lineStr + lineStr, index, m_USDTList.Count);
            }

            sb.Append(lineStr + "大饼多空比：" + longShortRatio + lineStr);

            sb.Append(lineStr + "更新时间：" + updateTime.ToString());

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
