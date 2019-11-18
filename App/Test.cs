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

            web.WebSocketPush += Result;

            await web.ConnectAsync();

            List<string> list = new List<string>();
            list.Add("futures/trade:BTC-USD-191227");
            await web.Subscribe(list);
        }


        void Result(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
