using System;
using System.Collections.Generic;
using System.Text;

public class Order
{
    public int dir;

    public float money;

    public float price;

    public float mul;

    public DateTime time;

    public Order() { }

    public Order(int d,float mon,float p,float m,DateTime t) {
        dir = d;
        money = mon;
        price = p;
        mul = m;
        time = t;
    }

    public float GetPercent(float hPrice,float lPrice) {
        if (dir > 0)
        {
            if (lPrice > price)
            {
                return ((hPrice - price) / hPrice) * 100 * mul;
            }
            else {
                return ((lPrice - price) / lPrice) * 100 * mul;
            }
            
        }
        else {

            if (hPrice < price)
            {
                return ((price - lPrice) / lPrice) * 100 * mul;
            }
            else {
                return ((price - hPrice) / hPrice) * 100 * mul;
            }
        }
    }

    public float GetWin(float hPrice,float lPrice) {

        float p = GetPercent(hPrice, lPrice);

        return money * p * 0.01f;
    }
}
