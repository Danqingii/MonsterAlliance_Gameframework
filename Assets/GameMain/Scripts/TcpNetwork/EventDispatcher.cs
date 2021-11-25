using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 观察者模式
    /// </summary>
    public class EventDispatcher
    {
        public delegate void OnActionHandler(byte[] buffer);
        
        private static EventDispatcher instance;

        public static EventDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventDispatcher();
                }
                return instance;
            }
        }
       
        private Dictionary<ushort, List<OnActionHandler>> dic = new Dictionary<ushort, List<OnActionHandler>>();

        public void AddEventListener(ushort id, OnActionHandler handler)
        {
            List<OnActionHandler> handlers;
            if (dic.TryGetValue(id,out  handlers))
            {
                handlers.Add(handler);
                dic[id] = handlers;
            }
            else
            {
                handlers = new List<OnActionHandler>();
                handlers.Add(handler);
                dic.Add(id,handlers);
            }
        }

        public void RemoveListener(ushort id,OnActionHandler handler)
        {
            List<OnActionHandler> handlers;
            if (dic.TryGetValue(id, out handlers))
            {
                bool isSucceed = handlers.Remove(handler);
                if (!isSucceed)
                {
                    Debug.LogError("不存在事件");
                    return;
                }
                dic[id] = handlers; 
            }
            else
            {
                Debug.LogError($"不存在事件Id{id}");
            }
        }

        public void Dispatch(ushort id,byte[] buffer)
        {
            List<OnActionHandler> handlers;
            if (dic.TryGetValue(id, out handlers))
            {
                for (int i = 0; i < handlers.Count; i++)
                {
                    if (handlers[i] != null)
                    {
                        handlers[i].Invoke(buffer);
                    }
                }
            }
            else
            {
                Debug.LogError($"不存在事件Id{id}");
            }
        }
    }
}