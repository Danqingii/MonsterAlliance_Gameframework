using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LoginDBModel
{
    private static LoginDBModel instance;
    public static LoginDBModel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LoginDBModel();
            }
            return instance;
        }
    }

    public void Init()
    {
        EventDispatcher.Instance.AddEventListener(ProtoCodeDef.CS_Login, OnLoginCallBack);
    }

    private void OnLoginCallBack(Role role, byte[] buffer)
    {
        CS_LoginProto proto = CS_LoginProto.GetProto(buffer);

        bool isSucceed = false;
        if(proto.Pw == "1" && proto.Id == "1")
        {
            isSucceed = true;
        }

        Log.Debug($"客户端请求登陆:{isSucceed}");


        //返回是否可以登陆 协议
        SC_LoginProto backProto = new SC_LoginProto();
        backProto.IsSuccess = isSucceed;

        role.ClientSocket.SendMsg(backProto.ToArray());
    }
}