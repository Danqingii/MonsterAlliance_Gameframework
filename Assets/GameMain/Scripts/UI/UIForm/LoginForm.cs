using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Game
{
    public class LoginForm : UGuiForm
    {
        private ProcedureLogin m_ProcedureLogin;
        
        private Button m_LoginBtn;
        private Button m_RegisterBtn;
        private Button m_GuestModelBtn;
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            m_LoginBtn = transform.GetComponent<Button>("Button_Login");
            m_RegisterBtn = transform.GetComponent<Button>("Button_Register");
            m_GuestModelBtn = transform.GetComponent<Button>("Button_GuestModel");
            
            m_LoginBtn.onClick.AddListener(OnLoginClick);
            m_RegisterBtn.onClick.AddListener(OnRegisterClick);
            m_GuestModelBtn.onClick.AddListener(OnGuestLoginClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_ProcedureLogin = (ProcedureLogin) userData;
            if (m_ProcedureLogin == null)
            {
                Log.Warning("ProcedureLogin is invalid when open ProcedureLogin.");
                return;
            }
        }

        private void OnLoginClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.LoginPopup);
        }

        private void OnRegisterClick()
        {
            GameEntry.UI.OpenUIForm(UIFormId.RegisterPopup);
        }
        
        private void OnGuestLoginClick()
        {
            
        }
    }
}