using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK.Models.Spot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OKExSDK
{
    public class InformationApi : SdkApi
    {
        private string INFORMATION_SEGMENT = "api/information/v3";

        /// <summary>
        /// InformationApi构造函数
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <param name="secret">Secret</param>
        /// <param name="passPhrase">Passphrase</param>
        public InformationApi(AccountAPIKey api) : base(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase) { }

        public InformationApi(string apiKey, string secret, string passPhrase) : base(apiKey, secret, passPhrase)
        {
        }

        /// <summary>
        /// 获取多空比数据
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="granularity"></param>
        /// <returns></returns>
        public async Task<JContainer> getLongShortRatioAsync(string coin, DateTime start, DateTime end, int granularity)
        {
            var url = $"{this.BASEURL}{this.INFORMATION_SEGMENT}/{coin}/long_short_ratio";

            using (var client = new HttpClient(new HttpInterceptor(this._apiKey, this._secret, this._passPhrase, null)))
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add("start", TimeZoneInfo.ConvertTimeToUtc(start).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                queryParams.Add("end", TimeZoneInfo.ConvertTimeToUtc(end).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                queryParams.Add("granularity", granularity.ToString());

                var encodedContent = new FormUrlEncodedContent(queryParams);
                var paramsStr = await encodedContent.ReadAsStringAsync();
                var res = await client.GetAsync($"{url}?{paramsStr}");
                var contentStr = await res.Content.ReadAsStringAsync();
                if (contentStr[0] == '[')
                {
                    return JArray.Parse(contentStr);
                }
                return JObject.Parse(contentStr);
            }
        }


        public async Task<float> F_GetLongShortRatio(string coin,DateTime time,int min)
        {
            InformationApi api = CommonData.Ins.V_InformationApi;
            JContainer con = await api.getLongShortRatioAsync(coin.ToLower(), time.AddMinutes(-min), time, min * 60);
            JToken temp = con.First;

            try
            {
                while (temp != null)
                {
                    List<string> content = temp.ToObject<List<string>>();
                    if (content.Count > 1)
                    {
                        return float.Parse(content[1]);
                    }
                    temp = temp.Next;
                }
            }
            catch (Exception ex)
            {

            }

            
            return 1;
        }
    }
}
