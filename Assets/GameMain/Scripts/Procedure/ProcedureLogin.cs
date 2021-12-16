using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace Game
{
    public class ProcedureLogin :ProcedureBase
    {
        private bool m_LoginComplete = false;
        private LoginForm m_LoginForm = null;

        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        public void LoginComplete()
        {
            m_LoginComplete = true;
        }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoginComplete = false;
            
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
            
            GameEntry.UI.OpenUIForm(UIFormId.LoginForm, this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.Event.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            if (m_LoginForm != null)
            {
                m_LoginForm.Close(isShutdown);
                m_LoginForm = null;
            }
            
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_LoginComplete)
            {
                //进入下一个流程 或者是什么
                Log.Info("进入下一个流程");
                m_LoginForm = null;
            }
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoginForm = (LoginForm)ne.UIForm.Logic;
        }
        
        private void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Load StartForm error.");
        }
    }
}