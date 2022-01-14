using System;
using System.Net;
using GameFramework.Network;
using UnityEngine;
using UnityGameFramework.Runtime;
using XLua;


namespace Game
{
    [LuaCallCSharp]
    public static class NetworkExtension
    {
        //lua要调用的 类型
        public static INetworkChannel CreateChannelAndConnect(this NetworkComponent self,string name,int serviceType,string ip,int post)
        {
            NetworkChannelHelper helper = new NetworkChannelHelper();
            try
            {
                INetworkChannel channel = self.CreateNetworkChannel(name, (ServiceType) serviceType, helper);
                channel.Connect(IPAddress.Parse(ip),post);
                return channel;
            }
            catch (Exception e)
            {
                Log.Debug($"创建网络频道失败 {e}");
                return null;
            }
        }
    }
}