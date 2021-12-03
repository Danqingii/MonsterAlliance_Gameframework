using System;
using ProtoBuf;

namespace Game
{
    [Serializable, ProtoContract(Name = @"SCLogin")]
    public class SCLogin : SCPacketBase
    {
        public override int Id
        {
            get
            {
                return PacketCoding.SCLogin;
            }
        }
        
        [ProtoMember(1)]
        public bool IsCanLogin
        {
            get;
            set;
        }

        public override void Clear()
        {
            IsCanLogin = false;
        }
    }
}