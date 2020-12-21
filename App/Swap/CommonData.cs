using NetFrame.Tool;
using OKExSDK;
using System;
using System.Collections.Generic;
using System.Text;

public class CommonData
{
    public AccountAPIKey V_ApiKey;

    public SwapApi V_SwapApi;

    public InformationApi V_InformationApi;


    static CommonData ins;

    public static CommonData Ins {
        get {
            if (ins == null)
            {
                ins = new CommonData();

                string[] keys = AppSetting.Ins.GetValue("api").Split(';');

                ins.V_ApiKey = new AccountAPIKey(keys);

                ins.V_SwapApi = new SwapApi(ins.V_ApiKey);

                ins.V_InformationApi = new InformationApi(ins.V_ApiKey);
            }
            return ins;
        }
    }
}
