using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame.AbsClass
{
    /// <summary>
    /// 广播类，用于同时向多个连接发送消息
    /// </summary>
    public class BroadcastSender:SingleSender
    {
        public void Broadcast(List<BaseToken> tokens, int pid, int area) {
            Parallel.ForEach(tokens, (item) => {
                Send(item, pid, area);
            });
        }
        
        public void Broadcast<T>(List<BaseToken> tokens, int pid, int area, T message) {
            Parallel.ForEach(tokens, (item) => {
                Send(item, pid, area,message);
            });
        }

        /// <summary>
        /// 向单个token发送数据
        /// </summary>
        /// <param name="token">连接</param>
        /// <param name="model">传输模型</param>
        public void Broadcast<T>(List<BaseToken> tokens, T model) where T : TransModel {
            Parallel.ForEach(tokens, (item) => {
                Send(item, model);
            });
        }
    }
}
