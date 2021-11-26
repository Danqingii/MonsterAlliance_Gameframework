//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// 心跳状态
        /// </summary>
        private sealed class HeartBeatState
        {
            private float m_HeartBeatElapseSeconds;
            private int m_MissHeartBeatCount;

            public HeartBeatState()
            {
                m_HeartBeatElapseSeconds = 0f;
                m_MissHeartBeatCount = 0;
            }

            /// <summary>
            /// 心跳包 流逝的时间
            /// </summary>
            public float HeartBeatElapseSeconds
            {
                get
                {
                    return m_HeartBeatElapseSeconds;
                }
                set
                {
                    m_HeartBeatElapseSeconds = value;
                }
            }

            /// <summary>
            /// 未成功的心跳包数量
            /// </summary>
            public int MissHeartBeatCount
            {
                get
                {
                    return m_MissHeartBeatCount;
                }
                set
                {
                    m_MissHeartBeatCount = value;
                }
            }

            /// <summary>
            /// 重置心跳包
            /// </summary>
            /// <param name="resetHeartBeatElapseSeconds"></param>
            public void Reset(bool resetHeartBeatElapseSeconds)
            {
                if (resetHeartBeatElapseSeconds)
                {
                    m_HeartBeatElapseSeconds = 0f;
                }

                m_MissHeartBeatCount = 0;
            }
        }
    }
}