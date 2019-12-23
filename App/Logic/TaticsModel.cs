using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;

public class TaticsModel
{
    public Dictionary<string, Tactics> m_TacticsDic = new Dictionary<string, Tactics>();

    public TaticsModel() {

        string[] coins = AppSetting.Ins.GetValue("Run").Split(';');
        Console.WriteLine(AppSetting.Ins.GetValue("Run"));
        for (int i = 0; i < coins.Length; i++)
        {
            string item = coins[i];
            EMATaticsHelper2 m_emaHelper = new EMATaticsHelper2();
            m_emaHelper.Init(AppSetting.Ins.GetValue(string.Format("EMA_{0}", item)));

            Tactics maTactics = new Tactics(string.Format("{0}-USD-SWAP", item), m_emaHelper);

            m_TacticsDic[m_emaHelper.V_Instrument_id] = maTactics;
        }
    }
}
