using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// Mongo 数据库管理
    /// </summary>
    public class MongoManager
    {
        private IMongoClient m_MongoClient;
        private IMongoDatabase m_MongoDatabase;
        private readonly Dictionary<string, IMongoCollection<BsonDocument>> m_CollectionDict = new Dictionary<string, IMongoCollection<BsonDocument>>();

        public void Init()
        {
            string path = "mongodb://localhost:27017";
            m_MongoClient = new MongoClient(path);

            if (m_MongoClient == null)
            {
                Log.Error($"MongoDB 连接 '{path}' 失败.");
                return;
            }

            m_MongoDatabase = m_MongoClient.GetDatabase("怪物乐园");
            Log.Info($"MongoDB 连接 '{path}' 成功.");
        }

        //同步插入一个元素 到最后一行
        public void InsertOne(string tablet,BsonDocument bd)
        {
            GetCollection(tablet).InsertOne(bd);
        }
        
        //同步插入许多元素
        public void InsertMany(string tablet, IEnumerable<BsonDocument> bd)
        {
            GetCollection(tablet).InsertMany(bd);
        }
        
        //异步插入一个元素 到最后一行
        public async Task InsertOneAsync(string tablet, BsonDocument bd)
        {
            await GetCollection(tablet).InsertOneAsync(bd);
        }
        
        //异步插入许多元素
        public async Task InsertManyAsync(string tablet, IEnumerable<BsonDocument> bd)
        {
            await GetCollection(tablet).InsertManyAsync(bd);
        }

        private IMongoCollection<BsonDocument> GetCollection(string tabletName)
        {
            if (m_CollectionDict.ContainsKey(tabletName))
            {
                return m_CollectionDict[tabletName];
            }

            IMongoCollection<BsonDocument> collection = m_MongoDatabase.GetCollection<BsonDocument>(tabletName);
            m_CollectionDict.Add(tabletName,collection);
            return collection;
        }
    }
}