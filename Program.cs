using CoinAPP_Server.App;
using NetFrame.Base;
using NetFrame.EnDecode;
using NetFrame.EnDecode.Extend;
using NetFrame.Interfaces;
using NetFrame.Tool;
using System;

namespace CoinAPP_Server
{
    class Program
    {
        static void Main(string[] args)
        {
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

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
