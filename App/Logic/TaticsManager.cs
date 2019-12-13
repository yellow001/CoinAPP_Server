using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsManager
{
    public TaticsNetLogic V_NetLogic;

    public TaticsModel V_Model;

    public TaticsManager() {

        string[] coins = AppSetting.Ins.GetValue("Run").Split(';');
        Console.WriteLine(AppSetting.Ins.GetValue("Run"));
        for (int i = 0; i < coins.Length; i++)
        {
            string item = coins[i];
            EMATaticsHelper2 m_emaHelper = new EMATaticsHelper2();
            m_emaHelper.Init(AppSetting.Ins.GetValue(string.Format("EMA_{0}",item)));

            Tactics maTactics = new Tactics(string.Format("{0}-USD-SWAP",item), m_emaHelper);
        }


        //EMATaticsHelper m_emaHelper = new EMATaticsHelper();
        //m_emaHelper.Init(AppSetting.Ins.GetValue("EMA_ETH"));

        //Tactics maTactics = new Tactics("ETH-USD-SWAP", m_emaHelper);

        //TurtleTaticsHelper m_turtleHelper = new TurtleTaticsHelper();
        //m_turtleHelper.Init(AppSetting.Ins.GetValue("Turtle_EOS"));

        //Tactics maTactics = new Tactics("EOS-USD-SWAP", m_turtleHelper);
    }
}