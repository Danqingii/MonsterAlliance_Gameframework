using System.Net;
using GameFramework.Fsm;
using GameFramework.Network;
using GameFramework.Procedure;

namespace Game
{
    public class ProcedureNetwork:ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
        }
    }
}