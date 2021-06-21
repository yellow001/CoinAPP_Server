using CoinAPP_Server.App;
using CoinAPP_Server.NetFrame.Base;
using NetFrame.Base;
using NetFrame.EnDecode;
using NetFrame.EnDecode.Extend;
using NetFrame.Interfaces;
using NetFrame.Tool;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoinAPP_Server
{
    class Program
    {
        static void Main(string[] args)
        {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            //StreamWriter sw = new StreamWriter("ConsoleOutput.txt");
            //Console.SetOut(sw);

            //BaseServer<TransModel, H5Token> server = new BaseServer<TransModel, H5Token>(12345);
            //AbsCoding.Ins = new PbCoding();
            //server.Init(IMessageHandler.Ins);
            //server.Start();

            //DbHelperMySQL.connectionString = AppSetting.Ins.Settings["connstring"];

            //CidModel m = new CidModel(1, 11, 111, -500);
            //byte[] ms = SeProtobuf.Serialization(m);

            //CidModel m2 = SeProtobuf.DeSerialization<CidModel>(ms);

            //Console.WriteLine(m2.cid);

            //Console.WriteLine(AppSetting.GetValue("test"));


            //DbHelperMySQL.connectionString = "server=120.25.84.142;database=TaskServer;uid=yellow;pwd=qW789456123=";

            //DataResult dr = DbHelperMySQL.Query("select * from user");

            //Console.WriteLine(dr.rows);

#if false
            //Test t = new Test();

            if (AppSetting.Ins.GetInt("Mode") == 1)
            {
                HttpHandler handler2 = new HttpHandler(new ASpotHandle(), AppSetting.Ins.GetValue("HttpUtlA"));
            }
            else
            {
                HttpHandler handler = new HttpHandler(new SpotHandle(), AppSetting.Ins.GetValue("HttpUtl"));
            }


            while (true)
            {
                Task.Run(delegate
                {
                    Thread.Sleep(36000000);
                }).Wait();
            }
#else

            int port = AppSetting.Ins.GetInt("Port");

            BaseServer<TransModel, BaseToken> server = new BaseServer<TransModel, BaseToken>(port);

            server.Init(NetCenter.Ins);

            server.Start();

            AbsCoding.Ins = new PbCoding();

            TaticsManager.GetIns();

            while (true)
            {
                Task.Run(delegate
                {
                    Thread.Sleep(36000000);
                }).Wait();
            }
#endif

            //sw.Flush();
            //sw.Close();
        }
    }
}
