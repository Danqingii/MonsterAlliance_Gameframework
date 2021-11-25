using System.Collections.Generic;
using GameFramework;
using MongoDB.Bson;
using UnityGameFramework.Runtime;
using UnityEngine;
using MongoDB.Driver;

namespace Game
{
    public sealed class MongoDBComponent : GameFrameworkComponent
    {
        [SerializeField] private ConnectionType m_ConnectionModel = ConnectionType.None;

        //位置
        private readonly string m_Location = string.Empty;
        private IMongoClient m_MongoClient = null;
        private IMongoDatabase m_MongoDatabase = null;
        private Dictionary<string, IMongoCollection<IMongoDataBase>> m_CollectionDict;

        protected override void Awake()
        {
            base.Awake();
            m_CollectionDict = new Dictionary<string, IMongoCollection<IMongoDataBase>>();
        }

        public void Start()
        {
            if (m_ConnectionModel == ConnectionType.None)
            {
                return;
            }

            string path;
            if (m_ConnectionModel == ConnectionType.LocalHost)
            {
                path = "mongodb://localhost:27017";
            }
            else
            {
                path = $"mongodb://{m_Location}";
            }

            m_MongoClient = new MongoClient(path); //尝试连接数据库
            if (m_MongoClient == null)
            {
                Log.Error("MongoDB Establish a connection with '{0}' Failed.", path);
                return;
            }

            m_MongoDatabase = m_MongoClient.GetDatabase("Demo"); //连接主表
            Log.Info("MongoDB Establish a connection with '{0}' succeed.", path);
        }

        public List<IMongoDataBase> FindAll(string collectionName)
        {
            IMongoCollection<IMongoDataBase> collection = IntimateGetCollection(collectionName);

            List<IMongoDataBase> target = new List<IMongoDataBase>();
            try
            {
                FilterDefinition<IMongoDataBase> filter = Builders<IMongoDataBase>.Filter.Empty;
                target = collection.Find(filter).ToList<IMongoDataBase>();
            }
            catch (GameFrameworkException ex)
            {
                Log.Error("{0}: FindAll '{1}'", collectionName, ex);
            }

            return target;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="mongoData">具体的数据</param>
        public bool InsertData<T>(string collectionName, T mongoData) where T : IMongoDataBase
        {
            return InsertData(collectionName, (IMongoDataBase) mongoData);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="mongoData">具体的数据</param>
        public bool InsertData(string collectionName, IMongoDataBase mongoData)
        {
            if (m_MongoClient == null)
            {
                Log.Error("MongoClient not connected ,Unable to insert data.");
                return false;
            }

            IMongoCollection<IMongoDataBase> collection = IntimateGetCollection(collectionName);
            try
            {
                collection.InsertOne(mongoData);
                return true;
            }
            catch (GameFrameworkException ex)
            {
                Log.Error("{0}: InsertData '{1}'", collectionName, ex);
                return false;
            }
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="mongoDatas"></param>
        public bool InsertManyData<T>(string collectionName, params T[] mongoDatas) where T : IMongoDataBase
        {
           return InsertManyData(collectionName, mongoDatas as IMongoDataBase);
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="mongoDatas"></param>
        public bool InsertManyData(string collectionName, params IMongoDataBase[] mongoDatas)
        {
            if (m_MongoClient == null)
            {
                Log.Error("MongoClient not connected ,Unable to insert data.");
                return false;
            }

            IMongoCollection<IMongoDataBase> collection = IntimateGetCollection(collectionName);
            try
            {
                collection.InsertMany(mongoDatas);
                return true;
            }
            catch (GameFrameworkException ex)
            {
                Log.Error("{0}: InsertData '{1}'", collectionName, ex);
                return false;
            }
        }

        private IMongoCollection<IMongoDataBase> IntimateGetCollection(string collectionName)
        {
            if (m_CollectionDict.TryGetValue(collectionName, out var collections))
            {
                return collections;
            }

            collections = m_MongoDatabase.GetCollection<IMongoDataBase>(collectionName);
            return collections;
        }
    }
}