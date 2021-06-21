using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoinAPP_Server.App
{
    public enum MatchItemType
    {
        Swap = 1,
        Spot = 2,
    }

    public enum MatchItemActionType { 
        DoLong=1,
        DoShort=2,
        CloseLong=3,
        CLoseShort=4,
    }

    public enum MatchConditionType { 
        MA = 1,
        EMA = 2,
        BtcLSPercent=3,
        PriceList = 4,
        OldPrice=5,
    }

    public class MatchConditionItem {
        public MatchConditionType type;
        public float args1;
        public List<float> paramsList1=new List<float>();
        public List<float> paramsList2=new List<float>();

        public MatchConditionItem(string str) {
            if (!string.IsNullOrEmpty(str))
            {
                string[] list = str.Split('#');

                type = (MatchConditionType)Enum.Parse(typeof(MatchConditionType), list[0]);

                if (list.Length>1)
                {
                    string[] list2 = list[1].Split('|');

                    if (list2.Length>0)
                    {
                        args1 = float.Parse(list2[0]);
                    }

                    if (list2.Length > 1)
                    {
                        string[] data1 = list2[1].Split('_');
                        for (int i = 0; i < data1.Length; i++)
                        {
                            paramsList1.Add(float.Parse(data1[i]));
                        }
                    }

                    if (list2.Length > 2)
                    {
                        string[] data2 = list2[2].Split('_');
                        for (int i = 0; i < data2.Length; i++)
                        {
                            paramsList2.Add(float.Parse(data2[i]));
                        }
                    }
                }
            }
        }

        public bool IsMatch(Dictionary<int, KLineCache> klineDataDic, float btcLSPercent, List<int> MACycleList)
        {

            switch (type)
            {
                case MatchConditionType.MA:

                    int value = (int)args1;

                    if (klineDataDic.ContainsKey(value))
                    {
                        KLineCache kLineCache = klineDataDic[value];

                        List<float> maList = new List<float>();

                        List<float> perList = new List<float>();

                        for (int i = 0; i < paramsList1.Count; i++)
                        {

                            int maIndex = (int)paramsList1[i];

                            float maValue = 0;

                            if (maIndex == 0)
                            {
                                maValue = kLineCache.V_KLineData[0].V_ClosePrice;
                            }
                            else {
                                maValue = MA.GetMA(MACycleList[maIndex-1], kLineCache.V_KLineData);
                            }
                            

                            maList.Add(maValue);


                            float perValue =MathF.Abs((kLineCache.V_KLineData[0].V_ClosePrice - maValue) / maValue * 100);

                            perList.Add(perValue);

                        }

                        bool match = true;
                        for (int i = 0; i < maList.Count -1; i++)
                        {
                            float perValue = MathF.Abs((maList[i] - maList[i + 1]) / maList[i + 1] * 100);

                            if (perValue>1&&maList[i]<maList[i+1])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            for (int i = 0; i < paramsList2.Count; i++)
                            {
                                if (paramsList2[i] < 0)
                                {
                                    if (perList[i]< MathF.Abs(paramsList2[i]))
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                else {
                                    if (perList[i]> paramsList2[i])
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                            }
                        }

                        return match;
                    }

                    break;
                case MatchConditionType.EMA:

                    value = (int)args1;

                    if (klineDataDic.ContainsKey(value))
                    {
                        KLineCache kLineCache = klineDataDic[value];

                        List<float> maList = new List<float>();

                        List<float> perList = new List<float>();

                        for (int i = 0; i < paramsList1.Count; i++)
                        {
                            int maIndex = (int)paramsList1[i];

                            float maValue = 0;

                            if (maIndex == 0)
                            {
                                maValue = kLineCache.V_KLineData[0].V_ClosePrice;
                            }
                            else
                            {
                                maValue = EMA.GetEMA(MACycleList[maIndex - 1], kLineCache.V_KLineData);
                            }

                            maList.Add(maValue);


                            float perValue = MathF.Abs((kLineCache.V_KLineData[0].V_ClosePrice - maValue) / maValue * 100);

                            perList.Add(perValue);

                        }

                        bool match = true;
                        for (int i = 0; i < maList.Count - 1; i++)
                        {
                            if (maList[i] < maList[i + 1])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            for (int i = 0; i < paramsList2.Count; i++)
                            {
                                if (paramsList2[i] < 0)
                                {
                                    if (perList[i] < MathF.Abs(paramsList2[i]))
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (perList[i] > paramsList2[i])
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                            }
                        }

                        return match;
                    }
                    break;
                case MatchConditionType.BtcLSPercent:

                    if (args1 < 0)
                    {
                        if (btcLSPercent < MathF.Abs(args1))
                        {
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        if (btcLSPercent > args1)
                        {
                            return false;
                        }

                        return true;
                    }

                    break;
                case MatchConditionType.PriceList:

                    value = (int)args1;

                    if (klineDataDic.ContainsKey(value))
                    {
                        KLineCache kLineCache = klineDataDic[value];

                        bool match = true;

                        int count = (int)paramsList1[0];

                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                if (kLineCache.V_KLineData[i].GetAvg() < kLineCache.V_KLineData[i + 1].GetAvg()) {
                                    match = false;
                                    break;
                                }
                            }
                        }
                        else {
                            for (int i = 0; i < -count; i++)
                            {
                                if (kLineCache.V_KLineData[i].GetAvg() > kLineCache.V_KLineData[i + 1].GetAvg())
                                {
                                    match = false;
                                    break;
                                }
                            }
                        }
                        return match;
                    }
                    break;
                case MatchConditionType.OldPrice:

                    value = (int)args1;

                    if (klineDataDic.ContainsKey(value))
                    {
                        KLineCache kLineCache = klineDataDic[value];

                        bool match = false;

                        int startInedx = (int)paramsList1[0];
                        int dir = (int)paramsList1[1];
                        int count = Math.Abs(dir);

                        float curValue = kLineCache.V_KLineData[0].V_ClosePrice;

                        if (startInedx <= kLineCache.V_KLineData.Count)
                        {
                            float p1 = kLineCache.V_KLineData[startInedx].GetAvg();
                            float p2 = kLineCache.V_KLineData[startInedx - count].GetAvg();

                            float percent = MathF.Abs((p1 - p2) / p2 * 100);

                            if (percent > 2)
                            {
                                if (dir > 0)
                                {
                                    if (p1 < p2 && curValue < p1)
                                    {
                                        match = true;
                                    }
                                }
                                else
                                {
                                    if (p1 > p2 && curValue > p1)
                                    {
                                        match = true;
                                    }
                                }
                            }
                        }
                        return match;
                    }
                    break;
                default:
                    break;
            }

            return false;
        }
    }

    public class MatchItem
    {
        public int id;

        public MatchItemType type;

        public MatchItemActionType actionType; 

        public List<MatchConditionItem> matchConditions = new List<MatchConditionItem>();

        public float matchValue;

        public MatchItem(string str) {
            if (!string.IsNullOrEmpty(str))
            {
                string[] list = str.Split(',');

                id = int.Parse(list[0]);

                type = (MatchItemType)Enum.Parse(typeof(MatchItemType), list[1]);

                actionType = (MatchItemActionType)Enum.Parse(typeof(MatchItemActionType), list[2]);

                string[] conditionStr = list[3].Split(';');

                for (int i = 0; i < conditionStr.Length; i++)
                {
                    MatchConditionItem item = new MatchConditionItem(conditionStr[i]);
                    matchConditions.Add(item);
                }

                matchValue = float.Parse(list[4]);
            }
        }

        public float IsMatch(Dictionary<int,KLineCache> klineDataDic,float btcLSPercent, List<int> MaCycleList) {

            for (int i = 0; i < matchConditions.Count; i++)
            {
                if (!matchConditions[i].IsMatch(klineDataDic,btcLSPercent,MaCycleList))
                {
                    return 0;
                }
            }

            return matchValue;
        }
    }
}
