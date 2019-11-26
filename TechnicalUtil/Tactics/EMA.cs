using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EMA
{
    public static float GetEMA(int N, List<float> list)
    {

        return GetEMA(N, list, N - 1);
    }

    public static float GetEMA(int N, List<KLine> list)
    {
        if (N <= 0) { return 0; }

        if (list == null) { return 0; }

        if (list.Count < N) { return 0; }

        List<float> temp = list.Select(q => q.V_ClosePrice).ToList();

        return GetEMA(N, temp);
    }

    public static float GetEMA(int N, List<float> list, int finalIndex)
    {
        if (N <= 0 || finalIndex < 0) { return 0; }

        if (list == null) { return 0; }

        if (list.Count < N) { return 0; }
        if (list.Count < finalIndex) { return 0; }

        if (1 == N)
        {
            return list[finalIndex];
        }
        else
        {
            return (2 * list[finalIndex + 1 - N] + (N - 1) * GetEMA(N - 1, list, finalIndex)) / (N + 1);
        }
    }
}