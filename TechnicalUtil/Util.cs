using System;
using System.Collections.Generic;
using System.Text;

public class Util
{
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
