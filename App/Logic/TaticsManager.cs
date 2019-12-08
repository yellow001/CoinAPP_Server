using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsManager
{
    public TaticsNetLogic V_NetLogic;

    public TaticsModel V_Model;

    public TaticsManager() {

        EMATaticsHelper m_emaHelper = new EMATaticsHelper();
        m_emaHelper.Init(AppSetting.Ins.GetValue("EMA_ETH"));

        Tactics maTactics = new Tactics("ETH-USD-SWAP", m_emaHelper);

        //TurtleTaticsHelper m_turtleHelper = new TurtleTaticsHelper();
        //m_turtleHelper.Init(AppSetting.Ins.GetValue("Turtle_EOS"));

        //Tactics maTactics = new Tactics("EOS-USD-SWAP", m_turtleHelper);
    }
}