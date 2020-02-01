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
        Debugger.Log(AppSetting.Ins.GetValue("Run"));
        for (int i = 0; i < coins.Length; i++)
        {
            string item = coins[i];
            RunTactics(item);
        }
    }

    public void RunTactics(string coin,bool isPause=true) {
        EMATaticsHelper2 m_emaHelper = new EMATaticsHelper2();
        m_emaHelper.Init(AppSetting.Ins.GetValue(string.Format("EMA_{0}", coin)));

        Tactics maTactics = new Tactics(string.Format("{0}-USD-SWAP", coin), m_emaHelper);

        if (isPause) {
            //默认开启的是暂停状态
            maTactics.V_TacticsState = EM_TacticsState.Pause;
        }

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
    /// 请求操作  （ 1:开始  2 停止  3 暂停 4 开空  5 平空 6 开多  7 平多  8 全平  9 对冲）
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
    public int F_ReqOperation(string coin,int state) {

        EM_TacticsState em_state = (EM_TacticsState)state;

        bool success = false;

        switch (em_state)
        {
            case EM_TacticsState.Start:
                success = StartTatics(coin);
                return success ? 1 : -1 ;
                break;
            case EM_TacticsState.Stop:
                success = StopTatics(coin);
                return success ? 2 : -2;
                break;
            case EM_TacticsState.Pause:
                success = PauseTatics(coin);
                return success ? 3 : -3;
                break;
            case EM_TacticsState.Short:
                break;
            case EM_TacticsState.CloseShort:
                break;
            case EM_TacticsState.Long:
                break;
            case EM_TacticsState.CloseLong:
                break;
            case EM_TacticsState.CloseAll:
                break;
            case EM_TacticsState.Hedge:
                break;
            default:
                break;
        }

        string instrument_id = "";
        if (!IsTacticsRunning(coin, out instrument_id))
        {
            return -1;
        }
        else
        {
            m_TacticsDic[instrument_id].SetTacticsState(em_state);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// 改变开单操作状态
    /// </summary>
    /// <param name="coin"></param>
    /// <param name="state"></param>
    /// <returns>
    /// </returns>
    public int F_ReqChangeOrderState(string coin, int state)
    {
        try
        {
            EM_OrderOperation em_state = (EM_OrderOperation)state;

            string instrument_id = "";

            if (!IsTacticsRunning(coin, out instrument_id))
            {
                return -1;
            }
            else
            {
                m_TacticsDic[instrument_id].SetOrderState(em_state);
                return 1;
            }
        }
        catch (Exception ex)
        {
            return 0;
        }
        
    }


    #region 操作
    /// <summary>
    /// 开始执行一个合约策略
    /// </summary>
    bool StartTatics(string coin) {
        string instrument_id = "";
        if (!IsTacticsRunning(coin, out instrument_id))
        {
            //该币种没有在运行，可以开始
            RunTactics(coin);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 停止一个合约策略
    /// </summary>
    bool StopTatics(string coin)
    {
        string instrument_id = "";
        if (!IsTacticsRunning(coin, out instrument_id))
        {
            return false;
        }

        m_TacticsDic[instrument_id].SetTacticsState(EM_TacticsState.Stop);
        m_TacticsDic.Remove(instrument_id);

        return true;
    }

    /// <summary>
    /// 暂停一个合约策略
    /// </summary>
    bool PauseTatics(string coin)
    {
        string instrument_id = "";
        if (!IsTacticsRunning(coin, out instrument_id)) {
            return false;
        }

        m_TacticsDic[instrument_id].SetTacticsState(EM_TacticsState.Pause);
        m_TacticsDic.Remove(instrument_id);

        return true;
    }

    /// <summary>
    /// 某币种策略是否在运行
    /// </summary>
    /// <returns></returns>
    bool IsTacticsRunning(string coin,out string instrument_id) {
        instrument_id = "";
        foreach (var item in m_TacticsDic.Keys)
        {
            if (item.Contains(coin))
            {
                instrument_id = item;
                break;
            }
        }

        if (string.IsNullOrEmpty(instrument_id))
        {
            //该币种没有在运行
            return false;
        }
        else {
            return true;
        }
    }
    #endregion


}
