using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;
using System.IO;
using GameFramework.Resource;
using UnityEngine;
using XLua;

namespace Game
{
    [DisallowMultipleComponent]
    public sealed partial class LuaComponent : GameFrameworkComponent
    {
        //Lua脚本缓存
        private readonly Dictionary<string, byte[]> m_LuaScriptCache = new Dictionary<string, byte[]>();
        
        //加载Lua脚本回调 里面有一个加载成功回调  有一个加载失败回调
        private LoadAssetCallbacks m_LoadAssetCallbacks = null;

        private static LuaEnv m_LuaEnv = null;   //Lua虚拟机
        private LuaTable m_LuaTable = null;      //Lua表
        private LuaTable m_LuaMetaTable = null;  //Lua元表

        private System.Action m_LuaOnStart = null;
        private System.Action<float, float> m_LuaOnUpdate = null;
        private System.Action<float, float> m_LuaOnFixedUpdate = null;
        private System.Action m_LuaOnDestroy = null;

        private float m_LastGCTime = 0;
        private float m_GCInterval = 1;
        
        private void Start()
        {
            m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadLuaScriptSuccessCallback, LoadLuaScriptFailureCallback);
        }

        private void Update()
        {
            m_LuaOnUpdate?.Invoke(Time.deltaTime,Time.unscaledDeltaTime);
           
            if (m_LuaEnv != null && Time.time - m_LastGCTime > m_GCInterval)
            {
                m_LuaEnv.Tick();
                m_LastGCTime = Time.time;
            }
        }

        private void FixedUpdate()
        {
            m_LuaOnFixedUpdate?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            m_LuaOnDestroy?.Invoke();
            
            if (m_LuaMetaTable != null)
            {
                m_LuaMetaTable.Dispose();
                m_LuaMetaTable = null;
            }

            if (m_LuaTable != null)
            {
                m_LuaTable.Dispose();
                m_LuaTable = null;
            }
            
            m_LuaScriptCache.Clear();
            
            m_LuaOnStart = null;
            m_LuaOnUpdate = null;
            m_LuaOnFixedUpdate = null;
            m_LuaOnDestroy = null;
        }
        
        //开启Lua虚拟机 真正的开始启动Lua
        public void StartVirtualMachine()
        {
            if (m_LuaEnv != null)
            {
                m_LuaEnv.Dispose();
                m_LuaEnv = null;
            }

            m_LuaEnv = new LuaEnv();
            m_LuaEnv.AddLoader(CustomLuaScriptLoader);

            if (m_LuaMetaTable == null)
            {
                m_LuaMetaTable = m_LuaEnv.NewTable();
                m_LuaMetaTable.Set("__index", m_LuaEnv.Global);
            }

            if (m_LuaTable == null)
            {
                m_LuaTable = NewTable();
                DoScript("LuaMain", "LuaComponent"); //执行Lua进入脚本

                m_LuaTable.Get("OnStart", out m_LuaOnStart);
                m_LuaTable.Get("OnUpdate", out m_LuaOnUpdate);
                m_LuaTable.Get("OnFixedUpdate", out m_LuaOnFixedUpdate);
                m_LuaTable.Get("OnDestroy", out m_LuaOnDestroy);
            }

            m_LuaOnStart?.Invoke();
        }

        //添加一个Lua新表 TODO 这里还有一个不太理解 就是 如果是否可以添加多个元表
        public LuaTable NewTable()
        {
            if (m_LuaEnv == null)
            {
                Log.Error("LuaEnv is invalid.");
                return null;
            }

            if (m_LuaMetaTable == null)
            {
                Log.Error("LuaMetaTabl is invalid.");
                return null;
            }

            LuaTable luaTable = m_LuaEnv.NewTable();
            luaTable.SetMetaTable(m_LuaMetaTable);

            return luaTable;
        }
        
        //执行Lua脚本
        public object[] DoScript(string luaScriptName, string chunkName)
        {
            if (m_LuaEnv == null)
            {
                Log.Error("LuaEnv is invalid.");
                return null;
            }

            if (string.IsNullOrEmpty(luaScriptName))
            {
                Log.Error("Lua script name is invalid.");
                return null;
            }

            //如果不存在该Lua脚本 可以直接异常
            if (!m_LuaScriptCache.TryGetValue(luaScriptName, out var luaScriptBytes))
            {
                Log.Error("Can not find lua script '{0}', please LoadScript first.", luaScriptName);
                return null;
            }

            return m_LuaEnv.DoString(luaScriptBytes, chunkName, m_LuaTable);
        }
        
