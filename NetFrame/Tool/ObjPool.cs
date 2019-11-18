using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Tool
{
    public class ObjPool<T>
    {
        public Stack<T> pool;


        public ObjPool(int Max) {
            pool = new Stack<T>(Max);
        }

        /// <summary>
        /// 把对象压栈
        /// </summary>
        /// <param name="item">Item.</param>
        public void Push(T item) {
            pool.Push(item);
        }

        /// <summary>
        /// 对象出栈
        /// </summary>
        public T Pop() {
            return pool.Pop();
        }


        /// <summary>
        /// 获取栈中对象个数
        /// </summary>
        /// <value>The count.</value>
        public int Count {

            get {
                return pool.Count;
            }
        }

        public void Clear() {
            pool.Clear();
        }
    }
}
