using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedurePreload:ProcedureBase
    {
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

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            foreach (var state in m_LoadedFlag)
            {
                if (!state.Value)
                {
                    return;
                }
            }

            //预加载结束 开始Lua虚拟机
            GameEntry.Lua.StartVirtualMachine();
            
            //预加载结束 开始调整场景
            procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Main"));
            procedureOwner.SetData<VarString>("NextProcedure",typeof(ProcedureLogin).FullName);
            ChangeState<ProcedureChangeScene>(procedureOwner);

            //测试流程
            //ChangeState<ProedureTest>(procedureOwner);
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
            
            //加载lua脚本 可以加载很多  把一些比较大的Lua脚本给一次性加载完成 
            LoadLuaScript("LuaMain");
            LoadLuaScript("LuaEntry");
            LoadLuaScript("LuaConfig");
            LoadLuaScript("LuaDefault");
            
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
        
        private void LoadLuaScript(string luaScriptName)
        {
            m_LoadedFlag.Add(string.Format("LuaScript.{0}", luaScriptName), false);
            GameEntry.Lua.LoadLuaScript(luaScriptName, this);
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

            //TODO 这里有点问题
            m_LoadedFlag[Utility.Text.Format("LuaScript.{0}", ne.LuaScriptName)] = true;
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