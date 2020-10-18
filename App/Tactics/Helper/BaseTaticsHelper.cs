using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

public class BaseTaticsHelper
{

    /// <summary>
    /// 关联合约
    /// </summary>
    public string V_Instrument_id;

    public string CoinType;

    /// <summary>
    /// 倍数
    /// </summary>
    public float V_Leverage = 50;

    /// <summary>
    /// K线时长(分)
    /// </summary>
    public int V_Min = 5;

    /// <summary>
    /// 当前K线数据
    /// </summary>
    public KLineCache V_Cache;

    /// <summary>
    /// 2000条历史K线数据
    /// </summary>
    public KLineCache V_HistoryCache;

    /// <summary>
    /// 上次操作时间
    /// </summary>
    public DateTime V_LastOpTime;

    /// <summary>
    /// 历史建议止损百分比值
    /// </summary>
    protected float lossPercent = 0;

    public float V_LossPercent {
        get {
            return lossPercent;
        }
    }
    /// <summary>
    /// 历史建议止盈百分比值
    /// </summary>
    protected float winPercent = 0;

    public float V_WinPercent {
        get {
            return winPercent;
        }
    }

    /// <summary>
    /// 冷却
    /// </summary>
    protected long cooldown=10;

    /// <summary>
    /// 止损后冷却（止损次数越多，冷却越长，止盈后重置）
    /// </summary>
    protected float lossCooldown = 0;

    public bool V_WinClose = false;

    public bool V_MaxAlready = false;

    /// <summary>
    /// 能否双向开单
    /// </summary>
    public bool V_IsDoubleSide = false;

    /// <summary>
    /// 是否自处理订单
    /// </summary>
    public bool V_HandleOrderSelf = false;

    public long V_LastKLineTime = 0;


    public BaseTaticsHelper() {
        //cooldown *= (long)V_Min*60 * Util.Second_Ticks;
        //cooldown = AppSetting.Ins.GetInt("CoolDown");
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="setting"></param>
    public virtual void Init(string setting) {
        Console.WriteLine("合约 " + V_Instrument_id+"  setting "+setting);
        Debugger.Warn("合约 " + V_Instrument_id);

        CoinType = V_Instrument_id.Split('-')[0];
        cooldown = 0;

    }

    public long GetCoolDownTest() {

        long cd = (long)V_Min * cooldown * Util.Minute_Ticks; ;
        if (!V_WinClose)
        {
            cd = (long)(V_Min * lossCooldown * Util.Minute_Ticks);
        }
        long leave = (V_Cache.V_KLineData[0].V_Timestamp - V_LastOpTime).Ticks - cd;
        return leave;
    }

    public long GetCoolDown()
    {
        long cd = (long)V_Min * cooldown * Util.Minute_Ticks; ;
        if (!V_WinClose) {
            cd = (long)(V_Min * lossCooldown * Util.Minute_Ticks);
        }
        long leave = (DateTime.UtcNow - V_LastOpTime).Ticks - cd;
        return leave;
    }

    /// <summary>
    /// 设置倍数
    /// </summary>
    /// <param name="m"></param>
    public void SetLeverage(float m)
    {
        V_Leverage = m;
    }

    /// <summary>
    /// 设置止盈止损百分比值
    /// </summary>
    /// <param name="loss"></param>
    /// <param name="win"></param>
    public void SetStopPercent(float loss, float win)
    {
        lossPercent = loss;
        winPercent = win;
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public virtual int MakeOrder(bool isTest=false) {
        return 0;
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    public bool ShouldCloseOrderTest(int dir, float percent,KLine line) {
        //float lossMul = AppSetting.Ins.GetFloat("LossMul");
        //percent *= lossMul;
        bool result = OnShouldCloseOrder(dir, percent,true);
        if (result)
        {
            bool lastResult = V_WinClose;
            V_WinClose = percent > 0;
            if (V_WinClose)
            {
                lossCooldown = 0;
            }
            else {
                if (!lastResult) { lossCooldown = AppSetting.Ins.GetFloat("LossCoolDown_"+CoinType); }
            }
            V_LastOpTime = line.V_Timestamp;
            V_MaxAlready = false;
        }
        return result;
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    public bool ShouldCloseOrder(int dir, float percent)
    {
        bool result = OnShouldCloseOrder(dir, percent);
        if (result)
        {
            bool lastResult = V_WinClose;
            V_WinClose = percent > 0;
            if (V_WinClose)
            {
                lossCooldown = 0;
            }
            else
            {
                if (!lastResult) { lossCooldown = AppSetting.Ins.GetFloat("LossCoolDown_"+CoinType); }
            }
            V_LastOpTime = DateTime.UtcNow;
            V_MaxAlready = false;
        }
        return result;
    }

    protected virtual bool OnShouldCloseOrder(int dir, float percent, bool isTest = false) {
        return false;
    }

    public virtual async Task F_HandleOrder(AccountInfo info) {
    }

    public virtual void  F_HandleOrderTest(TaticsTestRunner testRunner)
    {
    }

    public bool F_CanHanleOrder() {
        DateTime t = DateTime.UtcNow;

        int hourValue = (int)Math.Ceiling((t.Hour + (t.Minute / 60f)) * 100f);

        int v = (int)((V_Min / 60f) * 100f);

        if ((v - hourValue % v) >= 3 || V_Cache.V_KLineData[0].V_Timestamp.Ticks == V_LastKLineTime)
        {
            return false;
        }

        V_LastKLineTime = V_Cache.V_KLineData[0].V_Timestamp.Ticks;

        return true;
    }

    /// <summary>
    /// 刷新历史数据
    /// </summary>
    public virtual async Task RunHistory()
    {
        Console.WriteLine(V_Instrument_id + ":获取历史数据");

        Debugger.Log(V_Instrument_id + ":获取历史数据");

        if (V_HistoryCache == null)
        {
            V_HistoryCache = new KLineCache();
        }

        List<KLine> history_data = new List<KLine>();

        SwapApi api = CommonData.Ins.V_SwapApi;

        int length = V_Min;

        DateTime t_start = DateTime.Now.AddMinutes(-length * 2000);

        DateTime t_end = DateTime.Now;

        while (t_start.AddMinutes(length * 200) < t_end)
        {
            JContainer con = await api.getCandlesDataAsync(V_Instrument_id, t_start, t_start.AddMinutes(length * 200), length * 60);

            List<KLine> d = KLine.GetListFormJContainer(con);

            d.AddRange(history_data);

            history_data.Clear();

            history_data.AddRange(d);

            t_start = t_start.AddMinutes(length * 200);
        }

        Console.WriteLine(V_Instrument_id + ":历史数据 "+ history_data.Count+ "条");

        Debugger.Log(V_Instrument_id + ":历史数据 " + history_data.Count + "条");

        V_HistoryCache.RefreshData(history_data);
        V_LastOpTime = history_data[history_data.Count - 1].V_Timestamp;
    }

    /// <summary>
    /// 是否是周末
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool F_IsWeekend(DateTime time) {

        if (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 清除临时数据
    /// </summary>
    public virtual void ClearTempData() {
        V_LastOpTime = DateTime.UtcNow;
        lossCooldown = 0;
        V_MaxAlready = false;
    }

    public virtual void ClearRunData() { 
        
    }
}