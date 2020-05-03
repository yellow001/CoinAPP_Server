using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 一小时 MA操作
/// </summary>
public class HourMATaticsHelper: BaseTaticsHelper
{
    /// <summary>
    /// 采样点
    /// </summary>
    public int V_Length = 1;

    /// <summary>
    /// 周期
    /// </summary>
    public List<int> V_CycleList = new List<int>() { 5, 7, 10 };

    public int V_HourCycle = 7;

    /// <summary>
    /// 历史正均值
    /// </summary>
    float result_add_avg = 0;

    /// <summary>
    /// 历史负均值
    /// </summary>
    float result_mul_avg = 0;

    /// <summary>
    /// 结果均值
    /// </summary>
    float result_avg = 0;

    #region 重载
    /// <summary>
    /// 初始化设置 合约;K线时长;采样点;周期(小_中_大);倍数
    /// </summary>
    /// <param name="setting"></param>
    public override void Init(string setting)
    {
        string[] strs = setting.Split(';');
        if (strs.Length >= 4)
        {
            V_Instrument_id = strs[0];
            V_Min = int.Parse(strs[1]);
            V_Length = int.Parse(strs[2]);
            V_Leverage = float.Parse(strs[3]);
        }
        //Console.WriteLine(V_Instrument_id + ":合约 " + V_Instrument_id);
        base.Init(setting);
    }

    /// <summary>
    /// 刷新历史数据
    /// </summary>
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
    public override int MakeOrder()
    {
        return GetValue(true,0);
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
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
            //if (percent >= winPercent)
            //{
            //    //达到 止盈后，判断继续持有还是平仓
            //    if (dir > 0)
            //    {
            //        if (sign < 0)
            //        {
            //            //有较为强烈的空头排列信号
            //            return true;
            //        }
            //    }
            //    else
            //    {
            //        if (sign > 0)
            //        {
            //            //有较为强烈的多头排列信号
            //            return true;
            //        }
            //    }
            //    return true;
            //}
            int sign = GetValue(false, dir);

            if (percent >= winPercent)
            {
                return sign > 0;
            }
        }

        return false;
    }

    #endregion

    #region 策略方法

    /// <summary>
    /// 获取 MA 值
    /// </summary>
    /// <param name="index">下标</param>
    /// <returns></returns>
    float GetMAValue(int length, int index = 0)
    {
        if (V_Cache == null)
        {
            return 0;
        }

        if (V_Cache.V_KLineData.Count < length + index)
        {
            return 0;
        }

        return MA.GetMA(length, V_Cache.V_KLineData.GetRange(index, length));
    }

    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <returns></returns>
    int GetValue(bool isOrder,int orderDir)
    {
        #region 点 计算
        List<float> pList_1 = new List<float>();
        List<float> pList_2 = new List<float>();
        List<float> pList_3 = new List<float>();

        List<float> pList84 = new List<float>();

        for (int i = 0; i < V_Length; i++)
        {
            float p1 = GetMAValue(V_CycleList[0], i);
            float p2 = GetMAValue(V_CycleList[1], i);
            float p3 = GetMAValue(V_CycleList[2], i);

            float p84 = GetMAValue(84, i);

            pList_1.Add(p1);
            pList_2.Add(p2);
            pList_3.Add(p3);

            pList84.Add(p84);
        }
        #endregion

        float bigDir = GetMA84Value(pList84);

        int dir = 0;

        #region 1. 计算 MA排列

        for (int i = 0; i < V_Length; i++)
        {
            if (pList_1[i] >= pList_2[i] && pList_2[i] >= pList_3[i])
            {
                //符合多头排列
                dir = 1;
            }
            else if (pList_1[i] <= pList_2[i] && pList_2[i] <= pList_3[i])
            {
                //符合空头排列
                dir = -1;
            }

        }

        if (isOrder)
        {
            if (bigDir > 0 && dir > 0)
            {
                return 1;
            }
            else if (bigDir < 0 && dir < 0)
            {
                return -1;
            }
        }
        else
        {
            //返回>0就是要平仓
            //if (bigDir > 0)
            //{
                if (orderDir < 0 && dir > 0)
                {
                    return 1;
                }
            //}
            //else if (bigDir < 0)
            //{
                if (orderDir > 0 && dir < 0)
                {
                    return 1;
                }
            //}
        }


        #endregion
        return 0;

    }

    float GetMA84Value(List<float> pList84)
    {

        float temp = 0;

        for (int i = 0; i < pList84.Count; i++)
        {

            float m = pList84[i];
            float y = V_Cache.V_KLineData[i].V_ClosePrice;

            temp += m >= y ? -1 : 1;
        }
        return temp;
    }

    #endregion
}