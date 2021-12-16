using System;
using GameFramework.Fsm;
using GameFramework.Procedure;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game
{
    public abstract class ProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        /// <summary>
        /// 在此流程下，是否使用原生对话框。
        /// </summary>
        public abstract bool UseNativeDialog
        {
            get;
        }
    }
}