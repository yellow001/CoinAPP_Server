/** MA 多空头排列
 
    V1.0 2019-11-20

    参数 MA5  MA15  MA30      MA120
    权重 MA排列 3   MA斜率 2   MA120 1
    
    注：+/- 表示要判断正负

    1.MA排列计算
    越靠前权重越大，符合排列则 1*权重，否则 0
    例如计算3个时间点的MA，权重就是 3 2 1 ，若三个时间点都符合多（空）头排列，则结果为 （1*3+1*2+1*1）/6 = 1

    2.斜率计算
    说明：
        选取时间点作为X轴变量，跨度假设为1（好计算斜率），则临近的两个时间点有：
            p1(x1,y1)  p2(x2,y2)
            k = (y2-y1)/(x2-x1)=y2-y1
        但是不同币种的价格跨度不一样，所以 k 值变化大，不利于计算，把币价本身算进去比较好，于是有以下优化
            k=(y2-y1)/y1
        若 |k|>0.01  ,则相当于5分钟内变化了 1% ，对于50倍期货就是50%，应给予重视
    
    计算：
        k值也是越靠前权重越大
        令外：
            |k| < 0.008,加权值 p = k*1
            |k|>0.008,加权值 p = ( (|k|-0.08)*2+|k|*1 )*(k>0?1:-1)

        假设 k1=0.01  k2=0.05,则
        result = ((k1-0.008)*2+k1*1)*2 + (k2*1)*1
               = (0.004+0.01)*2 + 0.005
               = 0.028 + 0.005
               = 0.033
        最后 result/(2+1)*100 ~= 1.1 
        (PS：为什么*100？自我感觉的。。。。。平常波动也就 1% 不到，垃圾时间居多，干脆把百分比值算出来好了)
        
        v2.0:斜率计算最好带上杠杆倍数，倍数越大，斜率参考意义越大
        result最后再 *（(倍数/100)+1)


    3.MA120位置点的影响
    说明：
        为何不计算 MA60 的影响？因为 MA120 更有参考性
        为何只计算 位置，不计算斜率？因为大周期斜率参考性不大。。。
    计算：
        位置越远，越有支撑（压制）作用

        位置也是越靠前权重越大
        假设有点 p1 p2 p3，y值对应 y1 y2 y3
        MA120 分别为 m1 m2 m3  权重 3 2 1 （6） 
        
        注：+/- 表示要判断多空 (dir>0?1:-1)

        temp = (y1 - m1) / y1 * 3 * (+/ -) + (y2 - m2) / y2 * 2 * (+/ -) + (m3 - y3) / y3 * 1 * (+/ -)
        result = temp / 6 * 100(*100的原因同2中的斜率计算)
        

    result_all = ( result_MA *5 + result_K *2 +result_120 *3 )/10

    多空对比值 result_final = result_多 - result_空


    V3.0 2019-11-21 去他妈的权重，干掉
 * **/


using NetFrame.Tool;
using Newtonsoft.Json.Linq;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// MA 多空头排列策略
/// </summary>
public class MATaticsHelper: BaseTaticsHelper,ICycleTatics
{
    /// <summary>
    /// 采样点
    /// </summary>
    public int V_Length = 5;

    /// <summary>
    /// 周期
    /// </summary>
    public List<int> V_CycleList = new List<int>() { 5, 15, 30 };

    /// <summary>
    /// 最近K线缓存
    /// </summary>
    public List<KLine> kLineCache = new List<KLine>();

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

        List<float> resultList_add = new List<float>();
        List<float> resultList_mul = new List<float>();

        List<float> klineList_add = new List<float>();
        List<float> klineList_mul = new List<float>();

        int start = 150;
        List<KLine> all_data = V_HistoryCache.V_KLineData;
        for (int i = start; i < all_data.Count - start; i++)
        {
            List<KLine> data = all_data.GetRange(all_data.Count - 1 - start - i, start);

            if (V_Cache == null)
            {
                V_Cache = new KLineCache();
            }
            V_Cache.RefreshData(data);

            float result = GetResult();

            if ((MathF.Abs(result) - 0.01f) > 0)
            {
                if (result > 0)
                {
                    resultList_add.Add(result);
                }
                else
                {
                    resultList_mul.Add(result);
                }
            }

            //KLine line = data[0];
            //float p = (line.V_ClosePrice - line.V_OpenPrice) / line.V_OpenPrice * V_Leverage * 100;
            //if (MathF.Abs(p) >= 0.1f)
            //{
            //    if (line.V_OpenPrice > line.V_ClosePrice)
            //    {
            //        //跌
            //        klineList_mul.Add(p);
            //    }
            //    else
            //    {
            //        //涨
            //        klineList_add.Add(p);
            //    }
            //}
        }

        result_add_avg = Util.GetAvg(resultList_add);
        result_mul_avg = Util.GetAvg(resultList_mul);

