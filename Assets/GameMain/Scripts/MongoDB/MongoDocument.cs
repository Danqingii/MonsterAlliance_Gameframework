using GameFramework;
using MongoDB.Bson;

namespace Game
{
    /// <summary>
    /// Mongo文件 基类
    /// </summary>
    public abstract class MongoDocument :  IMongoDocument ,IReference
    {
        private ObjectId m_Id;

        protected MongoDocument()
        {
            m_Id = ObjectId.Empty;
        }
        
        /// <summary>
        /// 数据库唯一标识
        /// </summary>
        public ObjectId ObjectId
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

        public long Id
        {
            get;
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