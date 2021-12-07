using MongoDB.Bson;

namespace Game
{
    public interface IMongoDocument
    {
        ObjectId ObjectId
        {
            get;
        }

        long Id
        {
            get;
        }
    }
}