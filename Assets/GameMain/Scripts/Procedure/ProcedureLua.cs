using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using XLua;

namespace Game
{
    public sealed class ProcedureLua :ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        private IFsm<IProcedureManager> m_ProcedureOwner;   //当前的C#状态
        private LuaTable m_CurrentLuaProcedure;             //当前的Lua脚本

        private LuaFunction m_OnEnter;
        private LuaFunction m_OnUpdate;
        private LuaFunction m_OnLeave;

        public string CurrentLuaProcedure
        {
            get;
            private set;
        }
        
        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_ProcedureOwner = procedureOwner;
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_OnEnter?.Call(this);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            m_OnUpdate?.Call(this,elapseSeconds,realElapseSeconds);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            m_OnLeave?.Call(this,isShutdown);
            base.OnLeave(procedureOwner, isShutdown);
        }

        public void ChangeProcedureLua(string luaScriptName,string luaTableName)
        {
            bool check = GameEntry.Lua.CheckLuaScript(luaScriptName);

            if (!check)
            {
                Log.Fatal($"ChangeProcedureLua :{luaScriptName} Fatal.");
                return;
            }

            m_CurrentLuaProcedure = null;
            m_CurrentLuaProcedure = GameEntry.Lua.GetGlobalLuaTable(luaScriptName,luaTableName);
            if (m_CurrentLuaProcedure == null)
            {
                Log.Fatal($"{luaScriptName} 加载Lua表失败.");
                return;
            }

            m_OnEnter = null;
            m_OnUpdate = null;
            
            m_OnEnter = m_CurrentLuaProcedure.Get<LuaFunction>("OnEnter");
            m_OnUpdate = m_CurrentLuaProcedure.Get<LuaFunction>("OnUpdate");
            
            if (m_OnEnter == null)
            {
                Log.Error($"'{luaTableName}' Get luaFunction OnEnter Is empty.");
            }
            if (m_OnUpdate == null)
            {
                Log.Error($"'{luaTableName}' Get luaFunction OnUpdate Is empty.");
            }

            CurrentLuaProcedure = luaScriptName;
            ChangeState<ProcedureLua>(m_ProcedureOwner);
            
            //需要等待改变结束后 在执行顺序
            m_OnLeave = null;
            m_OnLeave = m_CurrentLuaProcedure.Get<LuaFunction>("OnLeave");
            if (m_OnLeave == null)
            {
                Log.Error($"'{luaTableName}' Get luaFunction OnLeave Is empty.");
            }
        }
    }
}