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
    /// 冷却
    /// </summary>
    protected long cooldown;//秒

    public BaseTaticsHelper() { }

    /// <summary>
    /// 初始化设置
    /// </summary>
    /// <param name="setting"></param>
    public virtual void Init(string setting) {

    }

    public long GetCoolDown() {
        long leave = (V_Cache.V_KLineData[0].V_Timestamp - V_LastOpTime).Ticks - cooldown * 60 * 10000 * 1000 * V_Min;
        return leave;
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
    public virtual bool ShouldCloseOrder(int dir, float percent) {
        V_LastOpTime = DateTime.UtcNow;
        return true;
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
        V_HistoryCache.RefreshData(history_data);
    }
    
    /// <summary>
    /// 清除临时数据
    /// </summary>
    public virtual void ClearTempData() { 
    
    }
}