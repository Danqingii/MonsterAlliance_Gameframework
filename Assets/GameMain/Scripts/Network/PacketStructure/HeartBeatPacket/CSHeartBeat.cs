using ProtoBuf;
using System;

namespace Game
{
    /// <summary>
    /// 客户端-服务器 心跳包
    /// </summary>
    [Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat : CSPacketBase
    {
        public CSHeartBeat()
        {
        }

        public override int Id
        {
            get
            {
                return PacketCoding.CSHeartBeat;
            }
        }

        public override void Clear()
        {
        }
    }
}