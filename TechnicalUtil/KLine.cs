using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class KLine
{
    public DateTime timestamp;
    public float openPrice;
    public float hightPrice;
    public float lowPrice;
    public float closePrice;
    public float vol;

    public KLine() { }

    public void SetData(DateTime d, float oPrice, float hPrice, float lPrice, float cPrice, float v) {
        timestamp = d;
        openPrice = oPrice;
        hightPrice = hPrice;
        lowPrice = lPrice;
        closePrice = cPrice;
        vol = v;
    }

    public void SetData(string d, string oPrice, string hPrice, string lPrice, string cPrice, string v) {
        DateTime date = DateTime.Parse(d);
        SetData(date, float.Parse(oPrice), float.Parse(hPrice), float.Parse(lPrice), float.Parse(cPrice), float.Parse(v));
    }

    public void SetData(List<string> content) {
        if (content != null && content.Count >= 6) {
            SetData(content[0], content[1], content[2], content[3], content[4], content[5]);
        }
    }

    public static List<KLine> GetListFormJContainer(JContainer jcontainer)
    {
        List<KLine> result = new List<KLine>();
        JToken temp = jcontainer.First;
        while (temp!=null)
        {
            List<string> content = temp.ToObject<List<string>>();
            KLine line = new KLine();
            line.SetData(content);
            result.Add(line);
            temp = temp.Next;
        }
        return result;
    }
}