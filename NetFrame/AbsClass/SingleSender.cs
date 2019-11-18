using NetFrame.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.AbsClass
{
    public class SingleSender
    {

        /// <summary>
        /// 向单个token发送数据
        /// </summary>
        /// <param name="token">连接</param>
        /// <param name="pid">协议号</param>
        /// <param name="area">区域码</param>
        public void Send(BaseToken token, int pid, int area) {
            TransModel model = new TransModel(pid, area);
            Send(token, model);
        }

        /// <summary>
        /// 向单个token发送数据
        /// </summary>
        /// <param name="token">连接</param>
        /// <param name="pid">协议号</param>
        /// <param name="area">区域码</param>
        /// <param name="message">消息体</param>
        public void Send<T>(BaseToken token, int pid, int area, T message) {
            TransModel model = new TransModel(pid, area);
            model.SetMsg(message);
            Send(token,model);
        }

        /// <summary>
        /// 向单个token发送数据
        /// </summary>
        /// <param name="token">连接</param>
        /// <param name="model">传输模型</param>
        public void Send<T>(BaseToken token, T model)where T:TransModel {
            token.Send(model);
        }
    }
}
