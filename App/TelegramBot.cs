using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPP_Server.App
{
    public class TelegramBot
    {

        public string TokenStr = AppSetting.Ins.GetValue("TelegramBot");

        public string TelegramUrl = "https://api.telegram.org";

        static TelegramBot ins;

        public static TelegramBot Ins {
            get {
                if (ins==null)
                {
                    ins = new TelegramBot();
                }
                return ins;
            }
        }

        List<string> chatIDList = new List<string>();

        async Task RefreshChatList() {
            try
            {
                var url = $"{this.TelegramUrl}/{this.TokenStr}/getUpdates";
                using (var client = new HttpClient())
                {
                    var res = await client.GetAsync(url);
                    var contentStr = await res.Content.ReadAsStringAsync();
                    JObject contentJobject = JObject.Parse(contentStr);

                    JToken token = contentJobject["result"];

                    JArray array = JArray.Parse(contentJobject["result"].ToString());
                    if (array != null && array.Count > 0)
                    {
                        foreach (JToken item in array)
                        {
                            string id = item["message"]["from"]["id"].ToString();
                            if (!chatIDList.Contains(id))
                            {
                                chatIDList.Add(id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Error(ex.ToString());
            }
        }


        public async void SendMsg(string content) {
            await RefreshChatList();

            foreach (string id in chatIDList)
            {
                var url = $"{this.TelegramUrl}/{this.TokenStr}/sendmessage";
                using (var client = new HttpClient())
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add("chat_id", id);
                    queryParams.Add("text", content);
                    var encodedContent = new FormUrlEncodedContent(queryParams);
                    var paramsStr = await encodedContent.ReadAsStringAsync();
                    var res = await client.GetAsync($"{url}?{paramsStr}");
                }
            }
        }
    }
}
