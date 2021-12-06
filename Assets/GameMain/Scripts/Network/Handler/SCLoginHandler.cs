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
            if (packetImpl == null)
            {
                Log.Error("客户端: SCLogin 转换失败.");
            }
            else
            {
                Log.Info("客户端: 接收服务器返回登陆协议 '{0}'.", packetImpl.IsCanLogin.ToString());
            }
        }
    }
}