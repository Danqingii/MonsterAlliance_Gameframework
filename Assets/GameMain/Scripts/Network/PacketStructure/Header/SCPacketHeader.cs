using System;
using ProtoBuf;

namespace Game
{
    /// <summary>
    /// 服务器发送给客户端的 包头
    /// </summary>
    [Serializable, ProtoContract(Name = @"SCPacketHeader")]
    public sealed class SCPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ServerToClient;
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