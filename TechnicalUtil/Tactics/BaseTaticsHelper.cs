using System;
using System.Collections.Generic;
using System.Text;

public class BaseTaticsHelper
{
    /// <summary>
    /// 倍数
    /// </summary>
    public float V_Leverage = 50;

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
    /// 冷却
    /// </summary>
    protected long cooldown;//秒

    public BaseTaticsHelper() { }

    public virtual long GetCoolDown() {
        return 0;
    }

    public virtual int MakeOrder() {
        return 0;
    }

    public virtual bool ShouldCloseOrder(int dir, float percent) {
        V_LastOpTime = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// 刷新历史数据
    /// </summary>
    /// <param name="history"></param>
    public virtual void RefreshHistory(KLineCache history)
    {
        V_HistoryCache = history;
    }

    public virtual float GetResult()
    {
        return 0;
    }
}