//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IReference
        {
            private IEntity m_Entity;              //具体的实体
            private EntityStatus m_Status;         //实体的状态
            private IEntity m_ParentEntity;        //父亲实体
            private readonly List<IEntity> m_ChildEntities; //子 集合实体

            public EntityInfo()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities = new List<IEntity>();
            }

            /// <summary>
            /// 自身实体
            /// </summary>
            public IEntity Entity
            {
                get
                {
                    return m_Entity;
                }
            }

            /// <summary>
            /// 自身状态
            /// </summary>
            public EntityStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            /// <summary>
            /// 父物体实体
            /// </summary>
            public IEntity ParentEntity
            {
                get
                {
                    return m_ParentEntity;
                }
                set
                {
                    m_ParentEntity = value;
                }
            }

            /// <summary>
            /// 子 实体数量
            /// </summary>
            public int ChildEntityCount
            {
                get
                {
                    return m_ChildEntities.Count;
                }
            }

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = ReferencePool.Acquire<EntityInfo>();
                entityInfo.m_Entity = entity;
                entityInfo.m_Status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities.Clear();
            }

            /// <summary>
            /// 获取子实体 只返回第一个
            /// </summary>
            public IEntity GetChildEntity()
            {
                return m_ChildEntities.Count > 0 ? m_ChildEntities[0] : null;
            }

            /// <summary>
            /// 获取子实体集合
            /// </summary>
            /// <returns></returns>
            public IEntity[] GetChildEntities()
            {
                return m_ChildEntities.ToArray();
            }

            /// <summary>
            /// 获取子实体List
            /// </summary>
            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in m_ChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            /// <summary>
            /// 添加一个子实体
            /// </summary>
            public void AddChildEntity(IEntity childEntity)
            {
                if (m_ChildEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                m_ChildEntities.Add(childEntity);
            }

            /// <summary>
            /// 移除指定子实体
            /// </summary>
            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!m_ChildEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
