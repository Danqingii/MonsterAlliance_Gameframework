using MongoDB.Bson;

namespace Game
{
    /// <summary>
    /// MongoDB 接口
    /// </summary>
    public interface IMongoDataBase
    {
        /// <summary>
        /// 数据库唯一标识
        /// </summary>
        ObjectId Id
        {
            get;
        }
    }
}