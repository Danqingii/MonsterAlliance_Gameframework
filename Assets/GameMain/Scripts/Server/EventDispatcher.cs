using System.Collections.Generic;
using GameFramework.Network;
using UnityEngine;
using UnityGameFramework.Runtime;

public partial class ServerComponent : GameFrameworkComponent
{
    /// <summary>
    /// 事件处理者 观察者模式
    /// </summary>
    private class PackDispatche
    {
        public delegate void PackHandler(object sender, Packet packet);
        private Dictionary<int, PackHandler> dic = new Dictionary<int, PackHandler>();
    
        public void Subscribe(int id, PackHandler handler)
        {
            lock (dic)
            {
                if (!dic.ContainsKey(id))
                {
                    dic[id] = handler;
                    return;
                }
                Debug.Log($"已经存在处理者{id}");
            }
        }
    
        public void UnSubscribe(int id)
        {
            lock (dic)
            {
                if (dic.ContainsKey(id))
                {
                    dic.Remove(id);
                    return;
                }
                Debug.Log($"不存在处理者{id}");
            }
        }
    
        public void Fire(object sender, Packet packet)
        {
            if (packet == null)
            {
                Debug.Log("消息为空");
                return;
            }
            
            lock (dic)
            {
                if (dic.ContainsKey(packet.Id))
                {
                    dic[packet.Id](sender,packet);
                    return;
                }
                Debug.Log($"处理者不存在{packet.Id}");
            }
        }
    }
}