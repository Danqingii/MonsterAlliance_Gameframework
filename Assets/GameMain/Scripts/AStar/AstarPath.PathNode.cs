using GameFramework;
using  UnityEngine;

namespace Game
{
    /// <summary>
    /// A星的路径解析
    /// A代表自己  B代表障碍物   需要到达C
    /// G = 从起点 A 移动到指定方格的移动代价，沿着到达该方格而生成的路径。
    /// H = 从指定的方格移动到终点 B 的估算成本。这个通常被称为试探法，直到我们找到了路径我们才会知道真正的距离，因为途中有各种各样的东西 ( 障碍等 ) 。
    ///我们的路径是这么产生的：反复遍历 OpenList ，选择 F 值最小的方格。
    /// </summary>
    public class PathNode : IReference
    {
        private bool m_IsBarrier;   //是否为障碍
        private Vector3 m_Position; //所在的世界坐标具体的位置
        
        public PathNode ParentNode
        {
            get; private set;
        }

        public float F
        {
            get; private set;
        }

        public float G
        {
            get; private set;
        }

        public float H
        {
            get; private set;
        }

        public int X
        {
            get; private set;
        }

        public int Y
        {
            get; private set;
        }

        public int Id
        {
            get; private set;
        }

        /// <summary>是否为障碍</summary>
        public bool IsBarrier
        {
            get => m_IsBarrier;
            set => m_IsBarrier = value;
        }
        
        /// <summary>物体的坐标 事件坐标</summary>
        public Vector3 Position
        {
            get => m_Position;
            set => m_Position = value;
        }
        
        /// <summary>
        /// 初始化节点
        /// </summary>
        /// <param name="x">地图所在的位置x</param>
        /// <param name="y">地图所在的位置y</param>
        /// <returns>初始化完成的节点</returns>
        public static PathNode Create(int id,int x, int y)
        {
            PathNode pathNode = ReferencePool.Acquire<PathNode>();
            pathNode.Id = id;
            pathNode.X = x;
            pathNode.Y = y;
            return pathNode;
        }

        /// <summary>
        /// 更新 父节点
        /// </summary>
        public void RefreshPargetNode(PathNode parentNode)
        {
            ParentNode = parentNode;
        }
        
        /// <summary>
        /// 更新 节点路径的代价
        /// </summary>
        public void RefreshPathCosts(float g)
        {
            G = g;
            F = G + H;
        }

        /// <summary>
        /// 更新 节点路径的代价
        /// </summary>
        public void RefreshPathCosts(float g, float h)
        {
            G = g;
            H = h;
            F = G + H;
        }

        public void Clear()
        {
            ParentNode = null;
            Position = default;
            IsBarrier = false;

            F = G = H = X = Y = 0;
            Id = -1;
        }
    }
}