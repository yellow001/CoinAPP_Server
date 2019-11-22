using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 基础策略类（永续合约）
/// </summary>
public class BaseTactics
{

    /// <summary>
    /// 上次操作时间戳
    /// </summary>
    protected DateTime lastOperationTime;

    /// <summary>
    /// 当前订单
    /// </summary>
    protected Order order;

    /// <summary>
    /// 止损百分比（*100）
    /// </summary>
    protected float stopLossPercent;

    /// <summary>
    /// 止盈百分比（*100）
    /// </summary>
    protected float stopWinPercent;

    /// <summary>
    /// 操作冷却
    /// </summary>
    protected float operationCoolDown;

    public BaseTactics() { }


}