using System;
using ProtoBuf;

namespace Game
{
    public class SCLogin : SCPacketBase
    {
        public SCLogin()
        {
        }

        [ProtoMember(1)]
        public override int Id
        {
            get
            {
                return 20001;
            }
        }
        
        [ProtoMember(2)]
        public bool IsCanLogin
        {
            get;
            set;
        }

        public override void Clear()
        {
        }
    }
}