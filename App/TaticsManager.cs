using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsManager
{
    public TaticsManager() {

        MATaticsHelper m_maHelper = new MATaticsHelper();
        m_maHelper.Init(AppSetting.Ins.GetValue("EOS"));

        BaseTactics maTactics = new BaseTactics("EOS-USD-SWAP", m_maHelper);

        maTactics.Start();
    }
}