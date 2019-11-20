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

    public float GetPercent(float curentPrice) {
        if (dir > 0)
        {
            return ((curentPrice - price) / curentPrice) * 100 * mul;
        }
        else {
            return ((price - curentPrice) / curentPrice) * 100 * mul;
        }
    }

    public float GetWin(float curentPrice) {
        return money * GetPercent(curentPrice) * 0.01f;
    }
}
