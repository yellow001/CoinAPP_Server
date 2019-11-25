using System;
using System.Collections.Generic;
using System.Text;

public class MA
{
    public static float GetMA(int length, List<float> data) {

        if (data.Count < 0) {
            return 0;
        }

        float sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += data[i];
        }
        return sum / length;
    }

    public static float GetMA(int length, List<KLine> data)
    {
        if (data == null|| data.Count < 0)
        {
            return 0;
        }

        float sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += data[i].V_ClosePrice;
        }
        return sum / length;
    }
}
