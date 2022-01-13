using TMPro;
using UnityEngine.UI;

namespace Game
{
    public class RegisterPopup:UGuiForm
    {
        private Button m_ClosePopup;
        private Button m_ConfirmRegisterBtn;
        private TMP_InputField m_EmailField;    
        private TMP_InputField m_UserNameField;
        private TMP_InputField m_PasswordField;
        
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_ClosePopup = transform.GetComponent<Button>("Button_Close");
            m_ConfirmRegisterBtn = transform.GetComponent<Button>("Button_ConfirmRegister");
            m_EmailField = transform.GetComponent<TMP_InputField>("InputField_Email");
            m_UserNameField = transform.GetComponent<TMP_InputField>("InputField_UserName");
            m_PasswordField = transform.GetComponent<TMP_InputField>("InputField_Password");
            
            m_ClosePopup.onClick.AddListener(OnClosePopupClick);
            m_ConfirmRegisterBtn.onClick.AddListener(OnRegisterClick);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
        }

        private void OnClosePopupClick()
        {
            GameEntry.UI.CloseUIForm(this);
        }

        private void OnRegisterClick()
        {
            /*
            RegisterDBData dbData = RegisterDBData.Create(m_EmailField.text,m_UserNameField.text,m_PasswordField.text);
            bool insertState = GameEntry.MongoDB.InsertData("Login",dbData);
            dbData.Clear();
            
            //清除数据 如果
            m_EmailField.text = m_UserNameField.text = m_PasswordField.text = null;

            //注册失败 调用原生接口
            if (!insertState)
            {
                //GameEntry.UI.OpenDialog();
            }
            */
        }
    }
}