using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace Game
{
    public class ProcedureTest:ProcedureBase
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
            
            //测试下载
            //int id = GameEntry.Download.AddDownload(Application.dataPath + "/Test/测试","https://blog.csdn.net/youshi520000/article/details/51492606");
        }
    }
}