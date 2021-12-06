using GameFramework;
using MongoDB.Bson;

namespace Game
{
    /// <summary>
    /// Mongo文件 基类
    /// </summary>
    public class MongoDocument : BsonDocument ,IMongoDocument ,IReference
    {
        private ObjectId m_Id;
        
        public ObjectId Id
        {
            get
            {
                return m_Id;
            }
        }

        public MongoDocument()
        {
            m_Id = ObjectId.GenerateNewId();
        }

        public override void Clear()
        {
            m_Id = ObjectId.Empty;
        }
    }
}