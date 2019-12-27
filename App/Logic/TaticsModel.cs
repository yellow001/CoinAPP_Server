using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
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
            RunTactics(item);
        }
    }

    public void RunTactics(string coin) {
        EMATaticsHelper2 m_emaHelper = new EMATaticsHelper2();
        m_emaHelper.Init(AppSetting.Ins.GetValue(string.Format("EMA_{0}", coin)));

        Tactics maTactics = new Tactics(string.Format("{0}-USD-SWAP", coin), m_emaHelper);

        m_TacticsDic[m_emaHelper.V_Instrument_id] = maTactics;
    }

    public List<Tactics> GetTacticsInfo() {
        return m_TacticsDic.Values.ToList();
    }


    /// <summary>
    /// 客户端请求运行策略
    /// </summary>
    /// <param name="coin"></param>
    /// <returns>
    /// 1 成功
    /// 2 已在运行
    /// </returns>
    public int F_ReqRunTactics(string coin) {
        foreach (var item in m_TacticsDic.Keys)
        {
            if (item.Contains(coin)) {
                return 2;
            }
        }
        RunTactics(coin);

        return 1;
    }

    /// <summary>
    /// 请求下单  （ 1:平空   -1:平多   0:全平   2:对冲）
    /// </summary>
    /// <param name="coin"></param>
    /// <param name="state"></param>
    /// <returns>
    /// 1 成功
    /// 2 策略没运行
    /// 3 平空失败
    /// 4 平多失败
    /// 5 全平失败
    /// 6 对冲失败
    /// </returns>
    public int F_ReqOrder(string coin,int state) {
        string instrument_id = "";
        foreach (var item in m_TacticsDic.Keys)
        {
            if (item.Contains(coin))
            {
                instrument_id = item;
                break;
            }
        }

        if (string.IsNullOrEmpty(instrument_id)) {
            return 2;
        }

        Tactics tactics = m_TacticsDic[instrument_id];

        //switch (state)
        //{
        //    case 1:
        //        //平空
        //        tactics.
        //        break;
        //    case -1:
        //        //平多
        //        break;
        //    case 0:
        //        //全平
        //        break;
        //    case 2:
        //        //对冲
        //        break;
        //    default:
        //        break;
        //}

        return 0;
    }
}
