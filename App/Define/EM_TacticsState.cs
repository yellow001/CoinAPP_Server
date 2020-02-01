using System;
using System.Collections.Generic;
using System.Text;

public enum EM_TacticsState:int
{
    /// <summary>
    /// 正常
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 开始
    /// </summary>
    Start = 1,

    /// <summary>
    /// 停止
    /// </summary>
    Stop=2,

    /// <summary>
    /// 暂停
    /// </summary>
    Pause=3,

    /// <summary>
    /// 开空
    /// </summary>
    Short=4,

    /// <summary>
    /// 平空
    /// </summary>
    CloseShort=5,

    /// <summary>
    /// 开多
    /// </summary>
    Long=6,

    /// <summary>
    /// 平多
    /// </summary>
    CloseLong=7,

    /// <summary>
    /// 全平
    /// </summary>
    CloseAll=8,

    /// <summary>
    /// 对冲
    /// </summary>
    Hedge=9,
}
