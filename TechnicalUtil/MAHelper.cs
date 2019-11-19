using System;
using System.Collections.Generic;
using System.Text;

public class MAHelper
{

    /// <summary>
    /// 是否为多头排列（返回值越大，信号越强烈)
    /// </summary>
    /// <returns></returns>
    public static float LongValue(MA ma,int count=3) {
        /* 
         连续3个点都出现 MA5>MA15>MA30 ，斜率向上且相近，则认为有可能出现多头排列
         */
        float result = 0;

        List<float> pList5 = new List<float>();
        List<float> pList15 = new List<float>();
        List<float> pList30 = new List<float>();
        List<float> pList60 = new List<float>();
        List<float> pList120 = new List<float>();

        for (int i = 0; i < count; i++)
        {
            float p5 = ma.GetMAValue(5, i);
            float p15 = ma.GetMAValue(15, i);
            float p30 = ma.GetMAValue(30, i);

            float p60 = ma.GetMAValue(60, i);
            float p120 = ma.GetMAValue(120, i);

            pList5.Add(p5);
            pList15.Add(p15);
            pList30.Add(p30);

            pList60.Add(p60);
            pList120.Add(p120);

            if (!(p5 > p15 && p15 > p30))
            {
                result = 0;
                break;
            }
            else {
                result+=1;
            }
        }

        if (result > 0) {
            List<float> kList5 = new List<float>();
            List<float> kList15 = new List<float>();
            List<float> kList30 = new List<float>();

            for (int i = 0; i < pList5.Count; i++)
            {
                kList5.Add(pList5[i] - pList5[i + 1]);
            }

            foreach (var item in kList5)
            {
                if (item > 0)
                {
                    result += 1;
                }
                else {
                    result -= 1;
                }
            }

            for (int i = 0; i < pList15.Count; i++)
            {
                kList15.Add(pList15[i] - pList15[i + 1]);
            }

            foreach (var item in kList15)
            {
                if (item > 0)
                {
                    result += 1;
                }
                else {
                    result -= 1;
                }
            }

            for (int i = 0; i < pList30.Count; i++)
            {
                kList30.Add(pList30[i] - pList30[i + 1]);
            }

            foreach (var item in kList30)
            {
                if (item > 0)
                {
                    result += 1;
                }
                else {
                    result -= 1;
                }
            }


            //60 120位置和斜率，可加分或减分
            if (pList30[0] > pList60[0])
            {
                result += 1;
                if (pList60[0] - pList60[1] > 0)
                {
                    result += 1;
                }
            }
            else {
                result -=1;
            }

            if (pList30[0] > pList120[0])
            {
                result += 1;
                if (pList120[0] - pList120[1] > 0)
                {
                    result += 1;
                }
            }
            else {
                result -=1;
            }
        }

        return result;
    }

    public static float ShortValue(MA ma, int count = 3) {
        /* 
         连续3个点都出现 MA5<MA15<MA30 ，斜率向下且相近，则认为有可能出现空头排列
         */
        float result = 0;

        List<float> pList5 = new List<float>();
        List<float> pList15 = new List<float>();
        List<float> pList30 = new List<float>();
        List<float> pList60 = new List<float>();
        List<float> pList120 = new List<float>();

        for (int i = 0; i < count; i++)
        {
            float p5 = ma.GetMAValue(5, i);
            float p15 = ma.GetMAValue(15, i);
            float p30 = ma.GetMAValue(30, i);

            float p60 = ma.GetMAValue(60, i);
            float p120 = ma.GetMAValue(120, i);

            pList5.Add(p5);
            pList15.Add(p15);
            pList30.Add(p30);

            pList60.Add(p60);
            pList120.Add(p120);

            if (!(p5 < p15 && p15 < p30))
            {
                result = 0;
                break;
            }
            else
            {
                result += 1;
            }
        }

        if (result > 0)
        {
            List<float> kList5 = new List<float>();
            List<float> kList15 = new List<float>();
            List<float> kList30 = new List<float>();

            for (int i = 0; i < pList5.Count; i++)
            {
                kList5.Add(pList5[i] - pList5[i + 1]);
            }

            foreach (var item in kList5)
            {
                if (item < 0)
                {
                    result += 1;
                }
                else {
                    result -= 1;
                }
            }

            for (int i = 0; i < pList15.Count; i++)
            {
                kList15.Add(pList15[i] - pList15[i + 1]);
            }

            foreach (var item in kList15)
            {
                if (item < 0)
                {
                    result += 1;
                }
                else
                {
                    result -= 1;
                }
            }

            for (int i = 0; i < pList30.Count; i++)
            {
                kList30.Add(pList30[i] - pList30[i + 1]);
            }

            foreach (var item in kList30)
            {
                if (item < 0)
                {
                    result += 1;
                }
                else
                {
                    result -= 1;
                }
            }


            //60 120位置和斜率，可加分或减分
            if (pList30[0] < pList60[0])
            {
                result += 1;
                if (pList60[0] - pList60[1] < 0)
                {
                    result += 1;
                }
            }
            else
            {
                result -= 1;
            }

            if (pList30[0] < pList120[0])
            {
                result += 1;
                if (pList120[0] - pList120[1] < 0)
                {
                    result += 1;
                }
            }
            else
            {
                result -= 1;
            }
        }

        return result;
    }

    public float GetResult(MA ma, int count = 3) {
        return LongValue(ma, count) - ShortValue(ma, count);
    }
}