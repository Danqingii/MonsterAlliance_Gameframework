//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2021-11-24 15:31:27
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 服务器返回配置列表
/// </summary>
public struct System_GameServerConfigReturnProto : IProto
{
    public ushort ProtoCode { get { return 14003; } }

    public int ConfigCount; //配置数量
    public List<ConfigItem> ServerConfigList; //配置项

    /// <summary>
    /// 配置项
    /// </summary>
    public struct ConfigItem
    {
        public string ConfigCode; //配置编码
        public bool IsOpen; //是否开启
        public string Param; //参数
    }

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteInt(ConfigCount);
            for (int i = 0; i < ConfigCount; i++)
            {
                ms.WriteUTF8String(ServerConfigList[i].ConfigCode);
                ms.WriteBool(ServerConfigList[i].IsOpen);
                ms.WriteUTF8String(ServerConfigList[i].Param);
            }
            return ms.ToArray();
        }
    }

    public static System_GameServerConfigReturnProto GetProto(byte[] buffer)
    {
        System_GameServerConfigReturnProto proto = new System_GameServerConfigReturnProto();
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.ConfigCount = ms.ReadInt();
            proto.ServerConfigList = new List<ConfigItem>();
            for (int i = 0; i < proto.ConfigCount; i++)
            {
                ConfigItem _ServerConfig = new ConfigItem();
                _ServerConfig.ConfigCode = ms.ReadUTF8String();
                _ServerConfig.IsOpen = ms.ReadBool();
                _ServerConfig.Param = ms.ReadUTF8String();
                proto.ServerConfigList.Add(_ServerConfig);
            }
        }
        return proto;
    }
}