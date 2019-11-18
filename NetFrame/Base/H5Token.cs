using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NetFrame.Base
{
    /// <summary>
    /// 兼容 H5 websocket 的token
    /// </summary>
    public class H5Token:BaseToken
    {
        bool ishanded = false;
        bool isH5 = false;

        public override void Read<T>() {
            lock (cache) {
                if (cache == null||cache.Count==0) {
                    isRead = false;
                    return;
                }

                if (!ishanded) {
                    ishanded = true;
                    string msg = Encoding.UTF8.GetString(cache.ToArray());
                    if (msg.Contains("Sec-WebSocket-Key")) {
                        isH5 = true;
                        socket.Send(PackageHandShakeData(cache.ToArray()));
                        cache.Clear();
                        isRead = false;
                    }
                    else {
                        base.Read<T>();
                    }
                    
                }
                else {
                    if (isH5) {
                        byte[] data= AnalyzeClientData(cache.ToArray());
                        cache.Clear();
                        cache.AddRange(data);
                    }
                    base.Read<T>();
                    
                }
            }
        }

        protected override byte[] BeforeSend(byte[] v) {
            if (isH5) {
                return PackageServerData(v);
            }
            return base.BeforeSend(v);
        }

        /// <summary>
        /// 打包服务器握手数据
        /// </summary>
        /// <returns>The hand shake data.</returns>
        /// <param name="handShakeBytes">Hand shake bytes.</param>
        /// <param name="length">Length.</param>
        private byte[] PackageHandShakeData(byte[] handShakeBytes) {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, handShakeBytes.Length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "") {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }

            byte[] secKeyBytes = SHA1.Create().ComputeHash(
                                     Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);

            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");

            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 解析客户端发送来的数据
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="recBytes">Rec bytes.</param>
        /// <param name="length">Length.</param>
        protected byte[] AnalyzeClientData(byte[] recBytes) {
            if (recBytes.Length < 2) {
                return null;
            }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin) {
                return null;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag) {
                return null;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126) {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127) {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++) {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++) {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++) {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return payload_data;
        }


        /// <summary>
        /// 把客户端消息打包处理
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        protected byte[] PackageServerData(byte[] msgBytes) {

            byte[] content = null;

            if (msgBytes.Length < 126) {
                content = new byte[msgBytes.Length + 2];
                content[0] = 0x81;
                content[1] = (byte)msgBytes.Length;
                Array.Copy(msgBytes, 0, content, 2, msgBytes.Length);
            }
            else if (msgBytes.Length < 0xFFFF) {
                content = new byte[msgBytes.Length + 4];
                content[0] = 0x81;
                content[1] = 126;
                content[2] = (byte)(msgBytes.Length >> 8 & 0xFF); 
                content[3] = (byte)(msgBytes.Length & 0xFF);
                Array.Copy(msgBytes, 0, content, 4, msgBytes.Length);
            }
            else {
                // 暂不处理超长内容  
            }

            return content;
        }

        public override void ResetValue() {
            ishanded = false;
            isH5 = false;
            base.ResetValue();
        }
    }


}
