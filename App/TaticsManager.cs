using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsManager
{
    public TaticsManager() {

        MATaticsHelper m_maHelper = new MATaticsHelper();
        m_maHelper.Init(AppSetting.Ins.GetValue("EOS"));

        Tactics maTactics = new Tactics("EOS-USD-SWAP", m_maHelper);
    }
}