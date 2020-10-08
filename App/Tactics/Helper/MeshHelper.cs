using System;
using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MeshHelper: BaseTaticsHelper
{
    #region 参数
    int MaLength = 5;
    int MaLength2 = 10;

    int LongMaLength = 20;

    /// <summary>
    /// 网格交易整个百分比
    /// </summary>
    float WholePercent = 0;
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
    /// 网格交易每段j价格  = (最高价-最低价)/5
    /// </summary>
    float PerPrice = 0;
    #endregion




    #region 重载

    /// <summary>
    /// 初始化设置  合约名;时长;MA参考线1;MA参考线2;长期MA参考线;止盈冷却;倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 7)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            MaLength = int.Parse(strs[2]);
            MaLength2 = int.Parse(strs[3]);
            LongMaLength = int.Parse(strs[4]);
            cooldown = int.Parse(strs[5]);
            V_Leverage = float.Parse(strs[6]);
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

    #endregion

    #region 处理订单

    public override async Task F_HandleOrder(AccountInfo info)
    {
    }

    public override void F_HandleOrderTest(TaticsTestRunner testRunner)
    {
    }

    void HandleOrder(bool isTest=false) {

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

        if (indexValue > 2)
        {
            //超过区域上方 todo

            //1 还在兼容区内

            //1.2 有多单？ 
        }
        else if (indexValue < -2)
        {
            //低于区域下方 todo
        }

        switch (curIndex)
        {
            case 2:
                //1. 是否已经在2开过空单？没的话 开空 20%
                if (!orderList.Contains(2)) { 
                    //order short 20%
                }
                //2. 是否在-2开过多单且单子还在？是的话 平多 20%（就是单子的2/3）
                if (orderList.Contains(-2))
                {
                    //close long 20%
                }
                break;
            case 1:
                //1. 是否已经在1开过空单？没的话 开空 10%
                if (!orderList.Contains(1))
                {
                    //open short 10%
                }
                //2. 是否在-1开过多单且单子还在？是的话 平多 10%（就是单子的1/3）
                if (orderList.Contains(1))
                {
                    //close long 10%
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
                }
                //2. 是否在1开过空单且单子还在？是的话 平空 10%（就是单子的1/3）
                if (orderList.Contains(-1))
                {
                    //close short 10%
                }
                break;
            case -2:
                //1. 是否已经在-2开过多单？没的话 开多 20%
                if (!orderList.Contains(-2))
                {
                    //order long 20%
                }

                //2. 是否在2开过空单且单子还在？是的话 平空 20%（就是单子的2/3）
                if (orderList.Contains(-2))
                {
                    //close short 20%
                }
                break;
            default:
                break;
        }

    }
    #endregion
}
