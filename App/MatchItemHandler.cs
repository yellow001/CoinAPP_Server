using NetFrame.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CoinAPP_Server.App
{
    public class MatchItemHandler
    {
        Dictionary<MatchItemActionType, List<MatchItem>> matchListDic = new Dictionary<MatchItemActionType, List<MatchItem>>();

        public MatchItemHandler() {
            InitSetting();
        }

        public static MatchItemHandler Ins {
            get {
                if (ins == null)
                {
                    if (AppSetting.Ins.GetInt("Mode") == 1)
                    {
                        settingPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "MatchListA.txt";
                    }
                    ins = new MatchItemHandler();
                }

                return ins;
            }
        }

        static MatchItemHandler ins;

        public static string settingPath = AppDomain.CurrentDomain.BaseDirectory + "/" + "MatchList.txt";

        void InitSetting()
        {
            try
            {
                using (StreamReader sr = new StreamReader(settingPath))
                {
                    string content = sr.ReadToEnd();
                    string[] settings = content.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in settings)
                    {
                        if (item.StartsWith("//")) { continue; }

                        MatchItem matchItem = new MatchItem(item);

                        if (!matchListDic.ContainsKey(matchItem.actionType))
                        {
                            matchListDic.Add(matchItem.actionType, new List<MatchItem>());
                        }

                        matchListDic[matchItem.actionType].Add(matchItem);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public float GetMatchValue(MatchItemType type, MatchItemActionType actionType,Dictionary<int, KLineCache> klineDataDic, float btcLSPercent,List<int> MACycleList, ref List<int> matchIDList) {
            float result = 0;
            if (matchListDic.ContainsKey(actionType))
            {
                List<MatchItem> list = matchListDic[actionType];

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].type == type)
                    {
                        float matchValue = list[i].IsMatch(klineDataDic, btcLSPercent,MACycleList);

                        if (matchValue > 0)
                        {
                            result += matchValue;
                            matchIDList.Add(list[i].id);
                            //Debugger.Log(list[i].id.ToString());
                        }
                    }
                }
            }

            return result;
        }
    }
}
