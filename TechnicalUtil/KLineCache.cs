using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

public class KLineCache
{
    public List<KLine> V_KLineData;

    public KLineCache() { }

    public void RefreshData(List<KLine> data) {
        if (V_KLineData == null) {
            V_KLineData = new List<KLine>();
        }
        V_KLineData.Clear();
        if (data != null && data.Count > 1) {
            data.RemoveAt(0);
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
