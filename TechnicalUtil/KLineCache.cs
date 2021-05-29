using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

[Serializable]
[ProtoContract]
public class KLineCache
{
    [ProtoMember(1)]
    public List<KLine> V_KLineData;

    public KLineCache() { }

    public void RefreshData(List<KLine> data) {
        if (V_KLineData == null) {
            V_KLineData = new List<KLine>();
        }
        V_KLineData.Clear();
        if (data != null) {
            V_KLineData.AddRange(data);
        }
    }

    public void RefreshData(JContainer jcontainer)
    {
        List<KLine> list = KLine.GetListFormJContainer(jcontainer);
        RefreshData(list);
    }

    public void RefreshAData(JContainer jcontainer)
    {
        List<KLine> list = KLine.GetAListFormJContainer(jcontainer);
        RefreshData(list);
    }

    public List<KLine> GetMergeKLine(int scale)
    {

        List<KLine> result = new List<KLine>();

        bool refresh = false;

        KLine data = new KLine();

        KLine oldData = new KLine();

        for (int i = 0; i < V_KLineData.Count; i ++)
        {
            oldData = V_KLineData[i];

            if (i % scale == 0)
            {
                refresh = true;
            }
            else
            {
                refresh = false;
            }

            if (refresh)
            {
                data = new KLine();
                data.V_LowPrice = oldData.V_LowPrice;

                data.V_ClosePrice = oldData.V_ClosePrice;
                data.V_Timestamp = oldData.V_Timestamp;
            }

            data.V_Vol += oldData.V_Vol;

            if (data.V_LowPrice > oldData.V_LowPrice)
            {
                data.V_LowPrice = oldData.V_LowPrice;
            }

            if (data.V_HightPrice < oldData.V_HightPrice)
            {
                data.V_HightPrice = oldData.V_HightPrice;
            }

            if (i % scale == scale - 1 || i == V_KLineData.Count - 1)
            {
                data.V_OpenPrice = oldData.V_OpenPrice;
                result.Add(data);
            }
        }

        return result;
    }

    public void FilterData() {

    }
}
