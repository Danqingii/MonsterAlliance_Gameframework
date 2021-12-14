using System;
using GameFramework;
using UnityGameFramework.Runtime;
using XLua;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    /// <summary>
    /// 将C#的GF Procedure逻辑处理转嫁到Lua中</summary>
    /// <remarks>
    /// </remarks>
    [LuaCallCSharp]
    public class ProcedureLuaWorker
    {
        public delegate void DelegateOnDestroy(LuaTable self, ProcedureOwner procedureOwner);

        public delegate void DelegateOnEnter(LuaTable self, ProcedureOwner procedureOwner);

        public delegate void DelegateOnLeave(LuaTable self, ProcedureOwner procedureOwner, bool isShutdown);

        public delegate void DelegateOnUpdate(LuaTable self, ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds);

        private LuaTable _scriptEnv = null;
        private LuaTable _workerTable = null;

        private DelegateOnDestroy _luaOnDestroy = null;
        private DelegateOnEnter _luaOnEnter = null;
        private DelegateOnLeave _luaOnLeave = null;
        private DelegateOnUpdate _luaOnUpdate = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="target">要转嫁的目标Procedure</param>
        /// <param name="scriptName"></param>
        /// <param name="csChangeState"></param>
        public ProcedureLuaWorker(ProcedureBase target, string scriptName, Action<Type> csChangeState)
        {
            try
            {
                _scriptEnv = GameEntry.Lua.NewTable();
                _scriptEnv.Set("self", this);

                var objs = GameEntry.Lua.DoScript(scriptName, scriptName, _scriptEnv);
                _workerTable = objs[0] as LuaTable;
                _workerTable.Set("target", target);
                _workerTable.Set("CS_ChangeState", csChangeState);

                _workerTable.Get("OnDestroy", out _luaOnDestroy);
                _workerTable.Get("OnEnter", out _luaOnEnter);
                _workerTable.Get("OnLeave", out _luaOnLeave);
                _workerTable.Get("OnUpdate", out _luaOnUpdate);
            }
            catch (SystemException e)
            {
                Log.Error(Utility.Text.Format("ProcedureLuaWorker.Initialize failed: '{0}'.",e.Message));
                Cleanup();
            }
        }

        /// <summary>
        /// 最后清理.
        /// </summary>
        public void Cleanup()
        {
            _luaOnEnter = null;
            _luaOnLeave = null;
            _luaOnUpdate = null;
            _luaOnDestroy = null;
            _workerTable = null;

            if (_scriptEnv != null)
                _scriptEnv.Dispose();
            _scriptEnv = null;
        }

        /// <summary>
        /// 对Procedure的OnDestroy的回调接口.
        /// </summary>
        public void OnDestroy(ProcedureOwner procedureOwner)
        {
            if (_luaOnDestroy != null)
                _luaOnDestroy(_workerTable, procedureOwner);
        }

        /// <summary>
        /// 对Procedure的OnEnter的回调接口.
        /// </summary>
        public void OnEnter(ProcedureOwner procedureOwner)
        {
            if (_luaOnEnter != null)
                _luaOnEnter(_workerTable, procedureOwner);
        }

        /// <summary>
        /// 对Procedure的OnLeave的回调接口.
        /// </summary>
        public void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            if (_luaOnLeave != null)
                _luaOnLeave(_workerTable, procedureOwner, isShutdown);
        }

        /// <summary>
        /// 对Procedure的OnUpdate的回调接口.
        /// </summary>
        public void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if (_luaOnUpdate != null)
                _luaOnUpdate(_workerTable, procedureOwner, elapseSeconds, realElapseSeconds);
        }
    }
}