using CoinAPP_Server.App;
using NetFrame.Base;
using NetFrame.EnDecode;
using NetFrame.EnDecode.Extend;
using NetFrame.Interfaces;
using NetFrame.Tool;
using System;
using System.IO;

namespace CoinAPP_Server
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            StreamWriter sw = new StreamWriter("ConsoleOutput.txt");
            Console.SetOut(sw);

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

            Test t = new Test();

            string r= Console.ReadLine();

            while (!r.Equals("0"))
            {
                r = Console.ReadLine();
            }

            sw.Flush();
            sw.Close();
        }
    }
}
