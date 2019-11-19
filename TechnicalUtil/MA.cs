using System;
using System.Collections.Generic;
using System.Text;

public class MA
{
    public KLineCache cache;

    public void SetCache(KLineCache c) {
        cache = c;
    }

    /// <summary>
    /// 获取 MA 值
    /// </summary>
    /// <param name="count">周期</param>
    /// <param name="index">下标</param>
    /// <returns></returns>
    public float GetMAValue(int count, int index=0) {
        if (cache == null) {
            return 0;
        }

        if (cache.kLineData.Count < count + index)
        {
            return 0;
        }
        float sum = 0;
        for (int i = 0; i < count; i++)
        {
            sum += cache.kLineData[index + i].closePrice;
        }
        return sum / count;
    }
}