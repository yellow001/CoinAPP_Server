﻿using NetFrame.EnDecode;
using NetFrame.Tool;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame.Base
{
    [Serializable]
    [ProtoContract]
    public class TransModel
    {
        [ProtoMember(1)]
        public int pID;
        [ProtoMember(2)]
        public int area;
        [ProtoMember(3)]
        public byte[] msgBytes;

        public TransModel() { }

        public TransModel(int pid, int a=-999) {
            pID = pid;
            area = a;
        }

        public void SetMsg<T>(T msg) {
            try {
                if (AbsCoding.Ins != null) {
                    msgBytes = AbsCoding.Ins.MsgEncoding(msg);
                }
            }
            catch (Exception ex) {
                //Console.WriteLine(ex.ToString());
                Debugger.Error(ex.ToString());
            }
        }

        public T GetMsg<T>() {
            try {
                if (AbsCoding.Ins != null&&msgBytes!=null&&msgBytes.Length>0) {
                    return AbsCoding.Ins.MsgDecoding<T>(msgBytes);
                }
                return default(T);
            }
            catch (Exception ex) {
                //Console.WriteLine(ex.ToString());
                Debugger.Error(ex.ToString());
                return default(T);
            }
            
        }
    }
}
