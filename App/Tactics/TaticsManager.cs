using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsManager
{
    public TaticsManager() {

        MATaticsHelper m_maHelper = new MATaticsHelper();
        m_maHelper.Init(AppSetting.Ins.GetValue("MA_EOS"));

        Tactics maTactics = new Tactics("EOS-USD-SWAP", m_maHelper);

        //TurtleTaticsHelper m_turtleHelper = new TurtleTaticsHelper();
        //m_turtleHelper.Init(AppSetting.Ins.GetValue("Turtle_EOS"));

        //Tactics maTactics = new Tactics("EOS-USD-SWAP", m_turtleHelper);
    }
}