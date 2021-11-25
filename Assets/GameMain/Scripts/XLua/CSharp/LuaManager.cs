using System.IO;
using UnityEngine;
using UnityGameFramework.Runtime;
using XLua;

namespace Game
{
    public class LuaManager : ILuaManager
    {
        private LuaEnv m_LuaEnv;
        private float m_lastGCTime = 0;
        private float m_GCInterval = 1;

        public LuaEnv LuaEnv 
        {
            get
            {
                return m_LuaEnv;
            } 
        }

        public LuaManager()
        {
            Log.Info("LuaManager contruct");
            m_LuaEnv = new LuaEnv();
            m_LuaEnv.AddLoader(CustomLoader);

            m_LuaEnv.GcPause = 100;    //GC暂停
            m_LuaEnv.GcStepmul = 5000; //内存分配
        }

        private byte[] CustomLoader(ref string luaName)
        {
            //TODO
            string fullPath = Application.dataPath + "/GameMain/Scripts/XLua/Lua/" + luaName + ".lua";
            return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(fullPath));//执行lua程序
        }

        public void RestartGc()
        {
            if (null == m_LuaEnv)
            {
                return;
            }
            m_LuaEnv.RestartGc();
        }

        public void CollectGC()
        {
            if (null == m_LuaEnv)
            {
                return;
            }
            m_LuaEnv.FullGc();
        }

        public void DoString(string str)
        {
            if (null == m_LuaEnv)
            {
                return;
            }
            m_LuaEnv.DoString(str);
        }

        public object[] DoFile(string filename, string chunkName = "chunk", LuaTable env = null)
        {
            if (null == m_LuaEnv)
            {
                return null;
            }
            return m_LuaEnv.DoString("require '" + filename + "'", chunkName, env);
        }

        public T BindToLua<T>(string key)
        {
            if (null == m_LuaEnv)
            {
                return default(T);
            }
            T f = m_LuaEnv.Global.Get<T>(key);
            return f;
        }

        public void Update()
        {
            if (null == m_LuaEnv)
            {
                return;
            }
            if (Time.time - m_lastGCTime > m_GCInterval)
            {
                m_LuaEnv.Tick();
                m_lastGCTime = Time.time;
            }
        }

        public void OnDestroy()
        {
            if (null == m_LuaEnv)
            {
                return;
            }

            m_LuaEnv.Dispose();
            m_LuaEnv = null;

            Debug.Log("~LuaManager was destroyed!");
        }
    }
}
