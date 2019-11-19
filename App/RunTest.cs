using System;
using System.Collections.Generic;
using System.Text;

public class RunTest
{
    public float money;

    public DateTime lastTime;

    public Order order;

    public float stopLossValue = 0.6f;

    public float stopWinValue = 1.2f;

    public int cd = 7200;//秒

    bool init = true;

    public RunTest() { }

    public void Handle(List<KLine> data) {
        KLineCache cahce = new KLineCache();
        cahce.SetData(data);

        MA ma = new MA();
        ma.SetCache(cahce);

        //有单就算下是否需要平仓

        if (init)
        {
            OnHandle();
        }
        else {
            if ((data[0].timestamp - lastTime).Seconds >= cd) {
                OnHandle();
            }
        }
    }

    void OnHandle() {
        //是否有单

        //无

        ////获取结果
        /////大于等于3
        /////多

        /////小于等于-3
        /////空



        //有
        ////获取结果
        /////大于等于3
        /////多

        /////小于等于-3
        /////空


    }

    /// <summary>
    /// 开单
    /// </summary>
    void OpenOrder() { }

    /// <summary>
    /// 平仓
    /// </summary>
    void CloseOrder() {

    }
}
