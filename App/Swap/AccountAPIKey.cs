using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 用户api
/// </summary>
public class AccountAPIKey
{
    public string V_KeyName {
        get;
    }

    public string V_ApiKey {
        get;
    }

    public string V_Passphrase {
        get;
    }

    public string V_SecretKey {
        get;
    }


    public AccountAPIKey(string n,string passphrase,string apiKey,string secreKey) {
        V_KeyName = n;
        V_Passphrase = passphrase;
        V_ApiKey = apiKey;
        V_SecretKey = secreKey;
    }

    public AccountAPIKey(string[] paramsStr)
    {
        if (paramsStr!=null&&paramsStr.Length >= 4) {
            V_KeyName = paramsStr[0];
            V_Passphrase = paramsStr[1];
            V_ApiKey = paramsStr[2];
            V_SecretKey = paramsStr[3];
        }
    }

    public AccountAPIKey(List<string> paramsStr)
    {
        if (paramsStr != null && paramsStr.Count >= 4)
        {
            V_KeyName = paramsStr[0];
            V_Passphrase = paramsStr[1];
            V_ApiKey = paramsStr[2];
            V_SecretKey = paramsStr[3];
        }
    }
}
