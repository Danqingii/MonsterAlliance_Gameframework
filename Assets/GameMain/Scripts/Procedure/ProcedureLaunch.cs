using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 流程开始  生命周期的启动
    /// </summary>
    public class ProcedureLaunch:ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }
        
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            
            //切换到展示流程
            ChangeState<ProcedureSplash>(procedureOwner);
        }
    }
}