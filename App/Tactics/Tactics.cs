using NetFrame.Tool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OKExSDK;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 基础策略类（永续合约）
/// </summary>
[Serializable]
[ProtoContract]
public class Tactics
{
    /// <summary>
    /// 关联合约
    /// </summary>
    [ProtoMember(1)]
    public string V_Instrument_id;

    /// <summary>
    /// 当前持仓信息
    /// </summary>
    [ProtoMember(2)]
    public AccountInfo V_AccountInfo;

    /// <summary>
    /// 状态
    /// </summary>
    [ProtoMember(3)]
    public int V_State;

    protected KLineCache cache;

    BaseTaticsHelper m_TaticsHelper;

    DateTime m_LastRefreshTime;


    bool error = false;

    bool debug = false;

    public Tactics() { }

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

        m_LastRefreshTime = DateTime.Now;

        V_AccountInfo = new AccountInfo();

        V_AccountInfo.V_Leverage = m_TaticsHelper.V_Leverage;

        //获取一下合约面值
        JContainer con = await CommonData.Ins.V_SwapApi.getInstrumentsAsync();
        DataTable t = JsonConvert.DeserializeObject<DataTable>(con.ToString());
        foreach (DataRow dr in t.Rows)
        {
            if (dr["instrument_id"].Equals(V_Instrument_id))
            {
                V_AccountInfo.V_Contract_val = float.Parse(dr["contract_val"].ToString());
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
        if (DateTime.Now.Minute % 30 == 0 && DateTime.Now.Second < 5)
        {
            debug = true;
        }

        try
        {
            SwapApi api = CommonData.Ins.V_SwapApi;

            //更新账号信息
            JObject obj = await api.getAccountsByInstrumentAsync(V_Instrument_id);
            V_AccountInfo.RefreshData(obj["info"].ToString());

            //更新持仓信息
            obj = await api.getPositionByInstrumentAsync(V_Instrument_id);
            V_AccountInfo.RefreshPositions(Position.GetPositionList(obj["holding"].ToString()));

            //更新未完成订单信息，全部撤销掉
            await V_AccountInfo.ClearOrders();

            if (V_AccountInfo.GetAvailMoney() < 0.0001f)
            {
                return;
            }

            if (V_AccountInfo.V_Position==null&&(DateTime.Now - m_LastRefreshTime).Ticks > (long)m_TaticsHelper.V_Min *Util.Minute_Ticks*AppSetting.Ins.GetInt("RefreshSettingTime"))//更新设置操作
            {
                //更新参数
                await m_TaticsHelper.RunHistory();

                m_LastRefreshTime = DateTime.Now;

                Console.WriteLine("{0} {1}:更新设置", DateTime.Now, V_Instrument_id);
            }
            else
            {

                if (debug)
                {
                    Console.WriteLine("{0} {1}:获取数据", DateTime.Now, V_Instrument_id);
                }

                //获取近200条K线
                JContainer con = await api.getCandlesDataAsync(V_Instrument_id, DateTime.Now.AddMinutes(-m_TaticsHelper.V_Min * 200), DateTime.Now, m_TaticsHelper.V_Min * 60);

                cache.RefreshData(con);

                V_AccountInfo.V_CurPrice = cache.V_KLineData[0].V_ClosePrice;

                await Handle();
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.ToString());
            Console.WriteLine("{0} {1}:处理数据异常",DateTime.Now,V_Instrument_id);
            error = true;
        }

        debug = false;
        Update();

        //TimeEventHandler.Ins.AddEvent(new TimeEventModel(1, 1, Update));
    }

    public async Task Handle()
    {
        if (error) {
            error = false;
            Console.WriteLine("{0}  {1}:恢复处理", DateTime.Now, V_Instrument_id);
        }

        if (debug)
        {
            Console.WriteLine("{0} {1}:当前盈利率{2}  止损{3}  止盈{4}", 
                DateTime.Now, 
                V_Instrument_id, 
                V_AccountInfo.V_Position.GetPercent(V_AccountInfo.V_CurPrice),
                m_TaticsHelper.V_LossPercent,
                m_TaticsHelper.V_WinPercent
                );
        }

        m_TaticsHelper.V_Cache = cache;

        if (V_AccountInfo.V_Positions != null && V_AccountInfo.V_Positions.Count > 1)
        {
            //持仓有两个，异常（不管了。。。）
            //await accountInfo.ClearPositions();
        }
        else
        {
            if (V_AccountInfo.V_Positions == null || V_AccountInfo.V_Positions.Count == 0)
            {
                //cd 中 ，不开单
                long leave = m_TaticsHelper.GetCoolDown();
                if (leave < 0)
                {
                    if (debug) {
                        Console.WriteLine("{0} {1}:冷却中 cd {2}",DateTime.Now,V_Instrument_id,leave);
                    }
                    return;
                }

                int o = m_TaticsHelper.MakeOrder();

                if (o > 0)
                {
                    //多单
                    await V_AccountInfo.MakeOrder(1, V_AccountInfo.GetAvailMoney() * 0.2f);
                }
                else if (o < 0)
                {
                    //空单
                    await V_AccountInfo.MakeOrder(-1, V_AccountInfo.GetAvailMoney() * 0.2f);
                }
            }
            else
            {
                //有单就算下是否需要平仓
                float v = V_AccountInfo.V_Position.GetPercent(V_AccountInfo.V_CurPrice);

                if (m_TaticsHelper.ShouldCloseOrder(V_AccountInfo.V_Position.V_Dir,v))
                {
                    await V_AccountInfo.ClearPositions();
                }
            }
        }
    }

    public AccountInfo F_GetAccountInfo() {
        return V_AccountInfo;
    }
}