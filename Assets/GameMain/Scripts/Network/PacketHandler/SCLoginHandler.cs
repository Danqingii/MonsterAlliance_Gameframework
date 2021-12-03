using GameFramework.Network;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 服务器-客户端 登陆回调
    /// </summary>
    public class SCLoginHandler : PacketHandlerBase
    {
        public override int Id
        {
            get
            {
                return PacketCoding.SCLogin;
            }
        }
        public override void Handle(object sender, Packet packet)
        {
            SCLogin packetImpl = (SCLogin) packet;
            Log.Info("客户端: Receive SCLogin 包 '{0}'.", packetImpl.IsCanLogin.ToString());
        }
    }
}