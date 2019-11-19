using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

public class KLineCache
{
    public List<KLine> kLineData;

    public KLineCache() { }

    public void SetData(List<KLine> data) {
        if (kLineData == null) {
            kLineData = new List<KLine>();
        }
        kLineData.Clear();
        if (data != null && data.Count > 1) {
            data.RemoveAt(0);
            kLineData.AddRange(data);
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
