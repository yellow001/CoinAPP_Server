using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Tool
{
    public class TimeEventModel
    {
        /// <summary>
        /// 执行或时间
        /// </summary>
        public long Excute_time;

        /// <summary>
        /// 时间间隔
        /// </summary>
        public long Wait_time;

        public int count;

        public Action de;

        public TimeEventModel() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">执行间隔 单位 秒</param>
        /// <param name="count">执行次数 -1 表示无限循环 此时需要外部移除任务</param>
        /// <param name="de">触发事件</param>
        public TimeEventModel(long time, int count, Action de) {

            this.Wait_time = time * 10000 * 1000;//把秒数转换成 ticks 的单位长度
            this.Excute_time = DateTime.Now.Ticks+Wait_time;
            this.count = count;
            this.de = de;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">执行间隔 单位 秒</param>
        /// <param name="count">执行次数 -1 表示无限循环 此时需要外部移除任务</param>
        /// <param name="de">触发事件</param>
        public TimeEventModel(float time, int count, Action de) {

            this.Wait_time = (long)(time * 10000 * 1000);//把秒数转换成 ticks 的单位长度
            this.Excute_time = DateTime.Now.Ticks + Wait_time;
            this.count = count;
            this.de = de;
        }

        public void InitData()
        {
            Excute_time = DateTime.Now.Ticks + Wait_time;
        }
    }
}
