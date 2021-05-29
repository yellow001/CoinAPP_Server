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

            try
            {
                SpotHandle spotHandle = new SpotHandle();
            }
            catch (Exception ex)
            {

            }
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

        async void Result(string msg)
        {
            if (msg.Contains("success")) {
                List<string> list = new List<string>();
                list.Add("swap/account:BTC-USD-SWAP");
                await web.Subscribe(list);
            }
            
            Console.WriteLine(msg);
        }
    }
}
