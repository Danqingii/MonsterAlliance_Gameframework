//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2021-11-24 16:16:46
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 服务器返回登陆状态
/// </summary>
public struct SC_LoginProto : IProto
{
    public ushort ProtoCode { get { return 10002; } }

    public bool IsSuccess; //是否成功

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteBool(IsSuccess);
            return ms.ToArray();
        }
    }

    public static SC_LoginProto GetProto(byte[] buffer)
    {
        SC_LoginProto proto = new SC_LoginProto();
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.IsSuccess = ms.ReadBool();
        }
        return proto;
    }
}