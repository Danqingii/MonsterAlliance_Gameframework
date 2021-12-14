using System;
using GameFramework.Fsm;
using GameFramework.Procedure;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public abstract class ProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        protected ProcedureOwner m_ProcedureOwner = null;
        protected ProcedureLuaWorker m_LuaWorker = null;
        
        // 获取流程是否使用原生对话框
        // 在一些特殊的流程（如游戏逻辑对话框资源更新完成前的流程）中，可以考虑调用原生对话框进行消息提示行为
        public abstract bool UseNativeDialog
        {
            get;
        }
        
        /// <summary>
        /// 若希望Procedure使用Lua脚本，则重载此属性并返回Lua脚本名。
        /// </summary>
        public virtual string LuaScriptName
        {
            get
            {
                return null;
            }
        }
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;

            if (!string.IsNullOrEmpty(LuaScriptName))
            {
                m_LuaWorker = new ProcedureLuaWorker(this, LuaScriptName, ChangeStateForLua);
                m_LuaWorker.OnEnter(procedureOwner);
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_LuaWorker != null)
            {
                m_LuaWorker.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            if (m_LuaWorker != null)
            {
                m_LuaWorker.OnLeave(procedureOwner, isShutdown);
            }
        }

        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);

            if (m_LuaWorker != null)
            {
                m_LuaWorker.OnDestroy(procedureOwner);
                m_LuaWorker.Cleanup();
                m_LuaWorker = null;
            }
        }

        /// <summary>
        /// 为了在 Lua 的 Procedure 逻辑中切换状态所以创建此函数。
        /// </summary>
        /// <param name="stateType">目标 Procedure 的类型。</param>
        private void ChangeStateForLua(Type stateType)
        {
            ChangeState(m_ProcedureOwner, stateType);
        }
    }
}