        //真正的加载Lua脚本
        public void LoadLuaScript(string luaScriptName, object userData = null)
        {
            if (string.IsNullOrEmpty(luaScriptName))
            {
                Log.Warning("Lua script name is invalid.");
                return;
            }

            //得到lua脚本 真正的路径
            string luaScriptAssetName = AssetUtility.GetLuaScriptAsset(luaScriptName);
            if (GameEntry.Base.EditorResourceMode)
            {
                //脚本不存在 我们分发异常消息
                if (!File.Exists(luaScriptAssetName))
                {
                    string appendErrorMessage = $"Can not find lua script '{luaScriptAssetName}'.";
                    GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadLuaScriptFailureEventArgs>().Fill(luaScriptName, luaScriptAssetName, appendErrorMessage, userData));
                    return;
                }

                //真正的Lua数组器 可以用来刷新数据流
                byte[] luaBytes = File.ReadAllBytes(luaScriptAssetName); 

                //内部具体的加载
                InternalLoadLuaScript(luaScriptName, luaScriptAssetName,luaBytes, 0f, userData);
            }
            else
            {
                GameEntry.Resource.LoadAsset(luaScriptAssetName, m_LoadAssetCallbacks, new LuaScriptInfo(luaScriptName, userData));
            }
        }

        //自定义lua脚本加载器  我们只加载缓存  如果不存在就出错
        private byte[] CustomLuaScriptLoader(ref string luaScriptName)
        {
            if (!m_LuaScriptCache.TryGetValue(luaScriptName, out var luaScriptBytes))
            {
                Log.Warning("Can not find lua script '{0}'.", luaScriptName);
                return null;
            }
            return luaScriptBytes;
        }
        
        //内部核查真正的lua脚本
        private void InternalLoadLuaScript(string luaScriptName, string luaScriptAssetName, byte[] luaScripBytes, float duration, object userData)
        {
            if (luaScripBytes[0] == 239 && luaScripBytes[1] == 187 && luaScripBytes[2] == 191)
            {
                // 处理UFT-8 BOM头
                luaScripBytes[0] = luaScripBytes[1] = luaScripBytes[2] = 32;
            }

            m_LuaScriptCache[luaScriptName] = luaScripBytes;
            GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadLuaScriptSuccessEventArgs>().Fill(luaScriptName, luaScriptAssetName, duration, userData));
        }

        
        //加载Lua脚本成功回调 会把成功的Lua脚本缓存添加进入缓存器中
        private void LoadLuaScriptSuccessCallback(string luaScriptAssetName, object asset, float duration, object userData)
        {
            TextAsset luaScriptAsset = asset as TextAsset;
            if (luaScriptAsset == null)
            {
                Log.Warning("Lua script asset '{0}' is invalid.", luaScriptAssetName);
                return;
            }

            LuaScriptInfo luaScriptInfo = (LuaScriptInfo)userData;
            byte[] luaBytes = luaScriptAsset.bytes;
            InternalLoadLuaScript(luaScriptInfo.LuaScriptName, luaScriptAssetName, luaBytes, duration, luaScriptInfo.UserData);
        }

        //加载Lua脚本失败回调
        private void LoadLuaScriptFailureCallback(string luaScriptAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            LuaScriptInfo luaScriptInfo = (LuaScriptInfo)userData;
            string appendErrorMessage = Utility.Text.Format("Load lua script failure, asset name '{0}', status '{1}', error message '{2}'.", luaScriptAssetName, status.ToString(), errorMessage);
            GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadLuaScriptFailureEventArgs>().Fill(luaScriptInfo.LuaScriptName, luaScriptAssetName, appendErrorMessage, luaScriptInfo.UserData));
        }

#if UNITY_EDITOR
        /// <summary>
        /// 重新加载指定的Lua脚本,只在编辑器状态下可用
        /// </summary>
        public void LoadScriptInEditor(string luaScriptName, object userData = null)
        {
            if (GameEntry.Base.EditorResourceMode)
            {
                string luaScriptAssetName = AssetUtility.GetLuaScriptAsset(luaScriptName);
                if (!File.Exists(luaScriptAssetName))
                {
                    Log.Error("Can not find lua script '{0}'.", luaScriptAssetName);
                    return;
                }
                InternalLoadLuaScript(luaScriptName, luaScriptAssetName, File.ReadAllBytes(luaScriptAssetName), 0f, userData);
                m_LuaEnv.DoString(Utility.Text.Format("package.loaded[\"{0}\"] = nil", luaScriptName), luaScriptName);
                Debug.Log(luaScriptName);
            }
        }
#endif

    }
}