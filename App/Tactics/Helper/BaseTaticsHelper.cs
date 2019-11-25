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
    public virtual async Task RunHistory()
    {
    }

    public virtual float GetResult()
    {
        return 0;
    }

    public virtual void ClearTempData() { 
    
    }
}