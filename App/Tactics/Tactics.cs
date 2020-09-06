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
    /// 交易状态
    /// </summary>
    [ProtoMember(3)]
    public EM_TacticsState V_TacticsState;

    /// <summary>
    /// 下单状态
    /// </summary>
    [ProtoMember(4)]
    public EM_OrderOperation V_OrderState;

    [ProtoMember(5)]
    public KLineCache cache;

    BaseTaticsHelper m_TaticsHelper;

    DateTime m_LastRefreshTime;


    bool error = false;

    bool debug = false;

    float tempVol = 0;

    float orderPercent = 0.3236f;

    int debugCount = 0;

    public Tactics() { }

    public Tactics(string instrument_id, BaseTaticsHelper helper) {

        V_Instrument_id = instrument_id;

        m_TaticsHelper = helper;

        V_TacticsState = EM_TacticsState.Start;

        V_OrderState = EM_OrderOperation.Normal;

        orderPercent = float.Parse(AppSetting.Ins.GetValue("OrderValue"));
        Start();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public virtual async void Start() {

        try
        {
            await m_TaticsHelper.RunHistory();

            if (V_TacticsState == EM_TacticsState.Stop) { return; }

            m_LastRefreshTime = DateTime.Now;

            V_AccountInfo = new AccountInfo();

            V_AccountInfo.V_Leverage = m_TaticsHelper.V_Leverage;

            //获取一下合约面值
            JContainer con = await CommonData.Ins.V_SwapApi.getInstrumentsAsync();

            if (V_TacticsState == EM_TacticsState.Stop) { return; }

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
        }
        catch (Exception ex)
        {
            Console.WriteLine(V_Instrument_id + "  " + ex.ToString());
            Debugger.Error(V_Instrument_id + "  " + ex.ToString());

            Console.WriteLine(V_Instrument_id + "  ReStart");
            Debugger.Error(V_Instrument_id + "  ReStart");

            Start();
        }

        if (V_TacticsState == EM_TacticsState.Stop) { return; }

        cache = new KLineCache();

        if (V_TacticsState == EM_TacticsState.Start)
        {
            V_TacticsState = EM_TacticsState.Normal;
        }

        Console.WriteLine("start {0}", V_Instrument_id);
        Debugger.Warn(string.Format("start {0}", V_Instrument_id));
        Update();
    }

    //public void Start() {
    //    Console.WriteLine("start {0}",V_Instrument_id);
    //    Update();
    //}

    public virtual async void Update() {

        if (V_TacticsState == EM_TacticsState.Stop) { return; }

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

                if (!V_AccountInfo.HasEnoughMoney() && V_AccountInfo.V_Position == null)
                {
                    TimeEventHandler.Ins.AddEvent(new TimeEventModel(600, 1, Update));
                    return;
                }

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
        if (V_TacticsState == EM_TacticsState.Pause) { return; }

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

        switch (V_TacticsState)
        {
            case EM_TacticsState.Short:
                await OrderHandle();
                break;
            case EM_TacticsState.CloseShort:
                await CloseHandle();
                break;
            case EM_TacticsState.Long:
                await OrderHandle();
                break;
            case EM_TacticsState.CloseLong:
                await CloseHandle();
                break;
            case EM_TacticsState.CloseAll:
                await CloseHandle();
                break;
            case EM_TacticsState.Hedge:
                await HedgeHandle();
                break;
            case EM_TacticsState.Normal:
                await AutoHandle();
                break;
            default:
                await AutoHandle();
                break;
        }
    }

    /// <summary>
    /// 自动处理
    /// </summary>
    /// <returns></returns>
    public async Task AutoHandle() {

        bool hasLong = false, hasShort = false;
        float longPercent = 0, shortPrecent = 0;
        if (V_AccountInfo.V_Positions != null && V_AccountInfo.V_Positions.Count > 0)
        {
            foreach (var item in V_AccountInfo.V_Positions)
            {
                if (item.V_Dir > 0)
                {
                    hasLong = true;
                }
                else {
                    hasShort = true;
                }

                //有单就算下是否需要平仓
                float v = item.GetPercent(V_AccountInfo.V_CurPrice);

                if (item.V_Dir > 0)
                {
                    longPercent = v;
                }
                else {
                    shortPrecent = v;
                }

                if (m_TaticsHelper.ShouldCloseOrder(item.V_Dir, v))
                {
                    if (v > 0)
                    {
                        if (V_OrderState == EM_OrderOperation.NoClose || V_OrderState == EM_OrderOperation.LongNoClose || V_OrderState == EM_OrderOperation.ShortNoClose)
                        {
                            return;
                        }
                    }

                    if (item.V_Dir > 0)
                    {
                        hasLong = false;
                    }
                    else {
                        hasShort = false;
                    }

                    await V_AccountInfo.ClearPositions(item.V_Dir);
                }
            }
        }

        bool makeOrder = false;
        //bool Double = AppSetting.Ins.GetInt("DoubleDir") > 0;
        bool Double = false;
        if (Double)
        {
            makeOrder = !hasShort || !hasLong;
        }
        else {
            makeOrder = !hasShort && !hasLong;
        }


        if (makeOrder) {
            long leave = m_TaticsHelper.GetCoolDown();
            if (leave < 0 )
            {
                if (debug)
                {
                    Console.WriteLine("{0} {1}:冷却中 cd {2}", DateTime.Now, V_Instrument_id, leave);
                    Debugger.Log(string.Format("{0} {1}:冷却中 cd {2}", DateTime.Now, V_Instrument_id, leave));
                    debug = false;
                }
                else {
                    debugCount++;
                    if (debugCount >= 100) {
                        debugCount = 0;
                        debug = true;
                    }
                }
                return;
            }

            int o = m_TaticsHelper.MakeOrder();

            if (o > 0 && !hasLong)
            {
                if (hasShort && shortPrecent > 0) {
                    return;
                }

                //多单
                if (V_OrderState != EM_OrderOperation.ShortOnly && V_OrderState != EM_OrderOperation.ShortNoClose)
                {
                    await V_AccountInfo.MakeOrder(1, V_AccountInfo.GetAvailMoney() * orderPercent);
                }
            }
            else if (o < 0 && !hasShort)
            {
                if (hasLong && longPercent > 0) {
                    return;
                }

                //空单
                if (V_OrderState != EM_OrderOperation.LongOnly && V_OrderState != EM_OrderOperation.LongNoClose)
                {
                    await V_AccountInfo.MakeOrder(-1, V_AccountInfo.GetAvailMoney() * orderPercent);
                }
            }
        }

        #region 旧逻辑

        //if (V_AccountInfo.V_Positions != null && V_AccountInfo.V_Positions.Count > 1)
        //{
        //    //持仓有两个，异常（不管了。。。）
        //    //await accountInfo.ClearPositions();
        //}
        //else
        //{
        //    if (V_AccountInfo.V_Positions == null || V_AccountInfo.V_Positions.Count == 0)
        //    {
        //        //cd 中 ，不开单
        //        long leave = m_TaticsHelper.GetCoolDown();
        //        if (leave < 0)
        //        {
        //            if (debug)
        //            {
        //                Console.WriteLine("{0} {1}:冷却中 cd {2}", DateTime.Now, V_Instrument_id, leave);
        //            }
        //            return;
        //        }

        //        int o = m_TaticsHelper.MakeOrder();

        //        if (o > 0)
        //        {
        //            //多单
        //            if (V_OrderState != EM_OrderOperation.ShortOnly && V_OrderState != EM_OrderOperation.ShortNoClose)
        //            {
        //                await V_AccountInfo.MakeOrder(1, V_AccountInfo.GetAvailMoney() * orderPercent);
        //            }
        //        }
        //        else if (o < 0)
        //        {
        //            //空单
        //            if (V_OrderState != EM_OrderOperation.LongOnly && V_OrderState != EM_OrderOperation.LongNoClose)
        //            {
        //                await V_AccountInfo.MakeOrder(-1, V_AccountInfo.GetAvailMoney() * orderPercent);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //有单就算下是否需要平仓
        //        float v = V_AccountInfo.V_Position.GetPercent(V_AccountInfo.V_CurPrice);

        //        if (m_TaticsHelper.ShouldCloseOrder(V_AccountInfo.V_Position.V_Dir, v))
        //        {
        //            if (v > 0)
        //            {
        //                if (V_OrderState == EM_OrderOperation.NoClose || V_OrderState == EM_OrderOperation.LongNoClose || V_OrderState == EM_OrderOperation.ShortNoClose)
        //                {
        //                    return;
        //                }
        //            }
        //            await V_AccountInfo.ClearPositions();
        //        }
        //    }
        //}

        #endregion


    }

    /// <summary>
    /// 下单处理
    /// </summary>
    /// <returns></returns>
    public async Task OrderHandle() {
        if (V_TacticsState != EM_TacticsState.Short&&V_TacticsState!=EM_TacticsState.Long) {
            V_TacticsState = EM_TacticsState.Normal;
            return; 
        }

        if (V_AccountInfo.V_Positions == null || V_AccountInfo.V_Positions.Count == 0)
        {
            //什么单都没有   可以开
            tempVol = V_AccountInfo.GetAvailMoney() * 0.2f;
            await V_AccountInfo.MakeOrder(V_TacticsState==EM_TacticsState.Short?-1:1, tempVol);
            return;
        }
        else {
            if (V_AccountInfo.V_Positions.Count == 1) {
                //只有多单或者空单 检查开的量是否够(因为有可能挂单没吃完，被撤了)
                if ((V_AccountInfo.V_Position.V_Dir > 0 && V_TacticsState == EM_TacticsState.Long) || (V_AccountInfo.V_Position.V_Dir < 0 && V_TacticsState == EM_TacticsState.Short)) {
                    if ((tempVol - V_AccountInfo.V_Position.V_AllVol) >= tempVol * 0.2f) {
                        await V_AccountInfo.MakeOrder(V_TacticsState == EM_TacticsState.Short ? -1 : 1, tempVol - V_AccountInfo.V_Position.V_AllVol);
                        return;
                    }
                }
            }
        }

        tempVol = 0;
        V_TacticsState = EM_TacticsState.Normal;
    }

    /// <summary>
    /// 平仓处理
    /// </summary>
    /// <returns></returns>
    public async Task CloseHandle()
    {
        if (V_TacticsState != EM_TacticsState.CloseAll && V_TacticsState != EM_TacticsState.CloseLong && V_TacticsState != EM_TacticsState.CloseShort) {
            V_TacticsState = EM_TacticsState.Normal;
            return; 
        }

        if (V_AccountInfo.V_Positions != null || V_AccountInfo.V_Positions.Count > 0)
        {
            if (V_TacticsState == EM_TacticsState.CloseAll)
            {
                await V_AccountInfo.ClearPositions(0);
                return;
            }
            else if (V_TacticsState == EM_TacticsState.CloseLong) {
                foreach (var item in V_AccountInfo.V_Positions)
                {
                    if (item.V_Dir > 0) {
                        await V_AccountInfo.ClearPositions(1);
                        return;
                    }
                }
            }
            else
            {
                foreach (var item in V_AccountInfo.V_Positions)
                {
                    if (item.V_Dir < 0)
                    {
                        await V_AccountInfo.ClearPositions(-1);
                        return;
                    }
                }
            }
        }

        V_TacticsState = EM_TacticsState.Normal;
    }

    /// <summary>
    /// 对冲处理
    /// </summary>
    /// <returns></returns>
    public async Task HedgeHandle()
    {
        if (V_TacticsState != EM_TacticsState.Hedge)
        {
            V_TacticsState = EM_TacticsState.Normal;
            return;
        }

        if (V_AccountInfo.V_Positions != null || V_AccountInfo.V_Positions.Count > 0)
        {
            //只有一单
            if (V_AccountInfo.V_Positions.Count == 1)
            {
                await V_AccountInfo.MakeOrder(-V_AccountInfo.V_Position.V_Dir, V_AccountInfo.V_Position.V_AllVol);
                return;
            }
            else {
                //有两单，检查下量能不能对上
                float vol_1 = V_AccountInfo.V_Positions[0].V_AllVol;
                float vol_2 = V_AccountInfo.V_Positions[1].V_AllVol;
                float vol = vol_1 - vol_2;
                float maxVol = MathF.Max(vol_1, vol_2);
                if (MathF.Abs(vol) > maxVol * 0.2f) {
                    //算不上对冲，继续开单
                    await V_AccountInfo.MakeOrder(vol > 0 ? V_AccountInfo.V_Positions[1].V_Dir : V_AccountInfo.V_Positions[0].V_Dir,MathF.Abs(vol));
                    return;
                }
            }
        }

        V_TacticsState = EM_TacticsState.Pause;
    }


    public AccountInfo F_GetAccountInfo() {
        return V_AccountInfo;
    }

    public void SetTacticsState(EM_TacticsState state) {
        V_TacticsState = state;
    }

    public void SetOrderState(EM_OrderOperation state)
    {
        V_OrderState = state;
    }
}