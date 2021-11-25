using UnityEngine.UI;

namespace Game
{
    public class LoginPopup : UGuiForm
    {
        private Button m_ClosePopup;
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_ClosePopup = transform.GetComponent<Button>("Button_Close");
            m_ClosePopup.onClick.AddListener(OnClosePopupClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
        }

        private void OnClosePopupClick()
        {
            GameEntry.UI.CloseUIForm(this);
        }
    }
}