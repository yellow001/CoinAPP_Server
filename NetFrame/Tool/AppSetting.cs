using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetFrame.Tool
{
    [Serializable]
    public sealed class AppSetting
    {
        public Dictionary<string, string> Settings=new Dictionary<string, string>();

        public static AppSetting Ins {
            get {
                if (ins == null) {
                    ins = new AppSetting();
                }

                return ins;
            }
        }

        static AppSetting ins;

        [NonSerialized]
        public static string settingPath = AppDomain.CurrentDomain.BaseDirectory+"/"+ "AppSetting.txt";
        

        private AppSetting() {
            InitSetting();
        }


        public string GetValue(string name) {
            if (Ins.Settings.ContainsKey(name)) {
                return Ins.Settings[name];
            }
            else {

                return string.Empty;
            }
        }

        public int GetInt(string name)
        {
            int result = 0;
            if (Ins.Settings.ContainsKey(name))
            {
                int.TryParse(Ins.Settings[name],out result);
            }

            return result;
        }

        public float GetFloat(string name)
        {
            float result = 0;
            if (Ins.Settings.ContainsKey(name))
            {
                float.TryParse(Ins.Settings[name], out result);
            }

            return result;
        }

        void InitSetting() {
            try {
                using (StreamReader sr = new StreamReader(settingPath)) {
                    string content = sr.ReadToEnd();
                    string[] settings = content.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in settings) {
                        if (item.StartsWith("//")) { continue; }

                        string[] item2 = item.Split(':',2);
                        if (item2.Length == 2) {
                            Settings.Add(item2[0], item2[1]);
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
    }
    
}
