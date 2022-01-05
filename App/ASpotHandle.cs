using ExcelDataReader;
using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPP_Server.App
{
    public class ASpotData : SpotData {
        /// <summary>
        /// 所属类型
        /// </summary>
        public string Type;

        public ASpotData(string c, string name,string type):base(c,name)
        {
            Type = type;
        }
    }

    public class ASpotHandle: SpotHandleInterface
    {

        public Dictionary<string, List<ASpotData>> typeDataList = new Dictionary<string, List<ASpotData>>();

        public Dictionary<string, ASpotData> m_List = new Dictionary<string, ASpotData>();

        public List<ASpotData> m_allResult = new List<ASpotData>();

        public List<ASpotData> m_stResult = new List<ASpotData>();

        public List<string> typeKeys = new List<string>();

        bool running = false;

        DateTime updateTime = DateTime.Now;

        public string htmlPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "index.html";

        TimeEventModel getKLineData;

        int curIndex = 0;

        public ASpotHandle()
        {
            InitData();
            Handle();

            TimeEventHandler.Ins.AddEvent(new TimeEventModel(24*60 * 60, -1, () => {
                if (!running)
                {
                    try
                    {
                        InitData();
                        Handle();
                    }
                    catch (Exception ex)
                    {
                        Debugger.Error(ex.ToString());
                    }
                }
            }));
        }

        public void GetKLineValue(bool debug = true)
        {
            KLineCache tempCache = new KLineCache();

            List<string> keys = m_List.Keys.ToList();

            //获取K线数据
            //获取近200条K线
            try
            {
                ASpotData spotData = m_List[keys[curIndex]];

                spotData.kLineDataDic.Clear();

                //Debugger.Log("获取K线数据：" + spotData.SpotName);

                string result = GetKLineData(spotData.coin, 60, 400);
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("no Data");
                }
                JContainer con = JArray.Parse(result);
                spotData.hourData.RefreshAData(con);

                spotData.sixHourData.RefreshData(spotData.hourData.GetMergeKLine(6));

                result = GetKLineData(spotData.coin, 4 * 60, 400);
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("no Data");
                }
                con = JArray.Parse(result);

                tempCache.RefreshAData(con);
                spotData.dayData.RefreshData(tempCache.GetMergeKLine(2));

                //4h的其实是day
                //spotData.dayData.RefreshAData(con);

                spotData.kLineDataDic[60] = spotData.hourData;
                spotData.kLineDataDic[360] = spotData.sixHourData;
                spotData.kLineDataDic[1440] = spotData.dayData;

                //计算推荐值
                spotData.RefreshCommandValue(debug);
            }
            catch (Exception ex)
            {
                if (!ex.ToString().Contains("timed out"))
                {
                    curIndex++;
                    Debugger.Error(ex.ToString());
                    TimeEventHandler.Ins.RemoveEvent(getKLineData);
                    TimeEventHandler.Ins.AddEvent(
                        new TimeEventModel(300, 1, () => { TimeEventHandler.Ins.AddEvent(getKLineData); })
                    );
                }
                return;
            }
            curIndex++;
            if (curIndex >= keys.Count)
            {
                TimeEventHandler.Ins.RemoveEvent(getKLineData);
                curIndex = 0;
                SortData();
                TelegramBot.Ins.SendMsg(GetResult(false));
            }
        }

        async void Handle()
        {
            running = true;

            if (getKLineData == null)
            {
                getKLineData = new TimeEventModel(1.25f, -1, () =>
                {
#if DEBUG
                    GetKLineValue(true);
#else
                       GetKLineValue(false);
#endif
                });
            }
            TimeEventHandler.Ins.RemoveEvent(getKLineData);
            TimeEventHandler.Ins.AddEvent(getKLineData);
        }

        void SortData()
        {
            try
            {
                //排序 
                m_allResult = m_List.Values.ToList();

                if (m_allResult != null && m_allResult.Count > 0)
                {
                    m_allResult.Sort((a, b) =>
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

                m_stResult.Clear();
                foreach (var item in m_allResult)
                {
                    if (item.SpotName.Contains("ST"))
                    {
                        m_stResult.Add(item);
                    }
                }

                typeKeys = typeDataList.Keys.ToList();

                foreach (var key in typeKeys)
                {
                    List<ASpotData> data = typeDataList[key];

                    if (data != null && data.Count > 0)
                    {
                        data.Sort((a, b) =>
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
            }
            catch (Exception ex)
            {
                Debugger.Error(ex.ToString());
            }

            updateTime = DateTime.Now;
            running = false;
        }

        public string GetResult(bool useHtml=true)
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

            sb.Append("排除科创股" + lineStr);
            sb.Append("总推荐（前16）" + lineStr);

            if (m_allResult.Count > 16)
            {
                sb.Append("(o゜▽゜)o☆" + lineStr);

                for (int i = 0; i < 16; i++)
                {
                    if (i < m_allResult.Count)
                    {
                        sb.Append(m_allResult[i].coin + "  " + m_allResult[i].SpotName + "  " + m_allResult[i].Type + "  " + m_allResult[i].recommandValue + lineStr);
                    }
                }

                sb.Append(lineStr + lineStr + "(o゜▽゜)o☆  st推荐 前8 " + lineStr);
                int max = Math.Min(m_stResult.Count, 8);
                for (int i = 0; i < max; i++)
                {
                    sb.Append(m_stResult[i].coin + "  " + m_stResult[i].SpotName + "  " + m_stResult[i].Type + "  " + m_stResult[i].recommandValue + lineStr);
                }

                sb.Append(lineStr+ lineStr+"(o゜▽゜)o☆  各行业推荐 前5 " + lineStr);

                foreach (var key in typeKeys)
                {
                    List<ASpotData> data = typeDataList[key];

                    if (data != null && data.Count > 5)
                    {
                        sb.Append(lineStr);
                        sb.Append(key + lineStr);

                        for (int j = 0; j < 5; j++)
                        {
                            if (j < data.Count)
                            {
                                sb.Append(data[j].coin + "  " + data[j].SpotName + "  " + data[j].recommandValue + lineStr);
                            }
                        }
                    }
                }
            }
            else
            {
                sb.AppendFormat("还没初始化完呢。。。看jb   进度({0}/{1})╮(╯_╰)╭" + lineStr + lineStr, curIndex, m_List.Count);
            }

            sb.Append(lineStr + "更新时间：" + updateTime.ToString());

            resultStr = string.Format(resultStr, sb.ToString());

            return resultStr;
        }

        /// <summary>
        /// 刷新所有股票数据（开始执行一次算了。。。）
        /// </summary>
        public void InitData() {

            m_List.Clear();
            typeDataList.Clear();


            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/" + "A.xls";
            try
            {
                string url = "http://www.szse.cn/api/report/ShowReport?SHOWTYPE=xlsx&CATALOGID=1110&TABKEY=tab1&random=0.809744887977796";
                WebClient myWebClient = new WebClient();
                myWebClient.DownloadFile(url, filePath);
            }
            catch
            {
                Debugger.Error("download error");
            }



            //判断文件在不在，读取所有数据并分类
            if (File.Exists(filePath))
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Auto-detect format, supports:
                    //  - Binary Excel files (2.0-2003 format; *.xls)
                    //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Choose one of either 1 or 2:

                        // 1. Use the reader methods
                        do
                        {
                            reader.Read();
                            while (reader.Read())
                            {
                                // reader.GetDouble(0);

                                string name = reader.GetString(5);
                                string id = reader.GetString(4);
                                string type = reader.GetString(17);

                                if (id.StartsWith("3"))
                                {
                                    continue;
                                }

                                ASpotData data = new ASpotData(id, name, type);

                                m_List.Add(id,data);

                                if (!typeDataList.ContainsKey(type))
                                {
                                    typeDataList.Add(type, new List<ASpotData>());
                                }

                                typeDataList[type].Add(data);

                                //Console.WriteLine(id+"  "+name+"  "+type);
                            }
                        } while (reader.NextResult());
                    }
                }
            }
        }

        public string GetKLineData(string id, int time,int num) {
            string result = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData");

            builder.AppendFormat("?symbol=sz{0}&scale={1}&ma=no&datalen={2}", id, time,num);

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
                //req.Timeout = 5000;
                //添加参数
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                try
                {
                    //获取内容
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                finally
                {
                    stream.Close();
                }
                
            }
            catch (Exception ex)
            {
                Debugger.Error(ex.ToString());
            }
            return result;
        }
    }
}
