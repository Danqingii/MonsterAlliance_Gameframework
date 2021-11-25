using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;
using System;

namespace Game
{
    public sealed class LuaComponent : GameFrameworkComponent ,ICSharpCallLua
    {
        private LuaManager m_LuaManager;
        
        private Action m_Initialize;
        private Action m_Update;
        private Action m_FixedUpdate;
        private Action m_Destroy;
        private Action<List<byte[]>> m_ReceiveMsg;

        public void Start()
        {
            //初始化虚拟机
            m_LuaManager = new LuaManager();

            //设置运行时版本
            m_LuaManager.LuaEnv.Global.Set("RUNTIME_VERSION", 1);

            m_LuaManager.DoString("require 'Launcher'");

            m_Initialize = m_LuaManager.BindToLua<Action>("Initialize");
            m_Update = m_LuaManager.BindToLua<Action>("Update");
            m_FixedUpdate = m_LuaManager.BindToLua<Action>("FixedUpdate");
            m_Destroy = m_LuaManager.BindToLua<Action>("OnDestroy");
            m_ReceiveMsg = m_LuaManager.BindToLua<Action<List<byte[]>>>("OnReceiveMsg");

            m_Initialize?.Invoke();
        }

        public void OnReceiveMsg(ref List<byte[]> msg)
        {
            if (null != m_ReceiveMsg)
            {
                try
                {
                    m_ReceiveMsg(msg);
                }
                catch (GameFrameworkException e)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Lua: ReceiveMsg '{0}'.", e));
                }
            }
        }

        private void Update()
        {
            m_Update?.Invoke();
            m_LuaManager.Update();
        }
        private void FixedUpdate()
        {
            m_FixedUpdate?.Invoke();
        }
        private void OnDestroy()
        {
            m_Destroy?.Invoke();

            m_Initialize = null;
            m_Update = null;
            m_FixedUpdate = null;
            m_Destroy = null;
            m_ReceiveMsg = null;

            m_LuaManager.OnDestroy();
        }
    }
}