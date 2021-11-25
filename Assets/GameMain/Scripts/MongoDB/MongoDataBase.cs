using GameFramework;
using MongoDB.Bson;

namespace Game
{
    public class MongoDataBase : IMongoDataBase,IReference
    {
        private ObjectId m_Id;
        
        /// <summary>
        /// 数据库唯一标识
        /// </summary>
        public ObjectId Id
        {
            get
            {
                if (m_Id == ObjectId.Empty)
                {
                    m_Id = ObjectId.GenerateNewId();
                }
                return m_Id;
            }
        }

        /// <summary>
        /// 回收
        /// </summary>
        public virtual void Clear()
        {
            m_Id = ObjectId.Empty;
        }
    }
}