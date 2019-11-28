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
    protected string V_Instrument_id;

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
    /// 上次平仓时间
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
    protected long cooldown=2;

    public BaseTaticsHelper() {
        cooldown *= (long)V_Min*60 * 10000 * 1000;
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="setting"></param>
    public virtual void Init(string setting) {

    }

    public long GetCoolDownTest() {
        long leave = (V_Cache.V_KLineData[0].V_Timestamp - V_LastOpTime).Ticks - cooldown;
        return leave;
    }

    public long GetCoolDown()
    {
        long leave = (DateTime.UtcNow - V_LastOpTime).Ticks - cooldown;
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
    public virtual int MakeOrder() {
        return 0;
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    public bool ShouldCloseOrder(int dir, float percent,KLine line) {
        bool result = OnShouldCloseOrder(dir,percent);
        if (result) {
            V_LastOpTime = line.V_Timestamp;
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
            V_LastOpTime = DateTime.UtcNow;
        }
        return result;
    }

    protected virtual bool OnShouldCloseOrder(int dir, float percent) {
        return false;
    }

    /// <summary>
    /// 刷新历史数据
    /// </summary>
    public virtual async Task RunHistory()
    {
        Console.WriteLine("获取历史数据");

        if (V_HistoryCache == null)
        {
            V_HistoryCache = new KLineCache();
        }

        List<KLine> history_data = new List<KLine>();

        SwapApi api = CommonData.Ins.V_SwapApi;

        int length = V_Min;

        DateTime t_start = DateTime.Now.AddMinutes(-length * 1000);

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
        V_HistoryCache.RefreshData(history_data);
        V_LastOpTime = history_data[history_data.Count - 1].V_Timestamp;
    }
    
    /// <summary>
    /// 清除临时数据
    /// </summary>
    public virtual void ClearTempData() {
        V_LastOpTime = DateTime.UtcNow;
    }
}