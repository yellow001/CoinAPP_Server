using System;
using System.Collections.Generic;
using System.Text;

public class Order
{
    public int dir;

    public float price;

    public DateTime time;

    public Order() { }

    public Order(int d,float p,DateTime t) {
        dir = d;
        price = p;
        time = t;
    }
}
