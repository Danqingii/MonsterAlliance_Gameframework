//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Download;
using System;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif

namespace UnityGameFramework.Runtime
{
    public sealed partial class UnityWebRequestDownloadAgentHelper : DownloadAgentHelperBase, IDisposable
    {
        /// <summary>
        ///  管理和处理从远程服务器接收的 HTTP 响应体数据
        /// </summary>
        private sealed class DownloadHandler : DownloadHandlerScript
        {
            private readonly UnityWebRequestDownloadAgentHelper m_Owner;

            public DownloadHandler(UnityWebRequestDownloadAgentHelper owner)
                : base(owner.m_CachedBytes)
            {
                m_Owner = owner;
            }

            /// <summary>
            /// 从远程服务器收到数据时调用的回调。
            /// </summary>
            /// <param name="data">包含从远程服务器收到的未处理的数据的缓冲区。</param>
            /// <param name="dataLength">data 中新增的字节数。</param>
            /// <returns>bool 如果下载继续，则为 true，如果中止下载，则为 false</returns>
            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (m_Owner != null && m_Owner.m_UnityWebRequest != null && dataLength > 0)
                {
                    DownloadAgentHelperUpdateBytesEventArgs downloadAgentHelperUpdateBytesEventArgs = DownloadAgentHelperUpdateBytesEventArgs.Create(data, 0, dataLength);
                    m_Owner.m_DownloadAgentHelperUpdateBytesEventHandler(this, downloadAgentHelperUpdateBytesEventArgs);
                    ReferencePool.Release(downloadAgentHelperUpdateBytesEventArgs);

                    DownloadAgentHelperUpdateLengthEventArgs downloadAgentHelperUpdateLengthEventArgs = DownloadAgentHelperUpdateLengthEventArgs.Create(dataLength);
                    m_Owner.m_DownloadAgentHelperUpdateLengthEventHandler(this, downloadAgentHelperUpdateLengthEventArgs);
                    ReferencePool.Release(downloadAgentHelperUpdateLengthEventArgs);
                }

                return base.ReceiveData(data, dataLength);
            }
        }
    }
}