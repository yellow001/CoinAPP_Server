using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetFrame.Tool
{
    public class TimeEventHandler
    {
        /// <summary>
        /// 计时器
        /// </summary>
        Timer timer;

        /// <summary>
        /// 进程同步锁
        /// </summary>
        Mutex mutexLock;

        static TimeEventHandler ins;

        public static TimeEventHandler Ins {
            get {
                if (ins == null) {
                    ins = new TimeEventHandler();
                }
                return ins;
            }
        }

        public TimeEventHandler() {

            mutexLock = new Mutex();

            //初始化计时器（执行间隔为14毫秒）
            timer = new Timer(CallBack, null, 0, 14);

        }


        List<TimeEventModel> models = new List<TimeEventModel>();

        List<TimeEventModel> addList = new List<TimeEventModel>();

        List<TimeEventModel> removeList = new List<TimeEventModel>();


        void CallBack(object sender) {


            lock (models) {

                mutexLock.WaitOne();

                foreach (var item in removeList)
                {
                    if (models.Contains(item)) {
                        models.Remove(item);
                    }
                }
                removeList.Clear();

                foreach (var item in addList)
                {
                    if (!models.Contains(item)) {
                        models.Add(item);
                    }
                }
                addList.Clear();


                #region for 先不用这个
                //for (int i = 0; i < models.Count; i++) {

                //    if (DateTime.Now.Ticks >= models[i].Excute_time) {
                //        //如果委托不为空 执行
                //        models[i].de?.Invoke();

                //        //执行次数大于0，减一并更新下一次执行时间
                //        if (models[i].count > 0) {
                //            models[i].count--;

                //            if (models[i].count == 0) {
                //                models.Remove(models[i]);
                //            }
                //            else {
                //                models[i].Excute_time = DateTime.Now.Ticks + models[i].Wait_time;
                //            }
                //        }
                //        else {
                //            //执行次数小于0表示无限执行，更新下次执行时间
                //            models[i].Excute_time = DateTime.Now.Ticks + models[i].Wait_time;
                //        }

                //    }

                //}
                #endregion

                //用并行看看行不行
                Parallel.For(0, models.Count, (index) => {

                    if (index > models.Count) {
                        return;
                    }

                    if (DateTime.Now.Ticks >= models[index].Excute_time) {
                        //如果委托不为空 执行
                        models[index].de?.Invoke();

                        //执行次数大于0，减一并更新下一次执行时间
                        if (models[index].count > 0) {
                            models[index].count--;

                            if (models[index].count == 0) {
                                models.Remove(models[index]);
                            }
                            else {
                                models[index].Excute_time = DateTime.Now.Ticks + models[index].Wait_time;
                            }
                        }
                        else {
                            //执行次数小于0表示无限执行，更新下次执行时间
                            models[index].Excute_time = DateTime.Now.Ticks + models[index].Wait_time;
                        }

                    }
                });

                mutexLock.ReleaseMutex();
            }
        }

        public void AddEvent(TimeEventModel model) {
            addList.Add(model);
        }

        public void RemoveEvent(TimeEventModel model) {
            removeList.Add(model);
        }
    }
}
