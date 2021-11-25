using GameFramework;

namespace Game
{
    /// <summary>
    /// 注册MongoDB 数据
    /// </summary>
    public class RegisterDBData:MongoDataBase
    {
        /// <summary>
        /// 邮箱 唯一标识
        /// </summary>
        public string Email
        {
            get;
            private set;
        }

        /// <summary>
        /// 账号名
        /// </summary>
        public string UserName
        {
            get;
            private set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        public static RegisterDBData Create(string email,string userName,string password)
        {
            RegisterDBData registerDBData = ReferencePool.Acquire<RegisterDBData>();
            registerDBData.Email = email;
            registerDBData.UserName = userName;
            registerDBData.Password = password;
            return registerDBData;
        }

        public override void Clear()
        {
            base.Clear();
            Email = UserName = Password = null;
        }
    }
}