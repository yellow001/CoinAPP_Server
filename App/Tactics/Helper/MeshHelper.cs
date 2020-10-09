using System;
using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 订单操作
/// </summary>
public class OrderOperation {

    /// <summary>
    /// 操作 0：无 1：开仓 -1：平仓
    /// </summary>
    public int Operation;

    /// <summary>
    /// 方向 0：无 1：多 -1：空
    /// </summary>
    public int Dir;

    /// <summary>
    /// 百分比(开仓时是指可用余额的百分比  平仓时是指订单的百分比)
    /// </summary>
    public float Percent;

    public OrderOperation() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="op">操作 0：无 1：开仓 -1：平仓</param>
    /// <param name="dir">方向 0：无 1：多 -1：空</param>
    /// <param name="p">百分比</param>
    public OrderOperation(int op, int dir, float p) {
        Operation = op;
        Dir = dir;
        Percent = p;
    }
}

public class MeshHelper: BaseTaticsHelper
{
    #region 参数
    /// <summary>
    /// 网格交易整个百分比
    /// </summary>
    float WholePercent = 0;

    /// <summary>
    /// 兼容百分比
    /// </summary>
    float CompatiblePercent = 0;
    #endregion

    #region 运行时变量

    List<int> orderList = new List<int>();

    /// <summary>
    /// 网格交易最高价
    /// </summary>
    float MeshHighPrice = 0;

    /// <summary>
    /// 网格交易中间价
    /// </summary>
    float MeshMidPrice = 0;

    /// <summary>
    /// 网格交易最低价
    /// </summary>
    float MeshLowPrice = 0;

    /// <summary>
    /// 网格交易每段价格  = (最高价-最低价)/5
    /// </summary>
    float PerPrice = 0;

    /// <summary>
    /// 是否可以开单
    /// </summary>
    bool CanOpenOrder = false;

    bool Reset = false;
    DateTime ResetTime;
    DateTime FirstStopTime;
    #endregion

    #region 重载

    /// <summary>
    /// 初始化设置  合约名;时长;网格价格差百分比;兼容涨跌百分比;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 5)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            WholePercent = float.Parse(strs[2]);
            CompatiblePercent = float.Parse(strs[3]);
            V_Leverage = float.Parse(strs[4]);

