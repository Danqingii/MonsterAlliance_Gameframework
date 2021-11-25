using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [AddComponentMenu("AStar/Pathfinding")]
    public sealed partial class AStarPath : MonoBehaviour
    {
        [SerializeField] private bool m_Is2D = false;
        [SerializeField] private int m_Width = 10;
        [SerializeField] private int m_Depth = 10;
        [SerializeField] private int m_NodeSize = 1;
        [SerializeField] private Vector3 m_Conter = Vector3.zero;
        [SerializeField] private Vector3 m_Rotation = Vector3.zero;
        [SerializeField] private bool m_IsDrawMesh = false;

        private PathNode[,] m_PathNodes;
        public static AStarPath Pathfinding;

        private void Awake()
        {
            Pathfinding = this;
        }

        public bool FindPath(PathNode start, PathNode end, PathNode[,] map, int mapWidth, int mapHeight)
        {
           return Profiler.FindPath(start,end,map,mapWidth,mapHeight);
        }
        
        public bool FindPath(PathNode start, PathNode end, List<PathNode> nodes)
        {
            return Profiler.FindPath(start,end,m_PathNodes,m_Width,m_Depth);
        }

        public void GetAllPathNode(List<PathNode> nodes)
        {
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Depth; y++)
                {
                    nodes.Add(m_PathNodes[x,y]);
                }
            }
        }

        public PathNode[] GetAllPathNode()
        {
            List<PathNode> nodes = new List<PathNode>();
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Depth; y++)
                {
                    nodes.Add(m_PathNodes[x,y]);
                }
            }
            return nodes.ToArray();
        }

        public void Scan()
        {
            //重新刷新
            Afresh();

            //绘制计算
            GizmosHeler.SetGizmosHeler(m_Is2D,m_Width,m_Depth,m_NodeSize,m_Conter,m_Rotation,m_IsDrawMesh);

            int serial = 0;
            for (int x = 0; x < m_Width; x++)
            {
                for (int y = 0; y < m_Depth; y++)
                {
                    PathNode node = PathNode.Create(serial,x, y);
                    node.Position = GizmosHeler.CalcPosition(x,y);
                    m_PathNodes[x, y] = node;
                    serial += 1;
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            GizmosHeler.SetGizmosHeler(m_Is2D,m_Width,m_Depth,m_NodeSize,m_Conter,m_Rotation,m_IsDrawMesh);
            GizmosHeler.DrawGizmos();
        }

        private void Afresh()
        {
            if (m_PathNodes != null)
            {
                foreach (var node in m_PathNodes)
                {
                    node.Clear();
                }
            }
            m_PathNodes = new PathNode[m_Width, m_Depth];
        }

        private void OnDestroy()
        {
            if (m_PathNodes != null)
            {
                foreach (var node in m_PathNodes)
                {
                    node.Clear();
                }
            }
        }
    }
}