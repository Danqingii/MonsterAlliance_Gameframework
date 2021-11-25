using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game;
using LitJson;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        EventDispatcher.Instance.AddEventListener(ProtoCodeDef.SC_Login,OnLoginCallBack);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CS_LoginProto proto = new CS_LoginProto();
            proto.Id = "110";
            proto.Pw = "110";
            GameEntry.TcpNetwork.SendMsg(proto.ToArray());
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            CS_LoginProto proto = new CS_LoginProto();
            proto.Id = "1";
            proto.Pw = "1";
            GameEntry.TcpNetwork.SendMsg(proto.ToArray());
        }
    }

    private void OnLoginCallBack(byte[] buffer)
    {
        SC_LoginProto proto = SC_LoginProto.GetProto(buffer);
        Debug.Log("登陆:"+proto.IsSuccess);
    }
}
