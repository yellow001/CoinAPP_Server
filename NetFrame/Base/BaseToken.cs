using NetFrame.AbsClass;
using NetFrame.EnDecode;
using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetFrame.Base
{
    //public delegate void CloseToken<T>(BaseToken<TransModel> token, string error);

    /// <summary>
    /// 自定义连接对象
    /// </summary>
    public class BaseToken
    {
        /// <summary>
        /// 对应的套接字
        /// </summary>
        public Socket socket;

        /// <summary>
        /// 消息处理中心
        /// </summary>
        public AbsHandlerCenter center;

        /// <summary>
        /// 关闭连接委托
        /// </summary>
        public Action<BaseToken,string> CloseDe;

        /// <summary>
        /// 消息缓存
        /// </summary>
        public List<byte> cache=new List<byte>();

        protected bool isRead=false;

        /// <summary>
        /// 发送队列
        /// </summary>
        public Queue<TransModel> wqueue = new Queue<TransModel>();

        bool isWrite = false;

        bool shutdown = false;

        public BaseToken() { }

        public void Init(AbsHandlerCenter c) {
            center = c;
        }

        public async void ReceiveAsync<T>()where T:TransModel {
            try {
                byte[] buff = new byte[1024];
                int msg = await socket.ReceiveAsync(new ArraySegment<byte>(buff), SocketFlags.None);

                if (msg == 0) {
                    CloseDe(this, "客户端断开连接");
                    return;
                }

                byte[] value = new byte[msg];
                Buffer.BlockCopy(buff, 0, value, 0, msg);
                Receive<T>(value);

                //递归
                ReceiveAsync<T>();
            }
            catch (Exception ex) {
                //Console.WriteLine(ex.ToString());
                Debugger.Error(ex.ToString());
                CloseDe(this, "客户端断开连接");
            }
        }

        public void Receive<T>(byte[] value) where T:TransModel{
            cache.AddRange(value);
            if (!isRead) {
                isRead = true;
                Read<T>();
            }

        }

        /// <summary>
        /// 读取消息(子类可以重载该方法用来进行消息验证或H5的握手处理)
        /// </summary>
        public virtual void Read<T>()where T:TransModel {
            try {
                T model = AbsCoding.Ins.ModelDecoding<T>(ref cache);
                if (model == null) {
                    isRead = false;
                    return;
                }

                center.OnMsgReceive<T>(this, model);

                Read<T>();
            }
            catch (Exception ex) {
                isRead = false;
                shutdown = true;
                CloseDe(this, "解码出错");
                return;
            }

            
        }

        /// <summary>
        /// 发送传输模型
        /// </summary>
        /// <param name="model"></param>
        public void Send<T>(T model)where T:TransModel{
            wqueue.Enqueue(model);
            if (!isWrite) {
                isWrite = true;
                Write<T>();
            }
        }

        /// <summary>
        /// 向对应socket发送消息
        /// </summary>
        public async void Write<T>() where T:TransModel{
            if (wqueue.Count <= 0||shutdown) {
                isWrite = false;
                return;
            }

            T model = (T)wqueue.Dequeue();

            byte[] b_value = AbsCoding.Ins.ModelEncoding(model);
            byte[] value = BeforeSend(b_value);
            if (value == null) {
                isWrite = false;
                return;
            }

            try {
                ArraySegment<byte> buff = new ArraySegment<byte>(value);
                //等待发送消息
                await socket.SendAsync(buff, SocketFlags.None);

                //完成后循环发送消息
                Write<T>();
            }
            catch (SocketException ex) {
                isWrite = false;
                shutdown = true;
                CloseDe(this, "发送消息失败，断开连接");
                return;
            }
            

        }

        /// <summary>
        /// 发送数据前对该数据的修改（目前用于 H5 打包消息）
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected virtual byte[] BeforeSend(byte[] v) {
            return v;
        }

        public void Close() {
            try {
                ResetValue();
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
                socket = null;
            }
            // throws if client process has already closed
            catch (Exception ex) {
                //Console.WriteLine(ex.ToString());
                Debugger.Warn(ex.ToString());
            }
        }
        

        // <summary>
        /// 重置token的值，子类应该继承
        /// </summary>
        public virtual void ResetValue() {
            shutdown = false;
            isWrite = false;
            isRead = false;
            cache.Clear();
            wqueue.Clear();
        }
    }
}
