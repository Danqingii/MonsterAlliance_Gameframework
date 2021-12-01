using System;
using ProtoBuf;

namespace Game
{
    [Serializable, ProtoContract(Name = @"CSLogin")]
    public class CSLogin : CSPacketBase
    {
        public CSLogin()
        {
        }

        public override int Id
        {
            get
            {
                return 10001;
            }
        }
        
        [ProtoMember(1)]
        public string Account
        {
            get;
            set;
        }
        
        [ProtoMember(2)]
        public string Password
        {
            get;
            set;
        }

        public override void Clear()
        {
        }
    }
}