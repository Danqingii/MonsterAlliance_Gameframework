using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game
{
    /// <summary>
    /// 唯一实体
    /// </summary>
    public class YFUniqueEntity
    {
        /// <summary>
        /// Id
        /// </summary>
        public ObjectId Id;

        public ushort Type;
        public long CurrId;
    }
    
    /// <summary>
    /// 唯一标识基类
    /// </summary>
    public abstract class MongoUniqueBase
    {
        private FindOneAndUpdateOptions<YFUniqueEntity> options = new FindOneAndUpdateOptions<YFUniqueEntity>()
        {
            IsUpsert = true
        };
        
        /// <summary>
        /// MongoClient
        /// </summary>
        protected abstract MongoClient Client
        {
            get;
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        protected abstract string DatabaseName
        {
            get;
        }

        /// <summary>
        /// 集合名称
        /// </summary>
        protected abstract string CollectionName
        {
            get;
        }

        private IMongoCollection<MongoDocument> m_Collection;
        
        /// <summary>
        /// 获取文档集合
        /// </summary>
        /// <returns></returns>
        public IMongoCollection<MongoDocument> GetCollection()
        {
            if (m_Collection == null)
            {
                IMongoDatabase database = Client.GetDatabase(DatabaseName);
                m_Collection = database.GetCollection<MongoDocument>(CollectionName);
            }
            return m_Collection;
        }

        /// <summary>
        /// 获取唯一ID
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="seq">自增值</param>
        /// <returns></returns>
        public long GetUniqueID(int type, int seq = 1)
        {
            var collection = GetCollection();

            FilterDefinition<YFUniqueEntity> eq = Builders<YFUniqueEntity>.Filter.Eq(t => t.Type, type);
            UpdateDefinition<YFUniqueEntity> inc = Builders<YFUniqueEntity>.Update.Inc(t => t.CurrId, seq);

            //FindOneAndUpdateOptions<TDocument, TProjection> options = null
            
            //YFUniqueEntity cc = m_Collection.FindOneAndUpdate<YFUniqueEntity>(eq, inc,options);

            return 0;
        }
    }
}