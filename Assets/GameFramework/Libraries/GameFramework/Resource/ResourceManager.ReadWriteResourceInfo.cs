//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace GameFramework.Resource
{
    internal sealed partial class ResourceManager : GameFrameworkModule, IResourceManager
    {
        /// <summary>
        /// 可读写的资源数据.
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        private struct ReadWriteResourceInfo
        {
            private readonly string m_FileSystemName;
            private readonly LoadType m_LoadType;
            private readonly int m_Length;
            private readonly int m_HashCode;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="fileSystemName">文件流名字</param>
            /// <param name="loadType">加载类型</param>
            /// <param name="length">资源长度</param>
            /// <param name="hashCode">哈希值</param>
            public ReadWriteResourceInfo(string fileSystemName, LoadType loadType, int length, int hashCode)
            {
                m_FileSystemName = fileSystemName;
                m_LoadType = loadType;
                m_Length = length;
                m_HashCode = hashCode;
            }

            public bool UseFileSystem
            {
                get
                {
                    return !string.IsNullOrEmpty(m_FileSystemName);
                }
            }

            public string FileSystemName
            {
                get
                {
                    return m_FileSystemName;
                }
            }

            public LoadType LoadType
            {
                get
                {
                    return m_LoadType;
                }
            }

            public int Length
            {
                get
                {
                    return m_Length;
                }
            }

            public int HashCode
            {
                get
                {
                    return m_HashCode;
                }
            }
        }
    }
}