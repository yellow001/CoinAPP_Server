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

        public List<string> typeKeys = new List<string>();

        bool running = false;

        DateTime updateTime = DateTime.Now;

        bool init = true;

        public string htmlPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "index.html";

        public ASpotHandle()
        {
            InitData();
            Handle();

            TimeEventHandler.Ins.AddEvent(new TimeEventModel(120 * 60, -1, () => {
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

        public void GetKLineValue(bool debug = true)
        {
            KLineCache tempCache = new KLineCache();

            foreach (var item in m_List.Keys)
            {
                //获取K线数据
                //获取近200条K线

                try
                {
                    ASpotData spotData = m_List[item];

                    //Debugger.Log("获取K线数据：" + spotData.SpotName);

                    string result = GetKLineData(spotData.coin, 60,400);
                    JContainer con  = JArray.Parse(result);
                    spotData.hourData.RefreshAData(con);

                    spotData.sixHourData.RefreshData(spotData.hourData.GetMergeKLine(6));

                    result = GetKLineData(spotData.coin, 4 * 60,400);
                    con = JArray.Parse(result);
                    
                    tempCache.RefreshAData(con);

                    spotData.dayData.RefreshData(tempCache.GetMergeKLine(6));


                    //计算推荐值
                    spotData.RefreshCommandValue(debug);
                }
                catch (Exception ex)
                {
                    Debugger.Error(ex.ToString());
                }
            }

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
        }

        async void Handle()
        {
            running = true;


            GetKLineValue(false);

            updateTime = DateTime.Now;
            running = false;
        }

        public string GetResult()
        {

            string resultStr = "";

            if (File.Exists(htmlPath))
            {
                using (StreamReader sr = new StreamReader(htmlPath))
                {
                    resultStr = sr.ReadToEnd();
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("总推荐（前16）<br>");

            if (m_allResult.Count > 16 && !init)
            {
                sb.AppendLine("(o゜▽゜)o☆<br>");

                for (int i = 0; i < 16; i++)
                {
                    if (i < m_allResult.Count)
                    {
                        sb.AppendLine(m_allResult[i].coin + "  " + m_allResult[i].SpotName + "  " +m_allResult[i].Type + "  " + m_allResult[i].recommandValue + "<br>");
                    }
                }

                sb.AppendLine("<br><br>(o゜▽゜)o☆  各行业推荐 前5 <br>");

                foreach (var key in typeKeys)
                {
                    List<ASpotData> data = typeDataList[key];

                    if (data != null && data.Count > 5)
                    {
                        sb.AppendLine("<br>");
                        sb.AppendLine(key + " <br>");

                        for (int j = 0; j < 5; j++)
                        {
                            if (j < data.Count)
                            {
                                sb.AppendLine(data[j].coin + "  " + data[j].SpotName + "  " + data[j].recommandValue + "<br>");
                            }
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("还没初始化完呢。。。看jb   ╮(╯_╰)╭<br><br>");
            }

            sb.AppendLine("<br>更新时间：" + updateTime.ToString());

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

                                string name = reader.GetString(1);
                                string id = reader.GetString(4);
                                string type = reader.GetString(17);

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

            init = false;
        }

        public string GetKLineData(string id, int time,int num) {
            string result = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("http://money.finance.sina.com.cn/quotes_service/api/json_v2.php/CN_MarketData.getKLineData");

            builder.AppendFormat("?symbol=sz{0}&scale={1}&ma=no&datalen={2}", id, time,num);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
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
            return result;

        }
    }
}
