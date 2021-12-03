using GameFramework;
using GameFramework.Network;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 客户端-服务器 Login
    /// </summary>
    public class CSLoginHandler : PacketHandlerBase
    {
        public override int Id
        {
            get
            {
                return PacketCoding.CSLogin;
            }
        }

        public override void Handle(object sender, Packet packet)
        {
            CSLogin packetImpl = (CSLogin) packet;
            
            if (packetImpl == null)
            {
                Log.Error("CSLogin 转换失败.");
            }
            else
            {
                bool isLogin = packetImpl.Account == "110" && packetImpl.Password == "110";
                
                SCLogin scLogin = ReferencePool.Acquire<SCLogin>();
                scLogin.IsCanLogin = isLogin;
                
                Log.Info($"服务器: Receive CSLogin 包 '账号:{packetImpl.Account} 密码:{packetImpl.Password}' 返回{scLogin.IsCanLogin}.");
                GameEntry.Server.Send(scLogin);
                
                ReferencePool.Release(packetImpl);
                ReferencePool.Release(scLogin);
            }
        }
    }
}