        resultList_add.AddRange(resultList_mul);

        result_avg = Util.GetAvg(resultList_add);

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
        return GetSign(true);
        //float sign = GetSign();
        //if (sign > 0)
        //{
        //    //多单
        //    return 1;
        //}
        //else if (sign < 0)
        //{
        //    //空单
        //    return -1;
        //}
        ////不开单
        //return 0;
    }

    /// <summary>
    /// 是否要平仓
    /// </summary>
    /// <param name="dir">大于0多  其余空</param>
    /// <param name="percent">当前盈利百分比值</param>
    /// <returns></returns>
    protected override bool OnShouldCloseOrder(int dir, float percent, bool isTest = false)
    {
        int sign = GetSign();

        if (percent <= lossPercent)
        {
            //无条件止损
            return true;
        }
        else
        {
            //if (percent <= lossPercent * 0.8f)
            //{
            //    //亏损率达止损的0.8，判断继续持有还是平仓
            //    if (dir > 0)
            //    {
            //        //多单 亏损
            //        if (sign < 0)
            //        {
            //            //有较为强烈的空头排列信号
            //            return true;
            //        }
            //    }
            //    else
            //    {
            //        //空单 亏损
            //        if (sign > 0)
            //        {
            //            //有较为强烈的多头排列信号
            //            return true;
            //        }
            //    }
            //}
            if (percent >= winPercent)
            {
                //达到 止盈后，判断继续持有还是平仓
                if (dir > 0)
                {
                    if (sign < 0)
                    {
                        //有较为强烈的空头排列信号
                        return true;
                    }
                }
                else
                {
                    if (sign > 0)
                    {
                        //有较为强烈的多头排列信号
                        return true;
                    }
                }
                return true;
            }
            //else {
            //    V_MaxPercent = V_MaxPercent < percent ? percent : V_MaxPercent;
            //    if ((percent - V_MaxPercent) < lossPercent*1.618f) {
            //        return true;
            //    }
            //}
        }
        return false;
    }

    public override void ClearTempData()
    {
        kLineCache.Clear();
    }

    #endregion

    #region 策略方法
    public float GetResult()
    {
        return GetValue(1) - GetValue(-1);
    }

    /// <summary>
    /// 获取信号
    /// </summary>
    /// <param name="order">下单 or 止损</param>
    /// <returns></returns>
    int GetSign(bool order=false) {
        float result = GetResult();

        if (kLineCache.Count >= 3)
        {
            kLineCache.RemoveAt(0);
        }
        kLineCache.Add(V_Cache.V_KLineData[0]);

        if (kLineCache.Count < 3) {
            return 0;
        }
        if (kLineCache[2].GetAvg() > kLineCache[1].GetAvg() && kLineCache[1].GetAvg() > kLineCache[0].GetAvg())
        {

            //float temp = result_add_avg;
            //if (order) {
            //    temp = result_avg;
            //}

            //if(result>temp)
            //return 1;
            if (result > 0)
            {
                return 1;
            }

        }
        if (kLineCache[2].GetAvg() < kLineCache[1].GetAvg() && kLineCache[1].GetAvg() < kLineCache[0].GetAvg())
        {
            //float temp = result_mul_avg;
            //if (order)
            //{
            //    temp = result_avg;
            //}

            //if (result< temp)
            //return -1;

            if (result < 0)
            {
                return -1;
            }
        }

        //无信号
        return 0;
    }

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
    float GetValue(int dir = 1)
    {
        #region 点 计算
        List<float> pList_1 = new List<float>();
        List<float> pList_2 = new List<float>();
        List<float> pList_3 = new List<float>();

        List<float> pList120 = new List<float>();

        for (int i = 0; i < V_Length; i++)
        {
            float p1 = GetMAValue(V_CycleList[0], i);
            float p2 = GetMAValue(V_CycleList[1], i);
            float p3 = GetMAValue(V_CycleList[2], i);

            float p120 = GetMAValue(120, i);

            pList_1.Add(p1);
            pList_2.Add(p2);
            pList_3.Add(p3);

            pList120.Add(p120);
        }
        #endregion

        #region 斜率计算

        //斜率计算最后都是有 价格本身介入

        List<float> kList_1 = new List<float>();
        List<float> kList_2 = new List<float>();
        List<float> kList_3 = new List<float>();

        for (int i = 0; i + 1 < V_Length; i++)
        {
            float k1 = (pList_1[i] - pList_1[i + 1]) / pList_1[i + 1];
            float k2 = (pList_2[i] - pList_2[i + 1]) / pList_2[i + 1];
            float k3 = (pList_3[i] - pList_3[i + 1]) / pList_3[i + 1];

            if (dir <= 0)
            {
                k1 = -k1;
                k2 = -k2;
                k3 = -k3;
            }
            kList_1.Add(k1);
            kList_2.Add(k2);
            kList_3.Add(k3);
        }
        #endregion


        #region 1. 计算 MA排列
        float temp = 0;

        float rightCount = 0;

        for (int i = 0; i < V_Length; i++)
        {
            if (dir > 0)
            {
                if (pList_1[i] >= pList_2[i] - pList_1[i] * 0.01f && pList_2[i] >= pList_3[i] - pList_2[i] * 0.01f)
                {
                    //符合多头排列
                    //temp += count - i;
                    temp += 1;
                    rightCount += 1;
                }
                else
                {
                    temp -= 1;
                }
            }
            else
            {
                if (pList_1[i] <= pList_2[i] + pList_1[i] * 0.01f && pList_2[i] <= pList_3[i] + pList_2[i] * 0.01f)
                {
                    //符合空头排列
                    //temp += count - i;
                    temp += 1;
                    rightCount += 1;
                }
                else
                {
                    temp -= 1;
                }
            }

        }
        //权重值  = 1+2+3.。。+count = (count+1)*count*0.5f
        //V3.0 2019-11-21 去他妈的权重，干掉
        float result_MA = temp / V_Length;

        #endregion

        #region 2.计算斜率
        float kTemp1 = GetKValue(kList_1);
        float kTemp2 = GetKValue(kList_2);
        float kTemp3 = GetKValue(kList_3);

        float result_K = (kTemp1 * 3 + kTemp2 * 2 + kTemp3) / 6;

        #endregion

        #region 3.MA120 计算

        float result_MA120 = GetMA120Value(pList120, dir);

        #endregion

        //result_all = (result_MA * 3 + result_K * 2 + result_120 * 1) / 6
        //return (result_MA * 5 + result_K * 2 + result_MA120 * 3) / 10;
        return (result_MA * 5+ result_MA120 * 3) / 8;
    }

    float GetKValue(List<float> kList)
    {

        //| k |< 0.008,加权值 p = k * 1
        //| k |> 0.008, 加权值 p = ((| k | -0.08) * 2 +| k | *1) * (k > 0 ? 1 : -1)

        float temp = 0;
        for (int i = 0; i < kList.Count; i++)
        {
            float value = kList[i];
            float abs = MathF.Abs(value);
            if (abs > 0.08f)
            {
                //temp += ((abs - 0.08f) * 2 + abs)*(value > 0?1:-1)* (kList.Count - i);
                temp += ((abs - 0.08f) * 2 + abs) * (value > 0 ? 1 : -1);
            }
            else
            {
                //temp += value * (kList.Count - i);
                temp += value;
            }
        }

        //return temp/Util.GetAddListCount(kList.Count)*100;

        //v2.0:斜率计算最好带上杠杆倍数，倍数越大，斜率参考意义越大
        //result最后再 *（(倍数 / 50) + 1)

        //V3.0 2019-11-21 去他妈的权重，干掉

        //float result = temp / Util.GetAddListCount(kList.Count) * 100;
        float result = temp / kList.Count * 100;
        return result * ((V_Leverage * 0.02f) + 1);
    }

    float GetMA120Value(List<float> pList120, int dir)
    {

        //说明：
        //为何不计算 MA60 的影响？因为 MA120 更有参考性
        //为何只计算 位置，不计算斜率？因为大周期斜率参考性不大。。。
        //计算：
        //位置越远，越有支撑（压制）作用

        //位置也是越靠前权重越大
        //假设有点 p1 p2 p3，y值对应 y1 y2 y3
        //MA120 分别为 m1 m2 m3 权重 3 2 1 （6） 

        //注：+/- 表示要判断多空 (dir>0?1:-1)

        //temp = (y1 - m1) / y1 * 3 * (+/ -) + (y2 - m2) / y2 * 2 * (+/ -) + (m3 - y3) / y3 * 1 * (+/ -)
        //result = temp / 6 * 100(*100的原因同2中的斜率计算)

        //V3.0 2019-11-21 去他妈的权重，干掉

        float temp = 0;

        for (int i = 0; i < pList120.Count; i++)
        {

            float m = pList120[i];
            float y = V_Cache.V_KLineData[i].V_ClosePrice;

            if (dir > 0)
            {
                temp += (y - m) / y;
            }
            else
            {
                temp += (m - y) / y;
            }
        }

        //return temp/Util.GetAddListCount(pList120.Count)*100;
        return temp / pList120.Count * 100;
    }

    public void SetCycle(string setting)
    {
        string[] cycles = setting.Split('_');
        if (cycles.Length >= 3)
        {
            V_CycleList[0] = int.Parse(cycles[0]);
            V_CycleList[1] = int.Parse(cycles[1]);
            V_CycleList[2] = int.Parse(cycles[2]);
        }
    }

    #endregion
}