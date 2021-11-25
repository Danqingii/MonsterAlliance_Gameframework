//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFramework.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        /// <summary>
        /// 下载计数器
        /// </summary>
        private sealed partial class DownloadCounter
        {
            //保存了一个计数器节点链表
            private readonly GameFrameworkLinkedList<DownloadCounterNode> m_DownloadCounterNodes;
            private float m_UpdateInterval;  //更新间隔
            private float m_RecordInterval;  //记录间隔
            private float m_CurrentSpeed;    //当前的更新速度
            private float m_Accumulator;     //累加器
            private float m_TimeLeft;        

            /// <summary>
            /// 初始化
            /// </summary>
            /// <param name="updateInterval">更新间隔时间(秒)</param>
            /// <param name="recordInterval">记录间隔时间(秒)</param>
            public DownloadCounter(float updateInterval, float recordInterval)
            {
                if (updateInterval <= 0f)
                {
                    throw new GameFrameworkException("Update interval is invalid.");
                }

                if (recordInterval <= 0f)
                {
                    throw new GameFrameworkException("Record interval is invalid.");
                }

                m_DownloadCounterNodes = new GameFrameworkLinkedList<DownloadCounterNode>();
                m_UpdateInterval = updateInterval;
                m_RecordInterval = recordInterval;
                Reset();
            }

            /// <summary>
            /// 更新间隔
            /// </summary>
            /// <exception cref="GameFrameworkException">指定值错误</exception>
            public float UpdateInterval
            {
                get
                {
                    return m_UpdateInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Update interval is invalid.");
                    }

                    m_UpdateInterval = value;
                    Reset();
                }
            }

            /// <summary>
            /// 记录间隔
            /// </summary>
            /// <exception cref="GameFrameworkException">指定值错误</exception>
            public float RecordInterval
            {
                get
                {
                    return m_RecordInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Record interval is invalid.");
                    }

                    m_RecordInterval = value;
                    Reset();
                }
            }

            public float CurrentSpeed
            {
                get
                {
                    return m_CurrentSpeed;
                }
            }

            public void Shutdown()
            {
                Reset();
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_DownloadCounterNodes.Count <= 0)
                {
                    return;
                }

                m_Accumulator += realElapseSeconds;
                if (m_Accumulator > m_RecordInterval)
                {
                    m_Accumulator = m_RecordInterval;
                }

                m_TimeLeft -= realElapseSeconds;
                foreach (DownloadCounterNode downloadCounterNode in m_DownloadCounterNodes)
                {
                    downloadCounterNode.Update(elapseSeconds, realElapseSeconds);
                }

                while (m_DownloadCounterNodes.Count > 0)
                {
                    DownloadCounterNode downloadCounterNode = m_DownloadCounterNodes.First.Value;
                    
                    //如果节点的消逝时间 小于当前的记录间隔
                    if (downloadCounterNode.ElapseSeconds < m_RecordInterval)
                    {
                        break;
                    }

                    //大于记录间隔了 归还
                    ReferencePool.Release(downloadCounterNode);
                    m_DownloadCounterNodes.RemoveFirst();
                }

                if (m_DownloadCounterNodes.Count <= 0)
                {
                    Reset();
                    return;
                }

                if (m_TimeLeft <= 0f)
                {
                    long totalDeltaLength = 0L;
                    foreach (DownloadCounterNode downloadCounterNode in m_DownloadCounterNodes)
                    {
                        totalDeltaLength += downloadCounterNode.DeltaLength;
                    }

                    m_CurrentSpeed = m_Accumulator > 0f ? totalDeltaLength / m_Accumulator : 0f;
                    m_TimeLeft += m_UpdateInterval;
                }
            }

            /// <summary>
            /// 创建一个 DownloadCounterNode节点
            /// </summary>
            /// <param name="deltaLength"></param>
            public void RecordDeltaLength(int deltaLength)
            {
                if (deltaLength <= 0)
                {
                    return;
                }

                DownloadCounterNode downloadCounterNode = null;
                if (m_DownloadCounterNodes.Count > 0)
                {
                    downloadCounterNode = m_DownloadCounterNodes.Last.Value;
                    if (downloadCounterNode.ElapseSeconds < m_UpdateInterval)
                    {
                        downloadCounterNode.AddDeltaLength(deltaLength);
                        return;
                    }
                }

                downloadCounterNode = DownloadCounterNode.Create();
                downloadCounterNode.AddDeltaLength(deltaLength);
                m_DownloadCounterNodes.AddLast(downloadCounterNode);
            }

            private void Reset()
            {
                m_DownloadCounterNodes.Clear();
                m_CurrentSpeed = 0f;
                m_Accumulator = 0f;
                m_TimeLeft = 0f;
            }
        }
    }
}
