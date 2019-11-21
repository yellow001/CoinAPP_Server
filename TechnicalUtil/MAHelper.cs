using System;
using System.Collections.Generic;
using System.Text;

/** 五分钟 MA 多空头排列
 
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

public class MAHelper
{
    public static List<int> cycleList=new List<int>() { 5, 15, 30 };

    public static void SetCycle(int min, int mid, int big) {
        if (min < mid && mid < big && big < 120) {
            cycleList[0] = min;
            cycleList[1] = mid;
            cycleList[2] = big;
        }
    }

    public static float mul = 50;

    public static void SetMul(float m) {
        mul = m;
    }

    /// <summary>
    /// 获取排列强度
    /// </summary>
    /// <param name="ma"></param>
    /// <param name="dir">大于0为多，其他均为空</param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static float GetValue(MA ma,int dir=1, int count = 3)
    {
        #region 点 计算
        List<float> pList_1 = new List<float>();
        List<float> pList_2 = new List<float>();
        List<float> pList_3 = new List<float>();

        List<float> pList120 = new List<float>();

        for (int i = 0; i < count; i++)
        {
            float p1 = ma.GetMAValue(5, i);
            float p2 = ma.GetMAValue(15, i);
            float p3 = ma.GetMAValue(30, i);

            float p120 = ma.GetMAValue(120, i);

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

        for (int i = 0; i+1 < count; i++)
        {
            float k1 = (pList_1[i] - pList_1[i + 1]) / pList_1[i + 1];
            float k2 = (pList_2[i] - pList_2[i + 1]) / pList_2[i + 1];
            float k3 = (pList_3[i] - pList_3[i + 1]) / pList_3[i + 1];

            if (dir <= 0) {
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

        for (int i = 0; i < count; i++)
        {
            if (dir > 0)
            {
                if (pList_1[i] >= pList_2[i] && pList_2[i] >= pList_3[i])
                {
                    //符合多头排列
                    //temp += count - i;
                    temp += 1;
                    rightCount += 1;
                }
                else {
                    temp -= 1;
                }
            }
            else
            {
                if (pList_1[i] <= pList_2[i] && pList_2[i] <= pList_3[i])
                {
                    //符合空头排列
                    //temp += count - i;
                    temp += 1;
                    rightCount += 1;
                }
                else {
                    temp -= 1;
                }
            }

        }
        //权重值  = 1+2+3.。。+count = (count+1)*count*0.5f
        //V3.0 2019-11-21 去他妈的权重，干掉
        float result_MA = temp / count;

        #endregion

        #region 2.计算斜率
        float kTemp1 = GetKValue(kList_1);
        float kTemp2 = GetKValue(kList_2);
        float kTemp3 = GetKValue(kList_3);

        float result_K = (kTemp1*3+kTemp2*2+kTemp3)/6;

        #endregion

        #region 3.MA120 计算

        float result_MA120 = GetMA120Value(ma,pList120,dir);

        #endregion

        //result_all = (result_MA * 3 + result_K * 2 + result_120 * 1) / 6
        return (result_MA * 5 + result_K * 2 + result_MA120 * 3) / 10;
    }

    static float GetKValue(List<float> kList) {

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
        float result = temp/kList.Count * 100;
        return result * ((mul * 0.02f) + 1);
    }

    static float GetMA120Value(MA ma,List<float> pList120,int dir) {

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
            float y = ma.cache.kLineData[i].closePrice;

            //temp += (m - y) / y * (pList120.Count - i)*(dir>0?1:-1);
            temp+= (m - y) / y * (dir > 0 ? 1 : -1);
        }

        //return temp/Util.GetAddListCount(pList120.Count)*100;
        return temp/pList120.Count * 100;
    }

    public static float GetResult(MA ma, int count = 3)
    {
        //2000 条数据

        //1. MA5 MA15 MA30

        //1.1 count =3
        //result add count: 907
        //result mul count: 769
        //result zero count: 30
        //result add 均值: 0.6314206
        //result mul 均值: -0.5200006
        //result zero 均值: -0.002070717
        //result all count: 1706
        //result all 均值: 0.1012638

        //1.2 count=5
        //result add count: 931
        //result mul count: 755
        //result zero count: 21
        //result add 均值: 0.6044616
        //result mul 均值: -0.5141941
        //result zero 均值: 0.0008328014
        //result all count: 1707
        //result all 均值: 0.1022581

        //1.3 count=8
        //result add count: 941
        //result mul count: 752
        //result zero count: 13
        //result add 均值: 0.582562
        //result mul 均值: -0.4960645
        //result zero 均值: -0.001763125
        //result all count: 1706
        //result all 均值: 0.1026538

        return GetValue(ma,1,count) - GetValue(ma,-1,count);
    }
}