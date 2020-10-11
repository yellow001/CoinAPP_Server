using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class Boll
{

    public static float GetBoll(int length, List<KLine> data, out float upValue, out float lowValue)
    {
        float midValue = MA.GetMA(length, data);

        List<double> mulList = new List<double>();
        for (int i = 0; i < length; i++)
        {
            KLine line = data[i];
            mulList.Add(Math.Pow(line.V_ClosePrice - midValue,2));
        }

        //标准差
        double sd =Math.Sqrt(mulList.Average());


        //上轨
        upValue = midValue + 2 * (float)sd;


        //下轨
        lowValue = midValue - 2 * (float)sd;

        return midValue;
    }
}