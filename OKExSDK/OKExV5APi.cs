using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OKExSDK
{
    public class OKExV5APi : SdkApi
    {
        public OKExV5APi(AccountAPIKey api) : base(api.V_ApiKey, api.V_SecretKey, api.V_Passphrase) { }

        public OKExV5APi(string apiKey, string secret, string passPhrase) : base(apiKey, secret, passPhrase) { }

        #region 合约相关
        private string SWAP_SEGMENT = "api/swap/v3";

        /// <summary>
        /// 获取合约信息
        /// </summary>
        /// <returns></returns>
        public async Task<JContainer> getInstrumentsAsync(string instrument_id)
        {
            var url = $"{this.BASEURL}api/v5/account/leverage-info";
            using (var client = new HttpClient(new HttpInterceptor(this._apiKey, this._secret, this._passPhrase, null)))
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add("instId", instrument_id);
                queryParams.Add("mgnMode", "cross");

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
        #endregion

        #region 公共api

        /// <summary>
        /// V5那个接口太恶心了，用回v3的
        /// </summary>
        private string SPOT_SEGMENT = "api/spot/v3";

        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <param name="instrument_id">合约名称，如BTC-USD-SWAP</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="granularity">时间粒度，以秒为单位，必须为60的倍数</param>
        /// <returns></returns>
        public async Task<JContainer> getCandlesDataAsync(string instrument_id, DateTime? start, DateTime? end, int? granularity)
        {
            var url = $"{this.BASEURL}{this.SPOT_SEGMENT}/instruments/{instrument_id}/candles";
            using (var client = new HttpClient(new HttpInterceptor(this._apiKey, this._secret, this._passPhrase, null)))
            {
                var queryParams = new Dictionary<string, string>();
                if (start.HasValue)
                {
                    queryParams.Add("start", TimeZoneInfo.ConvertTimeToUtc(start.Value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                }
                if (end.HasValue)
                {
                    queryParams.Add("end", TimeZoneInfo.ConvertTimeToUtc(end.Value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                }
                if (granularity.HasValue)
                {
                    queryParams.Add("granularity", granularity.Value.ToString());
                }
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

        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <param name="instrument_id">合约名称，如BTC-USD-SWAP</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="granularity">时间粒度，以秒为单位，必须为60的倍数</param>
        /// <returns></returns>
        public async Task<JContainer> getCandlesDataAsyncV5(string instrument_id, DateTime? start, DateTime? end, int? granularity)
        {
            var url = $"{this.BASEURL}api/v5/market/candles";
            using (var client = new HttpClient(new HttpInterceptor(this._apiKey, this._secret, this._passPhrase, null)))
            {
                DateTime dameTime1970 = new DateTime(1970, 1, 1);
                var queryParams = new Dictionary<string, string>();
                queryParams.Add("instId", instrument_id);
                if (start.HasValue)
                {
                    TimeSpan t = TimeZoneInfo.ConvertTimeToUtc(end.Value) - dameTime1970;
                    queryParams.Add("after", ((long)t.TotalMinutes * 60 * 1000).ToString());
                }
                if (end.HasValue)
                {
                    TimeSpan t = TimeZoneInfo.ConvertTimeToUtc(start.Value) - dameTime1970;
                    queryParams.Add("before", ((long)t.TotalMinutes * 60 * 1000).ToString());
                }
                if (granularity.HasValue)
                {
                    queryParams.Add("bar", granularity.Value.ToString()+"m");
                }
                var encodedContent = new FormUrlEncodedContent(queryParams);
                var paramsStr = await encodedContent.ReadAsStringAsync();
                var res = await client.GetAsync($"{url}?{paramsStr}");
                var contentStr = await res.Content.ReadAsStringAsync();
                try
                {
                    JObject obj = JObject.Parse(contentStr);
                    string resultStr = obj["data"].ToString();

                    if (resultStr[0] == '[')
                    {
                        return JArray.Parse(resultStr);
                    }
                    return JObject.Parse(resultStr);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


        #endregion
    }
}
