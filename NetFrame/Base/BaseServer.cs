using NetFrame.AbsClass;
using NetFrame.EnDecode;
using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetFrame.Base
{
    public class BaseServer<T1,T2>where T1:TransModel where T2:BaseToken
    {
        /// <summary>
        /// 监听端口
        /// </summary>
        public int port;

        /// <summary>
        /// 本地socket
        /// </summary>
        Socket socket;

        /// <summary>
        /// 连接池对象
        /// </summary>
        protected ObjPool<BaseToken> tokens;

        /// <summary>
        /// 最大连接数
        /// </summary>
        protected int maxConn;

        /// <summary>
        /// 连接信号量
        /// </summary>
        Semaphore maxConn_se;

        /// <summary>
        /// 最大挂起连接数
        /// </summary>
        int waitConn;

        AbsHandlerCenter center;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">端口</param>
        /// <param name="mConn"></param>
        /// <param name="wConn"></param>
        public BaseServer(int p,int mConn=1000,int wConn=10) {
            port = p;
            maxConn = mConn;
            waitConn = wConn;

            maxConn_se = new Semaphore(maxConn, maxConn);
            
        }

        /// <summary>
        /// 初始化token池，BaseServer的子类可以重载该方法以填充 BaseToken 的子类
        /// </summary>
        /// <param name="c"></param>
        /// <param name="coding"></param>
        public virtual void Init(AbsHandlerCenter c) {
            //Console.WriteLine("绑定消息处理中心");
            Debugger.Log("绑定消息处理中心");
            center = c;

            //Console.WriteLine("初始化token池");
            Debugger.Log("初始化token池");
            tokens = new ObjPool<BaseToken>(maxConn);
            for (int i = 0; i < maxConn; i++) {
                T2 t = Activator.CreateInstance<T2>();
                t.Init(c);
                t.CloseDe = ClientClose;
                tokens.Push(t);
            }
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        public void Start() {

            //Console.WriteLine("开启服务");
            Debugger.Log("开启服务");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            socket.Listen(100);
            Accept();
        }

       

        /// <summary>
        /// 监听任务
        /// </summary>
        async void Accept() {//async 与 await 的组合是简写 TAP 模式的语法

            //Console.WriteLine("正在监听");
            Debugger.Log("开启新监听");

            //取出token 并循坏监听消息
            maxConn_se.WaitOne();

            //编译器遇到await时，等待异步操作（await语句后面的方法会直到有连接进来才会执行），并把控制流程退回到调用此方法处执行
            Socket s =await socket.AcceptAsync();
            
            BaseToken t = tokens.Pop();
            t.socket = s;
            center.OnClientConnent(t);

            t.ReceiveAsync<T1>();
            //Receive(t);

            //递归
            Accept();
        }

        [Obsolete("接收任务不应该在server中进行，请调用对应token的ReceiveAsync()方法")]
        /// <summary>
        /// 接收任务
        /// </summary>
        async void Receive(BaseToken t) {

            try {
                byte[] buff = new byte[1024];
                int msg = await t.socket.ReceiveAsync(new ArraySegment<byte>(buff), SocketFlags.None);

                if (msg == 0) {
                    t.CloseDe(t, "客户端断开连接");
                    return;
                }

                byte[] value=new byte[msg];
                Buffer.BlockCopy(buff, 0, value, 0, msg);
                t.Receive<T1>(value);

                //递归
                Receive(t);
            }
            catch (Exception ex) {
                //Console.WriteLine(ex.ToString());
                Debugger.Error(ex.ToString());
                t.CloseDe(t, "客户端断开连接");
            }
            
        }


        /// <summary>
        /// 连接关闭
        /// </summary>
        /// <param name="token"></param>
        /// <param name="error"></param>
        protected void ClientClose(BaseToken token,string error) {
            if (token != null) {
                try
                {
                    lock (token)
                    {
                        center.OnClientClose(token, error);
                        token.Close();
                        tokens.Push(token);
                        maxConn_se.Release();
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Error(ex.ToString());

                }
            }
            
            maxConn_se.Release();
        }
    }
}
