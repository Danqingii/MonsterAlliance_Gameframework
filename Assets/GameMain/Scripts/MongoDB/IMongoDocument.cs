using MongoDB.Bson;

namespace Game
{
    interface IMongoDocument
    {
        ObjectId Id { get;}
    }
}