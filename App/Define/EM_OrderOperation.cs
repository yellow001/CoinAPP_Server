
/// <summary>
/// 订单操作
/// </summary>
public enum EM_OrderOperation:int
{

    /// <summary>
    /// 正常
    /// </summary>
    Normal=0,

    /// <summary>
    /// 只做空
    /// </summary>
    ShortOnly=1,

    /// <summary>
    /// 只做多
    /// </summary>
    LongOnly=2,

    /// <summary>
    /// 只做空且盈利不平仓
    /// </summary>
    ShortNoClose=3,

    /// <summary>
    /// 只做多且盈利不平仓
    /// </summary>
    LongNoClose=4,

    /// <summary>
    /// 盈利不平仓
    /// </summary>
    NoClose=5,
}
