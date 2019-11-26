using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 基础策略类（永续合约）
/// </summary>
public class Tactics
{
    /// <summary>
    /// 关联合约
    /// </summary>
    protected string V_Instrument_id;

    /// <summary>
    /// 当前持仓信息
    /// </summary>
    protected AccountInfo accountInfo;

    protected KLineCache cache;

    BaseTaticsHelper m_TaticsHelper;

    DateTime m_LastRefreshTime;

    public Tactics(string instrument_id, BaseTaticsHelper helper) {
        Start(instrument_id, helper);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public virtual async void Start(string instrument_id, BaseTaticsHelper helper) {
        V_Instrument_id = instrument_id;

        m_TaticsHelper = helper;

        await m_TaticsHelper.RunHistory();

        m_LastRefreshTime = DateTime.UtcNow;

        accountInfo = new AccountInfo();

        accountInfo.V_Leverage = m_TaticsHelper.V_Leverage;

        //获取一下合约面值
        JContainer con = await CommonData.Ins.V_SwapApi.getInstrumentsAsync();
        DataTable t = JsonConvert.DeserializeObject<DataTable>(con.ToString());
        foreach (DataRow dr in t.Rows)
        {
            if (dr["instrument_id"].Equals(V_Instrument_id))
            {
                accountInfo.V_Contract_val = float.Parse(dr["contract_val"].ToString());
                break;
            }
        }

        //设置合约倍数
        await CommonData.Ins.V_SwapApi.setLeverageByInstrumentAsync(V_Instrument_id, (int)m_TaticsHelper.V_Leverage, "3");

        cache = new KLineCache();

        Console.WriteLine("start {0}", V_Instrument_id);
        Update();
    }

    //public void Start() {
    //    Console.WriteLine("start {0}",V_Instrument_id);
    //    Update();
    //}

    public virtual async void Update() {

        if ((DateTime.UtcNow - m_LastRefreshTime).Ticks > 0)
        {
            //更新参数
            await m_TaticsHelper.RunHistory();

            m_LastRefreshTime = DateTime.UtcNow;
        }
        else {
            SwapApi api = CommonData.Ins.V_SwapApi;

            //更新账号信息
            JObject obj = await api.getAccountsByInstrumentAsync(V_Instrument_id);
            accountInfo.RefreshData(obj["info"].ToString());

            //更新持仓信息
            obj = await api.getPositionByInstrumentAsync(V_Instrument_id);
            accountInfo.RefreshPositions(Position.GetPositionList(obj["holding"].ToString()));

            //更新未完成订单信息，全部撤销掉
            await accountInfo.ClearOrders();

            //获取近200条K线
            JContainer con = await api.getCandlesDataAsync(V_Instrument_id, DateTime.Now.AddMinutes(-5 * 200), DateTime.Now, 300);

            cache.RefreshData(con);

            accountInfo.V_CurPrice = cache.V_KLineData[0].V_ClosePrice;

            await Handle();
        }

        TimeEventHandler.Ins.AddEvent(new TimeEventModel(2, 1, Update));
    }

    public async Task Handle()
    {
        m_TaticsHelper.V_Cache = cache;

        if (accountInfo.V_Positions != null && accountInfo.V_Positions.Count > 1)
        {
            //持仓有两个，异常，平仓
            await accountInfo.ClearPositions();
        }
        else
        {
            if (accountInfo.V_Positions == null || accountInfo.V_Positions.Count == 0)
            {
                //cd 中 ，不开单
                long leave = m_TaticsHelper.GetCoolDown();
                if (leave < 0)
                {
                    //Console.WriteLine("冷却中 cd " + leave);
                    return;
                }

                int o = m_TaticsHelper.MakeOrder();

                if (o > 0)
                {
                    //多单
                    await accountInfo.MakeOrder(1, accountInfo.GetAvailMoney() * 0.2f);
                }
                else if (o < 0)
                {
                    //空单
                    await accountInfo.MakeOrder(-1, accountInfo.GetAvailMoney() * 0.2f);
                }
            }
            else
            {
                //有单就算下是否需要平仓
                float v = accountInfo.V_Position.GetPercent(accountInfo.V_CurPrice);

                if (m_TaticsHelper.ShouldCloseOrder(accountInfo.V_Position.V_Dir,v))
                {
                    await accountInfo.ClearPositions();
                }
            }
        }
    }
}