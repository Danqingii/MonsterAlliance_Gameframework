using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedurePreload:ProcedureBase
    {
        //配置表 最好也是可更新的
        public static readonly string[] DataTableNames = new string[]
        {
            "UIForm",
            "Scene",
        };
        
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }
        
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();
        private bool m_LoadLuaConfig = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            GameEntry.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
            GameEntry.Event.Subscribe(LoadLuaScriptSuccessEventArgs.EventId, OnLoadLuaScriptSuccess);
            GameEntry.Event.Subscribe(LoadLuaScriptFailureEventArgs.EventId, OnLoadLuaScriptFailure);
            
            m_LoadedFlag.Clear();
            
            //加载资源
            PreloadResources();
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);
            GameEntry.Event.Unsubscribe(LoadLuaScriptSuccessEventArgs.EventId, OnLoadLuaScriptSuccess);
            GameEntry.Event.Unsubscribe(LoadLuaScriptFailureEventArgs.EventId, OnLoadLuaScriptFailure);

            m_LoadedFlag.Clear();
            m_LoadedFlag = null;
            m_LoadLuaConfig = false;
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_LoadLuaConfig)
            {
                return;
            }
            
            foreach (var flag in m_LoadedFlag)
            {
                if (!flag.Value)
                {
                    return;
                }
            }

            //预加载结束 开始执行Lua虚拟机
            GameEntry.Lua.StartVirtualMachine();
            
            //等待Lua虚拟机开启完毕  进入Lua进行流程接管
            ProcedureLua procedureLua = (ProcedureLua)GameEntry.Procedure.GetProcedure<ProcedureLua>();
            procedureLua.ChangeProcedureLua("Procedure/ProcedureEntry", "ProcedureEntry");

            //预加载结束 开始调整场景
            //procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Main"));
            //procedureOwner.SetData<VarString>("NextProcedure",typeof(ProcedureLogin).FullName);
            //ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        private void PreloadResources()
        {
            //加载配置文件
            LoadConfig("DefaultConfig");

            //加载配置表
            foreach (string dataTableName in DataTableNames)
            {
                LoadDataTable(dataTableName);
            }

            //加载字典
            //LoadDictionary("Default");
            
            //加载字体
            //LoadFont();
            
            //加载lua脚本 可以加载很多 把lua脚本一次性加载完
            LoadLuaScript();
        }
        
        private void LoadConfig(string configName)
        {
            string configAssetName = AssetUtility.GetConfigAsset(configName, false);
            m_LoadedFlag.Add(configAssetName, false);
            GameEntry.Config.ReadData(configAssetName, this);
        }

        private void LoadDataTable(string dataTableName)
        {
            string dataTableAssetName = AssetUtility.GetDataTableAsset(dataTableName, false);
            m_LoadedFlag.Add(dataTableAssetName, false);
            GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetName, this);
        }

        private void LoadDictionary(string dictionaryName)
        {
            string dictionaryAssetName = AssetUtility.GetDictionaryAsset(dictionaryName, false);
            m_LoadedFlag.Add(dictionaryAssetName, false);
            Log.Info(dictionaryAssetName);
            GameEntry.Localization.ReadData(dictionaryAssetName, this);
        }
        
        private void LoadLuaScript()
        {
            //先加载一下配置表 通过lua配置表 才真正的加载lua脚本
            GameEntry.Resource.LoadAsset(AssetUtility.GetLuaScriptNameConfig(), new LoadAssetCallbacks(
                (assetName, asset, duration, userData) =>
                {
                     string[] luaScriptNames = Utility.Json.ToObject<string[]>(((TextAsset)asset).text);
                  
                     for (int i = 0; i < luaScriptNames.Length; i++)
                     {
                         m_LoadedFlag.Add(luaScriptNames[i],false);
                         GameEntry.Lua.LoadLuaScript(luaScriptNames[i], this);
                     }
                     
                     m_LoadLuaConfig = true;
                },

                (assetName, status, errorMessage, userData) =>
                {
                    Log.Error("LuaScriptNameConfig 加载失败 路径: '{0}'.", AssetUtility.GetLuaScriptNameConfig());
                }));
        }
        
        private void LoadFont(string fontName)
        {
            m_LoadedFlag.Add(Utility.Text.Format("Font.{0}", fontName), false);
            GameEntry.Resource.LoadAsset(AssetUtility.GetFontAsset(fontName), Constant.AssetPriority.FontAsset, new LoadAssetCallbacks(
                (assetName, asset, duration, userData) =>
                {
                    m_LoadedFlag[Utility.Text.Format("Font.{0}", fontName)] = true;
                    //加载字体 设置主要字体
                    //UGuiForm.SetMainFont((Font)asset);
                    Log.Info("Load font '{0}' OK.", fontName);
                },

                (assetName, status, errorMessage, userData) =>
                {
                    Log.Error("Can not load font '{0}' from '{1}' with error message '{2}'.", fontName, assetName, errorMessage);
                }));
        }
        
        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.ConfigAssetName] = true;
            Log.Info("Load config '{0}' OK.", ne.ConfigAssetName);
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName, ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DataTableAssetName] = true;
            Log.Info("Load data table '{0}' OK.", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DictionaryAssetName] = true;
            Log.Info("Load dictionary '{0}' OK.", ne.DictionaryAssetName);
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName, ne.DictionaryAssetName, ne.ErrorMessage);
        }
        
        private void OnLoadLuaScriptSuccess(object sender, GameEventArgs e)
        {
            LoadLuaScriptSuccessEventArgs ne = (LoadLuaScriptSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.LuaScriptName] = true;
            Log.Info("Load lua script '{0}' OK.", ne.LuaScriptName);
        }

        private void OnLoadLuaScriptFailure(object sender, GameEventArgs e)
        {
            LoadLuaScriptFailureEventArgs ne = (LoadLuaScriptFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load lua script '{0}' from '{1}' with error message '{2}'.", ne.LuaScriptName, ne.LuaScriptAssetName, ne.ErrorMessage);
        }
    }
}