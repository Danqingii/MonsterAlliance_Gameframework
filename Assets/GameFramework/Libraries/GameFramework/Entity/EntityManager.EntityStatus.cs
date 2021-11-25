//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFramework.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        /// <summary>
        /// 实体状态。
        /// </summary>
        private enum EntityStatus : byte
        {
            /// <summary>
            /// 无
            /// </summary>
            Unknown = 0,
            
            /// <summary>
            /// 等待初始化
            /// </summary>
            WillInit,
            
            /// <summary>
            /// 已经初始化
            /// </summary>
            Inited,
            
            /// <summary>
            /// 等待刷新
            /// </summary>
            WillShow,
            
            /// <summary>
            /// 刷新完毕
            /// </summary>
            Showed,
            
            /// <summary>
            /// 等待关闭
            /// </summary>
            WillHide,
            
            /// <summary>
            /// 已经关闭
            /// </summary>
            Hidden,
            
            /// <summary>
            /// 等待回收
            /// </summary>
            WillRecycle,
            
            /// <summary>
            /// 已经回收
            /// </summary>
            Recycled
        }
    }
}