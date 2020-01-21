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

    public void FilterData() {

    }
}
