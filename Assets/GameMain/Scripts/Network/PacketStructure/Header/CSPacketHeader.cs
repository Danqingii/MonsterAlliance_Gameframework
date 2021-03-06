using  System;
using ProtoBuf;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 客户端发送给服务器的 包头
    /// </summary>
    [Serializable, ProtoContract(Name = @"CSPacketHeader")]
    public sealed class CSPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ClientToServer;
            }
        }

        [ProtoMember(1)]
        public override int Id
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public override int PacketLength
        {
            get;
            set;
        }
    }
}