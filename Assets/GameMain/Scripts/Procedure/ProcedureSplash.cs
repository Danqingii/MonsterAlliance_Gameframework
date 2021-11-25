using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedureSplash:ProcedureBase
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

            //编辑器模式
            if (GameEntry.Base.EditorResourceMode)
            {
                Log.Info("Editor resource mode detected.");
                ChangeState<ProcedurePreload>(procedureOwner);
            }
            //单机模式
            else if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
            {
                //TODO
            }
            //可更新模式
            else if (GameEntry.Resource.ResourceMode == ResourceMode.Updatable)
            {
                //TODO
            }
            //下载的可更新模式
            else
            {
                //TODO
            }
        }
    }
}