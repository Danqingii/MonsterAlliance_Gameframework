using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public sealed partial class AStarPath : MonoBehaviour
    {
        /// <summary>
        /// A星寻路 分析器
        /// 实现原理
        /// 1、首先有一张一定宽高的地图 （定义好 Point 点的地图，其中 Point 中有 IsBarrier 属性）
        /// 2、设定开始点，和目标点
        /// 3、传入 FindPath 开始寻找较短路径，找到返回true，否则 false
        /// 4、为 true 就可以通过 目标点的父亲点的父亲点的父亲点，直到父亲点为开始点，这些点集合即是路径
        /// 5、FindPath 寻找原理
        /// 1）开列表，关列表初始化
        /// 2）添加开始点到开列表，然后获得周围点集合，接着又把开始点从开列表中移除，并添加到关列表
        /// 3）判断这些周围点集合是否已经在开列表中，不在则更新这些点的F 和 父亲点，并添加到开列表；再则重新计算G值，G较小则更新GF 和父亲点
        /// 4）从周围点集合中找到 F 最小的点，然后获得周围点集合，接着又把找到 F 最小的点从开列表中移除，并添加到关列表
        /// 5）接着执行第 3） 步骤
        /// 6）直到目标点被添加到开列表中，则路径找到
        /// 7）否则，直到开列表中没有了数据，则说明没有合适路径
        /// </summary>
        private static class Profiler
        {
            // 开列表（周围的点列表），关列表（每次比较后得到的最小 F 的点列表）
            private static readonly List<PathNode> m_OpenList = new List<PathNode>();
            private static readonly List<PathNode> m_CloseList = new List<PathNode>();

            /// <summary>
            /// 根据开始点和结束点，取得较短路径
            /// </summary>
            /// <param name="start">开始位置</param>
            /// <param name="end">结束位置</param>
            /// <param name="map">地图</param>
            /// <param name="mapWidth">地图宽</param>
            /// <param name="mapHeight">地图高</param>
            /// <returns>bool 找到路线/false 没有</returns>
            public static bool FindPath(PathNode start, PathNode end, PathNode[,] map, int mapWidth, int mapHeight)
            {
                InteriorClearNode();

                // 首先开始点添加进开列表
                m_OpenList.Add(start);
                while (m_OpenList.Count > 0)
                {
                    // 寻找开列表中最小的 F 值 也就是startNode下的所最优节点
                    PathNode optimalNode = FindOptimalNode(m_OpenList);

                    // 把得到的 F 最小的点移除 开列表，添加到关列表中
                    m_OpenList.Remove(optimalNode);
                    m_CloseList.Add(optimalNode);

                    // 获取当前最小 F 点周围的点集合 
                    List<PathNode> surroundNodes = GetSurroundPathNode(optimalNode, map, mapWidth, mapHeight);

                    // 核查过滤掉关列表中的数据
                    CheckFilterNode(surroundNodes, m_CloseList);

                    //过滤掉不合法的周围点 才是真正的节点
                    foreach (PathNode node in surroundNodes)
                    {
                        // 已存在开列表中的话
                        if (m_OpenList.IndexOf(node) > -1)
                        {
                            // 计算 G 值
                            float newG = CalcG(node, optimalNode);

                            // 得到的 G 值小的话，更新 G 值和点的父节点
                            if (newG < node.G)
                            {
                                node.RefreshPargetNode(optimalNode);
                                node.RefreshPathCosts(newG);
                            }
                        }
                        // 不在开列表中
                        else 
                        {
                            //把 最小 F 点，设置为他的父节点
                            node.RefreshPargetNode(optimalNode);
                            // 计算更新 item的 FGH
                            CalcF(node, end);
                            // 添加到 开列表
                            m_OpenList.Add(node);
                        }

                    }

                    // 判断 end 是否在 开列表中，即找到路径结束
                    if (m_OpenList.IndexOf(end) > -1)
                    {
                        return true;
                    }
                }

                //开列表没有了点，说明找不到路径
                return false;
            }


            /// <summary>
            /// 把关列表中的点从周围点集合中过滤掉
            /// </summary>
            /// <param name="src">周围点集合</param>
            /// <param name="closeList">关列表</param>
            private static void CheckFilterNode(List<PathNode> src, List<PathNode> closeList)
            {
                // 遍历，存在则移除
                foreach (PathNode item in closeList)
                {
                    if (src.IndexOf(item) > -1)
                    {
                        src.Remove(item);
                    }
                }
            }

            /// <summary>
            /// 获得当前最小 F 的周围点
            /// </summary>
            /// <param name="pathNode">当前最小 F 点</param>
            /// <param name="map">地图点集合</param>
            /// <param name="mapWidth">地图宽</param>
            /// <param name="mapHeight">地图高</param>
            /// <returns>返回获得的周围点集合</returns>
            private static List<PathNode> GetSurroundPathNode(PathNode pathNode, PathNode[,] map, int mapWidth, int mapHeight)
            {
                // 一个点一般会有上、下、左、右，左上、左下、右上、右下 八个点
                PathNode up = null, down = null, left = null, right = null;
                PathNode lu = null, ru = null, ld = null, rd = null;

                // 如果 点 Y 小于 mapHeight - 1，则表明不是最顶端，有上点
                if (pathNode.Y < mapHeight - 1)
                {
                    up = map[pathNode.X, pathNode.Y + 1];
                }

                // 如果 点 Y 大于 0，则表明不是最下端，有下点
                if (pathNode.Y > 0)
                {
                    down = map[pathNode.X, pathNode.Y - 1];
                }

                // 如果 点 X 小于 mapWidth - 1，则表明不是最右端，有右点
                if (pathNode.X < mapWidth - 1)
                {
                    right = map[pathNode.X + 1, pathNode.Y];
                }

                // 如果 点 X 大于 0，则表明不是最左端，有左点
                if (pathNode.X > 0)
                {
                    left = map[pathNode.X - 1, pathNode.Y];
                }

                // 边角点
                // 有上点和左点，说明有左上点
                if (up != null && left != null)
                {
                    lu = map[pathNode.X - 1, pathNode.Y + 1];
                }

                // 有上点和右点，说明有右上点
                if (up != null && right != null)
                {
                    ru = map[pathNode.X + 1, pathNode.Y + 1];
                }

                // 有下点和左点，说明有左下点
                if (down != null && left != null)
                {
                    ld = map[pathNode.X - 1, pathNode.Y - 1];
                }

                // 有下点和右点，说明有右下点
                if (down != null && right != null)
                {
                    rd = map[pathNode.X + 1, pathNode.Y - 1];
                }

                // 新建一个列表
                List<PathNode> list = new List<PathNode>();
                // 把点添加到列表
                // 上点不为空，且不是障碍物，则添加到返回列表中，下面同理
                if (up != null && up.IsBarrier == false)
                {
                    list.Add(up);
                }

                if (down != null && down.IsBarrier == false)
                {
                    list.Add(down);
                }

                if (left != null && left.IsBarrier == false)
                {
                    list.Add(left);
                }

                if (right != null && right.IsBarrier == false)
                {
                    list.Add(right);
                }

                // 添加边角到列表
                // 左上点不为空且不是障碍物，并且 左点和上点都不是障碍物，则添加到返回列表，以下同理
                if (lu != null && lu.IsBarrier == false && left.IsBarrier == false && up.IsBarrier == false)
                {
                    list.Add(lu);
                }

                if (ru != null && ru.IsBarrier == false && right.IsBarrier == false && up.IsBarrier == false)
                {
                    list.Add(ru);
                }

                if (ld != null && ld.IsBarrier == false && left.IsBarrier == false && down.IsBarrier == false)
                {
                    list.Add(ld);
                }

                if (rd != null && rd.IsBarrier == false && right.IsBarrier == false && down.IsBarrier == false)
                {
                    list.Add(rd);
                }

                // 返回列表
                return list;

            }

            /// <summary>
            /// 比较开列表中所有点的 F 值，获得当前列表中最小的点
            /// </summary>
            /// <param name="openList">开列表</param>
            /// <returns>最优的节点</returns>
            private static PathNode FindOptimalNode(List<PathNode> openList)
            {
                float f = float.MaxValue;
                PathNode cur = null;

                //找到代价最小的F 节点
                foreach (PathNode node in openList)
                {
                    if (node.F < f)
                    {
                        cur = node;
                        f = node.F;
                    }
                }

                return cur;
            }

            /// <summary>
            /// 计算 G 值
            /// </summary>
            /// <param name="cur">当前点</param>
            /// <param name="parent">当前点的父节点</param>
            /// <returns></returns>
            private static float CalcG(PathNode cur, PathNode parent)
            {
                return Vector2.Distance(new Vector2(cur.X, cur.Y), new Vector2(parent.X, parent.Y)) + parent.G;
            }

            /// <summary>
            /// 计算 F 值
            /// </summary>
            /// <param name="cur">当前点</param>
            /// <param name="end">结束点</param>
            private static void CalcF(PathNode cur, PathNode end)
            {
                // F = G + H
                // H 值的计算方式不唯一，有意义就行，这里是结束点X和Y的和
                float h = Mathf.Abs(end.X - cur.X) + Mathf.Abs(end.Y - cur.Y);
                float g = 0;
                if (cur.ParentNode == null)
                {
                    g = 0;
                }
                else
                {
                    // 当前点和父节点的距离
                    g = Vector2.Distance(new Vector2(cur.X, cur.Y), new Vector2(cur.ParentNode.X, cur.ParentNode.Y)) +
                        cur.ParentNode.G;
                }

                //更新当前点 F G H 的值
                cur.RefreshPathCosts(g,h);
            }

            //内部回收节点
            private static void InteriorClearNode()
            {
                m_OpenList.Clear();
                m_CloseList.Clear();
            }
        }
    }
}