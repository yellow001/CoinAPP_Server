using NetFrame.Tool;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Text;

public class CommonData
{
    public AccountAPIKey V_ApiKey;

    public SwapApi V_SwapApi;

    public SpotApi V_SpotApi;

    public InformationApi V_InformationApi;

    public OKExV5APi V5pApi;


    public float LongRatio = 2;
    public float LongMaxRatio = 2;
    public float ShortRatio = 0;
    public float ShortMaxRatio = 0;

    static CommonData ins;

    public static CommonData Ins {
        get {
            if (ins == null)
            {
                ins = new CommonData();

                string[] keys = AppSetting.Ins.GetValue("api").Split(';');

                ins.V_ApiKey = new AccountAPIKey(keys);

                ins.V_SwapApi = new SwapApi(ins.V_ApiKey);

                ins.V_SpotApi = new SpotApi(ins.V_ApiKey);

                ins.V_InformationApi = new InformationApi(ins.V_ApiKey);

                ins.V5pApi = new OKExV5APi(ins.V_ApiKey);

                ins.LongRatio = AppSetting.Ins.GetFloat("LongRatio");
                ins.LongMaxRatio = AppSetting.Ins.GetFloat("LongMaxRatio");
                ins.ShortRatio = AppSetting.Ins.GetFloat("ShortRatio");
                ins.ShortMaxRatio = AppSetting.Ins.GetFloat("ShortMaxRatio");
            }
            return ins;
        }
    }
}
