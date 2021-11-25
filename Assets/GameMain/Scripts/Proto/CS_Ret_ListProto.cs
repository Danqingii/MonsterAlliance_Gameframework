//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2021-11-24 15:31:27
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 测试协议
/// </summary>
public struct CS_Ret_ListProto : IProto
{
    public ushort ProtoCode { get { return 14005; } }

    public int ItemCount; //元素数量
    public List<int> ItemIdList; //元素Id
    public List<string> ItemNameList; //元素名字

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteInt(ItemCount);
            for (int i = 0; i < ItemCount; i++)
            {
                ms.WriteInt(ItemIdList[i]);
                ms.WriteUTF8String(ItemNameList[i]);
            }
            return ms.ToArray();
        }
    }

    public static CS_Ret_ListProto GetProto(byte[] buffer)
    {
        CS_Ret_ListProto proto = new CS_Ret_ListProto();
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.ItemCount = ms.ReadInt();
            proto.ItemIdList = new List<int>();
            proto.ItemNameList = new List<string>();
            for (int i = 0; i < proto.ItemCount; i++)
            {
                int _ItemId = ms.ReadInt();  //元素Id
                proto.ItemIdList.Add(_ItemId);
                string _ItemName = ms.ReadUTF8String();  //元素名字
                proto.ItemNameList.Add(_ItemName);
            }
        }
        return proto;
    }
}