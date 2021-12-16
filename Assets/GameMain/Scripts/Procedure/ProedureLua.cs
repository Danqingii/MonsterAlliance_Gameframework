using GameFramework.Fsm;
using GameFramework.Procedure;

namespace Game
{
    public class ProedureLua :ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
        }
    }
}