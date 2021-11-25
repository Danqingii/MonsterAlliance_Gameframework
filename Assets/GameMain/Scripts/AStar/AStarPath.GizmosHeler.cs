using UnityEngine;

namespace Game
{
    public sealed partial class AStarPath : MonoBehaviour
    {
        /// <summary>
        /// 绘制帮助
        /// </summary>
        private static class GizmosHeler
        {
            private static bool s_Is2D;
            private static int s_Width; //宽度
            private static int s_Depth; //深度
            private static int s_NodeSize; //地图大小
            private static Vector3 s_Conter; //地图的中心点

            private static float s_NodeSizePffset;      //节点大小的偏移量  mash最终点位的地方
            private static float s_WidthOffset;         //地图宽度偏移  X
            private static float s_DepthOffset;         //地图深度偏移  Y
            private static Quaternion s_RotationOffset; //地图旋转  

            private static bool s_IsDrawMash;

            internal static void SetGizmosHeler(bool is2D, int width, int depth, int nodeSize, Vector3 conter,
                Vector3 rotation, bool isDrawMash)
            {
                s_Is2D = is2D;
                s_Width = width;
                s_Depth = depth;
                s_NodeSize = nodeSize;
                s_Conter = conter;
                s_IsDrawMash = isDrawMash;

                s_NodeSizePffset = -0.5f;
                s_WidthOffset = (float) s_Width / 2;
                s_DepthOffset = (float) s_Depth / 2;
                s_RotationOffset = Quaternion.Euler(rotation);
            }

            internal static Vector3 CalcPosition(int x, int y)
            {
                //x = 地图偏移量 + x + 大小偏移量
                //y = 地图偏移量 + y + 大小偏移量
                //乘以一个地图大小
                //在加上一个地图的偏移量
                //最后在乘以一个 旋转
                //得到矩阵
                if (s_Is2D)
                    return s_RotationOffset * new Vector3(-s_WidthOffset + x - s_NodeSizePffset,-s_DepthOffset + y - s_NodeSizePffset, 0) * s_NodeSize + s_Conter;
                else
                    return s_RotationOffset * new Vector3(-s_WidthOffset + x - s_NodeSizePffset, 0,-s_DepthOffset + y - s_NodeSizePffset) * s_NodeSize + s_Conter;
            }

            /// <summary> 画行 开始坐标系</summary>
            private static Vector3 CalcYRowStart(int x)
            {
                if (s_Is2D)
                    return s_RotationOffset * new Vector3(-s_WidthOffset, -s_DepthOffset + x, 0) * s_NodeSize + s_Conter;
                else
                    return s_RotationOffset * new Vector3(-s_WidthOffset, 0, -s_DepthOffset + x) * s_NodeSize +  s_Conter;
            }

            /// <summary> 画行 结束坐标系</summary>
            private static Vector3 CalcYRowEnd(int x)
            {
                if (s_Is2D)
                    return s_RotationOffset * new Vector3(s_WidthOffset, -s_DepthOffset + x, 0) * s_NodeSize + s_Conter;
                else
                    return s_RotationOffset * new Vector3(s_WidthOffset, 0, -s_DepthOffset + x) * s_NodeSize + s_Conter;
            }

            /// <summary> 画列 开始坐标系</summary>
            private static Vector3 CalcXColumnStart(int y)
            {
                if (s_Is2D)
                    return s_RotationOffset * new Vector3(-s_WidthOffset + y, s_DepthOffset, 0) * s_NodeSize + s_Conter;
                else
                    return s_RotationOffset * new Vector3(-s_WidthOffset + y, 0, s_DepthOffset) * s_NodeSize + s_Conter;
            }

            /// <summary> 画列 结束坐标系</summary>
            private static Vector3 CalcXColumnEnd(int y)
            {
                if (s_Is2D)
                    return s_RotationOffset * new Vector3(-s_WidthOffset + y, -s_DepthOffset, 0) * s_NodeSize +
                           s_Conter;
                else
                    return s_RotationOffset * new Vector3(-s_WidthOffset + y, 0, -s_DepthOffset) * s_NodeSize +
                           s_Conter;
            }

            //绘制 图片的线路
            internal static void DrawGizmos()
            {
                //画横
                for (int y = 0; y <= s_Depth; y++)
                {
                    Vector3 start = CalcYRowStart(y);
                    Vector3 end = CalcYRowEnd(y);

                    Gizmos.DrawLine(start, end);
                }

                //画列
                for (int x = 0; x <= s_Width; x++)
                {
                    Vector3 start = CalcXColumnStart(x);
                    Vector3 endPoint = CalcXColumnEnd(x);

                    Gizmos.DrawLine(start, endPoint);
                }

                //画Mesh
                if (s_IsDrawMash)
                {
                    for (int x = 0; x < s_Width; x++)
                    {
                        for (int y = 0; y < s_Depth; y++)
                        {
                            Gizmos.color = Color.white;
                            Vector3 meshPos = CalcPosition(x, y);
                            Gizmos.DrawWireCube(meshPos, (Vector3.one * 0.4f) * s_NodeSize);
                        }
                    }
                }
            }
        }
    }
}