            V_HandleOrderSelf = true;
        }
        //Console.WriteLine(V_Instrument_id + ":合约 " + V_Instrument_id);
        base.Init(setting);
    }

    /// <summary>
    /// 分析历史数据
    /// </summary>
    /// <returns></returns>
    public override async Task RunHistory()
    {
        await base.RunHistory();

        Console.WriteLine(V_Instrument_id + ":分析结果");
        Debugger.Warn(V_Instrument_id + ":分析结果");
        TaticsTestRunner.TestRun(this);
        Console.WriteLine(V_Instrument_id + ":分析历史数据完毕");
        Debugger.Warn(V_Instrument_id + ":分析历史数据完毕");
    }

    /// <summary>
    /// 下单
    /// </summary>
    /// <returns>
    /// 1 多单 -1 空单 0 不开单
    /// </returns>
    public override int MakeOrder(bool isTest = false)
    {
        return GetValue(true, 0, isTest);
    }

    /// <summary>
    /// 是否需要平仓
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    protected override bool OnShouldCloseOrder(int dir, float percent, bool isTest = false)
    {
        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            int result = GetValue(false, dir, isTest);

            if (percent >= winPercent)
            {
                return true;
            }

            if (percent >= winPercent * 0.42f)
            {
                return result > 0;
            }
        }
        return false;
    }

    #endregion

    #region 策略方法


    int GetValue(bool isOrder, int orderDir, bool isTest = false)
    {
        if (!isTest)
        {
            DateTime t = DateTime.UtcNow;

            int hourValue = (int)Math.Ceiling((t.Hour + (t.Minute / 60f)) * 100f);

            int v = (int)((V_Min / 60f) * 100f);

            if ((v - hourValue % v) > 4 || (V_LastOpTime.Day == t.Day && V_LastOpTime.Hour == t.Hour))
            {
                return 0;
            }
        }

        return 0;
    }

    void RefreshMeshPrice() {
        //刷新网格策略参数
        MeshHighPrice = V_Cache.V_KLineData[0].V_HightPrice;
        MeshLowPrice = V_Cache.V_KLineData[0].V_LowPrice;

        for (int i = 0; i < V_Cache.V_KLineData.Count; i++)
        {
            KLine kLine = V_Cache.V_KLineData[i];
            if (kLine.V_HightPrice > MeshHighPrice) {
                MeshHighPrice = kLine.V_HightPrice;
            }
            if (kLine.V_LowPrice < MeshLowPrice) {
                MeshLowPrice = kLine.V_LowPrice;
            }

            float p = (MeshHighPrice - MeshLowPrice) / MeshLowPrice;
            if (p*100 >= WholePercent) {
                break;
            }

        }

        //计算 mid
        MeshMidPrice = (MeshHighPrice + MeshLowPrice) * 0.5f;

        // 网格交易每段价格  = (最高价-最低价)/5
        PerPrice = (MeshHighPrice - MeshLowPrice) * 0.2f;
    }

    #endregion

    #region 处理订单

    public override async Task F_HandleOrder(AccountInfo info)
    {
        List<OrderOperation> result = HandleOrder(info.V_Positions);
        if (result != null && result.Count > 0)
        {
            foreach (var item in result)
            {
                if (item.Operation != 0 && item.Dir != 0)
                {
                    if (item.Operation == 1)
                    {
                        //开仓
                        await info.MakeOrder(item.Dir, info.GetAvailMoney(), item.Percent);
                    }
                    else
                    {
                        //平仓
                        await info.ClearPositions(item.Dir, item.Percent);
                    }
                }

            }
        }

    }

    public override void F_HandleOrderTest(TaticsTestRunner testRunner)
    {
        List<OrderOperation> result = HandleOrder(testRunner.V_Positions,true);

        if (result != null && result.Count > 0) {
            foreach (var item in result)
            {
                if (item.Operation != 0 && item.Dir != 0) {
                    if (item.Operation == 1)
                    {
                        //开仓
                        testRunner.OpenOrder(item.Dir, V_Cache.V_KLineData[0], item.Percent);
                    }
                    else {
                        //平仓
                        testRunner.CloseOrder(V_Cache.V_KLineData[0], item.Dir, item.Percent);
                    }
                }
                
            }
        }
    }

    /// <summary>
    /// 处理订单
    /// </summary>
    /// <param name="isTest"></param>
    /// <returns>操作列表</returns>
    List<OrderOperation> HandleOrder(List<Position> curPositionList,bool isTest=false) {

        float orderPercent = float.Parse(AppSetting.Ins.GetValue("OrderValue"));

        if (MeshHighPrice == 0) {
            RefreshMeshPrice();
            CanOpenOrder = true;
        }

        if (Reset) {

            if (curPositionList.Count > 0) {
                //保留着上次超过兼容区的单，看看要不要平
                if (orderList.Count == 0)
                {
                    //列表没有了，应该是没平干净，平了
                    return CloseOrder(0);
                }
                else {
                    for (int i = 0; i < curPositionList.Count; i++)
                    {
                        Position position = curPositionList[i];
                        if (position.V_Dir > 0) {
                            //多单存着
                            if (!orderList.Contains(-1) && !orderList.Contains(-2)) {
                                //记录里没有多单，平掉
                                return CloseOrder(1);
                            }
                        }
                        if (position.V_Dir < 0)
                        {
                            //空单存着
                            if (!orderList.Contains(1) && !orderList.Contains(2))
                            {
                                //记录里没有空单，平掉
                                return CloseOrder(-1);
                            }
                        }

                        //判断下要不要走
                        float p = position.GetPercent(V_Cache.V_KLineData[0].V_ClosePrice);
                        if (isTest) {
                            p = position.GetPercentTest(V_Cache.V_KLineData[0], lossPercent);
                        }
                        if (p < winPercent * 0.5f)
                        {
                            return position.V_Dir > 0 ? CloseOrder(1) : CloseOrder(-1);
                        }
                    }
                }
            }

            if ((V_Cache.V_KLineData[0].V_Timestamp - ResetTime).TotalMinutes > lossCooldown * V_Min)
            {
                RefreshMeshPrice();
                Reset = false;
            }
            else {
                //没到冷却期，无操作
                return null;
            }
        }

        List<OrderOperation> result = new List<OrderOperation>();

        //只划分5个区域

        //获取当前价格
        float curValue = V_Cache.V_KLineData[0].V_ClosePrice;

        //当前价格所在的区间
        int curIndex = 0;

        float indexValue = 0f;
        if (curValue >= MeshMidPrice)
        {
            indexValue = (curValue - (MeshMidPrice + PerPrice * 0.5f)) / PerPrice;
        }
        else {
            indexValue = (curValue - (MeshMidPrice - PerPrice * 0.5f)) / PerPrice;
        }
        curIndex = (int)indexValue;

        if (indexValue > 2 || indexValue < -2)
        {
            if (CanOpenOrder)
            {
                CanOpenOrder = false;
                FirstStopTime = V_Cache.V_KLineData[0].V_Timestamp;
            }
            if ((V_Cache.V_KLineData[0].V_Timestamp - FirstStopTime).TotalMinutes > cooldown * V_Min)
            {
                //超过网格区一段时间了

                //平仓所有 重新计算网格数值
                return ResetOrder(0);
            }

            if (indexValue > 2)
            {
                //区域上方 todo
                bool IsCompatible = true;

                float p = (curValue - MeshHighPrice) / MeshHighPrice;
                if (p*100 > CompatiblePercent) {
                    IsCompatible = false;
                }

                if (IsCompatible)
                {
                    //还在兼容区内
                    return null;
                }
                else {
                    //超过兼容区 
                    //平仓所有空单 重新计算网格数值
                    return ResetOrder(-1);
                }
            }
            else if (indexValue < -2)
            {
                //区域下方 todo
                bool IsCompatible = true;

                float p = (MeshLowPrice - curValue) / curValue;
                if (p*100 > CompatiblePercent)
                {
                    IsCompatible = false;
                }

                if (IsCompatible)
                {
                    //还在兼容区内
                    return null;
                }
                else
                {
                    //超过兼容区 
                    //平仓所有多单 重新计算网格数值
                    return ResetOrder(1);
                }
            }
        }
        else {
            if (!CanOpenOrder) {
                CanOpenOrder = true;
            }
        }

        if (CanOpenOrder) {

            //判断下操作时间


            switch (curIndex)
            {
                case 2:
                    //1. 是否已经在2开过空单？没的话 开空 20%
                    if (!orderList.Contains(2))
                    {
                        //order short 20%
                        result.Add(new OrderOperation(1, -1, orderPercent*2));
                        orderList.Add(2);
                    }
                    //2. 是否在-2开过多单且单子还在？是的话 平多 100%
                    if (orderList.Contains(-2))
                    {
                        //close long 100%
                        result.Add(new OrderOperation(-1, 1, 1f));
                        orderList.Remove(-2);
                        if (orderList.Contains(-1))
                        {
                            orderList.Remove(-1);
                        }
                    }
                    break;
                case 1:
                    //1. 是否已经在1开过空单？没的话 开空 10%
                    if (!orderList.Contains(1))
                    {
                        //open short 10%
                        result.Add(new OrderOperation(1, -1, orderPercent));
                        orderList.Add(1);
                    }
                    //2. 是否在-1开过多单且单子还在？是的话 平多 10%（就是单子的1/3）
                    if (orderList.Contains(-1))
                    {
                        //close long 10%
                        if (orderList.Contains(-2))
                        {
                            result.Add(new OrderOperation(-1, 1, 0.35f));
                        }
                        else
                        {
                            result.Add(new OrderOperation(-1, 1, 1f));
                        }

                        orderList.Remove(-1);
                    }
                    break;
                case 0:
                    //什么都不用做
                    break;
                case -1:
                    //1. 是否已经在-1开过多单？没的话 开多 10%
                    if (!orderList.Contains(-1))
                    {
                        //order long 10%
                        result.Add(new OrderOperation(1, 1, orderPercent));
                        orderList.Add(-1);
                    }
                    //2. 是否在1开过空单且单子还在？是的话 平空 10%（就是单子的1/3）
                    if (orderList.Contains(1))
                    {
                        //close short 10%
                        if (orderList.Contains(2))
                        {
                            result.Add(new OrderOperation(-1, -1, 0.35f));
                        }
                        else
                        {
                            result.Add(new OrderOperation(-1, -1, 1f));
                        }

                        orderList.Remove(1);
                    }
                    break;
                case -2:
                    //1. 是否已经在-2开过多单？没的话 开多 20%
                    if (!orderList.Contains(-2))
                    {
                        //order long 20%
                        result.Add(new OrderOperation(1, 1, orderPercent*2));
                        orderList.Add(-2);
                    }

                    //2. 是否在2开过空单且单子还在？是的话 平空 20%（就是单子的2/3）
                    if (orderList.Contains(2))
                    {
                        //close short 20%
                        result.Add(new OrderOperation(-1, -1, 1f));
                        orderList.Remove(2);
                        if (orderList.Contains(1))
                        {
                            orderList.Remove(1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        
        return result;
    }

    List<OrderOperation> ResetOrder(int dir) {
        //平仓所有 重新计算网格数值

        Reset = true;
        ResetTime = V_Cache.V_KLineData[0].V_Timestamp;

        return CloseOrder(dir);
    }

    List<OrderOperation> CloseOrder(int dir) {
        List<OrderOperation> result = new List<OrderOperation>();

        if (dir == 0 || dir == 1)
        {
            result.Add(new OrderOperation(-1, 1, 1));

            if (orderList.Contains(-1))
            {
                orderList.Remove(-1);
            }
            if (orderList.Contains(-2))
            {
                orderList.Remove(-2);
            }
        }

        if (dir == 0 || dir == -1)
        {
            result.Add(new OrderOperation(-1, -1, 1));

            if (orderList.Contains(1))
            {
                orderList.Remove(1);
            }
            if (orderList.Contains(2))
            {
                orderList.Remove(2);
            }
        }

        return result;
    }


    public override void ClearTempData()
    {
        base.ClearTempData();
        ResetHelperData();
    }

    public override void ClearRunData()
    {
        base.ClearRunData();
        ResetHelperData();
    }

    void ResetHelperData() {
        MeshHighPrice = 0;
        MeshLowPrice = 0;
        MeshMidPrice = 0;

        orderList.Clear();
    }
    #endregion
}
