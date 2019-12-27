using System;
using System.Collections.Generic;
using System.Text;

public class Util
{
    /// <summary>
    /// 秒与ticks的转换常量
    /// </summary>
    public const long Second_Ticks= 10000 * 1000;

    /// <summary>
    /// 分与ticks的转换常量
    /// </summary>
    public const long Minute_Ticks = 60 * Second_Ticks;

    /// <summary>
    /// 时与ticks的转换常量
    /// </summary>
    public const long Hour_Ticks = 60 * Minute_Ticks;

    /// <summary>
    /// 天与ticks的转换常量
    /// </summary>
    public const long Day_Ticks = 24 * Hour_Ticks;

    public static float GetAddListCount(int count) {
        return ((count + 1) * count * 0.5f);
    }

    public static float GetAvg(List<float> list)
    {

        if (list == null || list.Count == 0)
        {
            return 0;
        }

        float temp = 0;
        foreach (var item in list)
        {
            temp += item;
        }
        return temp / list.Count;
    }
}
