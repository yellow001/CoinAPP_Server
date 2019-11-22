using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

public class KLineCache
{
    public List<KLine> V_KLineData;

    public KLineCache() { }

    public void SetData(List<KLine> data) {
        if (V_KLineData == null) {
            V_KLineData = new List<KLine>();
        }
        V_KLineData.Clear();
        if (data != null && data.Count > 1) {
            data.RemoveAt(0);
            V_KLineData.AddRange(data);
        }
    }

    public void SetData(JContainer jcontainer)
    {
        List<KLine> list = KLine.GetListFormJContainer(jcontainer);
        SetData(list);
    }

    public void FilterData() {

    }
}
