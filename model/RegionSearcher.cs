using Autodesk.AutoCAD.Geometry;
using PDPArxCommon.DataStructure.Compareres;
using PDPArxCommon.DataStructure.Topology2d;
using PDPNetCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using PDPNetCommon.DataStructure;
using PDPNetCommon.DataStructure.Indices;

namespace PDPArxCommon.Arithmetic
{
    public class RegionSearcher
    {

        /// <summary>
        /// all single loop + outLoop
        /// </summary>
        /// <returns></returns>
        public static TModel GetModel(IEnumerable<Curve2d> edges, Tolerance tolerance)
        {
            RegionSearcher regionSearcher = new RegionSearcher();
            regionSearcher._input = edges;
            regionSearcher._tolerance = tolerance;
            if (regionSearcher.Process())
            {
                return regionSearcher.Result;
            }
            return null;
        }
        /// <summary>
        /// 计算连续的边
        /// </summary>
        public static List<List<TTrim>> GetContinuousContourEdges(TModel tModel)
        {
            // 存储结果
            List<List<TTrim>> rt = new List<List<TTrim>>();
            // 获取逆时针的环
            var loops1 = tModel.GetAnticlockwiseLoops();
            // 遍历每一个环
            foreach (var loop in loops1)
            {
                // 存储子结果
                List<TTrim> subRt = new List<TTrim>();
                // 遍历每一个半边
                foreach (var trimIndex in loop.TrimIndices)
                {
                    // 将半边添加到子结果列表中
                    subRt.Add(tModel.Trims[trimIndex]);
                }

                // 将子结果列表添加到结果列表中
                rt.Add(subRt);
            }
            // 创建一个字典，用于存储顶点映射
            Dictionary<int,int> vMap = new Dictionary<int, int>();
            // 创建一个字典，用于存储顶点映射1
            Dictionary<int,int> vMap1 = new Dictionary<int, int>();
            // 创建一个字典，用于存储边映射
            Dictionary<Index2,int> eMap = new Dictionary<Index2, int>();
            // 遍历每一个分割线
            foreach (var dissociativeEdge in tModel.GetDissociativeEdges())
            {
                // 创建一个索引2，用于存储分割线的两个顶点
                Index2 index2 = new Index2(dissociativeEdge.StartVertexIndex, dissociativeEdge.EndVertexIndex);
                // 如果边映射中已经包含该分割线，则跳过
                if (eMap.ContainsKey(index2))
                {
                    continue;
                }
                // 将分割线添加到边映射中
                eMap.Add(index2, dissociativeEdge.EdgeIndex);
                // 如果顶点映射中不包含分割线的第一个顶点，则添加到顶点映射中
                if (!vMap.ContainsKey(index2.I1))
                {
                    int newIndex = vMap.Count;
                    vMap.Add(index2.I1, newIndex);
                    vMap1.Add(newIndex, index2.I1);
                }
                // 如果顶点映射中不包含分割线的第二个顶点，则添加到顶点映射中
                if (!vMap.ContainsKey(index2.I2))
                {
                    int newIndex = vMap.Count;
                    vMap.Add(index2.I2, newIndex);
                    vMap1.Add(newIndex, index2.I2);
                }
            }

            // 创建一个图，用于存储顶点映射
            Dfs.Graph graph = new Dfs.Graph(new int[vMap.Count, vMap.Count], vMap.Count);
            // 遍历边映射
            foreach (var item in eMap)
            {
                // 获取分割线的两个顶点的顶点索引
                int v1 = vMap[item.Key.I1];
                int v2 = vMap[item.Key.I2];
                // 将顶点索引添加到图中
                graph.Map[v1, v2] = 1;
                graph.Map[v2, v1] = 1;
            }

            // 遍历图中每一个分割线
            foreach (var dissociativeEdges in graph.FindDissociative())
            {
                // 创建一个空的列表，用于存储子结果
                List<TTrim> subRt = new List<TTrim>();
                // 遍历分割线的每一个分割线
                for (int i = 0; i < dissociativeEdges.Count - 1; i++)
                {
                    // 获取分割线的两个顶点的顶点索引
                    int v1 = vMap1[dissociativeEdges[i]];
                    int v2 = vMap1[dissociativeEdges[i + 1]];
                    // 创建一个索引2，用于存储分割线的两个顶点
                    Index2 index2 = new Index2(v1,v2);
                    // 创建一个布尔值，用于标记分割线是否被反转
                    bool isReserve = false;
                    // 如果边映射中不包含分割线的两个顶点，则将分割线的两个顶点反转
                    if (!eMap.ContainsKey(index2))
                    {
                        isReserve = true;
                        index2 = new Index2(v2, v1);
                    }

                    // 获取分割线的边索引
                    var edgeIndex = eMap[index2];
                    // 创建一个分割线索引，用于存储分割线的边索引和是否被反转
                    TrimIndex trimIndex = new TrimIndex(edgeIndex, isReserve);
                    // 将分割线添加到子结果列表中
                    subRt.Add(tModel.Trims[trimIndex]);
                }

                // 将子结果列表添加到结果列表中
                rt.Add(subRt);
            }
            // 返回结果列表
            return rt;
        }

        protected RegionSearcher()
        {

        }

        /// <summary>
        /// 输入
        /// </summary>
        private IEnumerable<Curve2d> _input;
        /// <summary>
        /// 公差
        /// </summary>
        private Tolerance _tolerance;
        /// <summary>
        /// 结果
        /// </summary>
        public TModel Result;

        #region algorithm

        private bool Process()
        {
            if (_input is null)
            {
                return false;
            }

            //1.创建模型
            if (!CreateModel())
            {
                return false;
            }

            // 2. 创建节点
            if (!CreateNodeDict())
            {
                return false;
            }

            //3.搜索环
            if (!FindLoop())
            {
                return false;
            }

            return true;
        }

        private Dictionary<int, List<TTrim>> _vertexTrimDict;
        /// <summary>
        /// 数据预处理（缓存顶点、半边及排序信息）
        /// </summary>
        /// <returns></returns>
        private bool CreateNodeDict()
        {
            // 创建一个字典，用于存储顶点索引和边列表的映射关系
            _vertexTrimDict = new Dictionary<int, List<TTrim>>();
            // 遍历所有的边
            foreach (var resultTrim in Result.Trims)
            {
                // 如果字典中不包含边的起始顶点索引，则添加该索引
                if (!_vertexTrimDict.ContainsKey(resultTrim.Value.StartVertexIndex))
                {
                    _vertexTrimDict.Add(resultTrim.Value.StartVertexIndex, new List<TTrim>());
                }

                // 将边添加到字典中
                _vertexTrimDict[resultTrim.Value.StartVertexIndex].Add(resultTrim.Value);
            }

            // 创建一个比较器，用于比较两个边是否相等
            var comparer = new TrimComparer(_tolerance.EqualVector);
            // 遍历字典中的每一个元素
            foreach (var item in _vertexTrimDict)
            {
                // 对每一个元素进行排序
                item.Value.Sort(comparer);
            }
            return true;
        }
        /// <summary>
        /// 已处理的半边
        /// </summary>
        private SortedSet<TrimIndex> _processedTrimIndices;
        /// <summary>
        /// 非流形边
        /// </summary>
        private SortedSet<int> _nonManifoldEdgeIndices;
        /// <summary>
        /// 搜索环
        /// </summary>
        /// <returns></returns>
        private bool FindLoop()
        {
            // 创建一个排序集合，用于存储已经处理过的半边索引
            _processedTrimIndices = new SortedSet<TrimIndex>();
            // 创建一个排序集合，用于存储非非流形半边索引
            _nonManifoldEdgeIndices = new SortedSet<int>();
            // 查找自环情况
            FindSelf();
            // 找到第一个半边索引
            while (GetAnyTrimIndex(out var first))
            {
                // 查找环
                if (!FindLoop_Work(first))
                {
                    // 如果没有找到环，返回 false
                    return false;
                }
            }

            // 返回 true
            return true;
        }
        /// <summary>
        /// 搜索自环（边的起点等于终点）
        /// </summary>
        private void FindSelf()
        {
            foreach (var item in Result.Trims)
            {
                var edge = Result.Edges[item.Value.EdgeIndex];
                if (edge.StartVertexIndex == edge.EndVertexIndex)
                {
                    Result.AddLoop(new[] {item.Key});
                    _processedTrimIndices.Add(item.Key);
                }
            }
        }
        /// <summary>
        /// 找到任意一条半边
        /// </summary>
        /// <param name="trim"></param>
        /// <returns></returns>
        private bool GetAnyTrimIndex(out TTrim trim)
        {
            trim = null;
            foreach (var resultTrim in Result.Trims)
            {
                if (_nonManifoldEdgeIndices.Contains(resultTrim.Key.Index))
                {
                    continue;
                }
                if (_processedTrimIndices.Contains(resultTrim.Key))
                {
                    continue;
                }

                trim = resultTrim.Value;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 搜索环工作函数（由一条半边出发、搜索环）
        /// </summary>
        /// <param name="startHalfEdge">起始半边</param>
        /// <returns></returns>
        private bool FindLoop_Work(TTrim startHalfEdge)
        {
            //创建一个列表，用于存储半边
            List<TTrim> trims = new List<TTrim>();
            //将起始半边添加到列表中
            trims.Add(startHalfEdge);
            //搜索
            while (FindLoop_Work(trims))
            {

            }

            //如果找到一个环
            if (IsClosed(trims))
            {
                //获取非流形半边
                var nonManifoldEdgeIndices = GetNonManifoldHalfEdges(trims);
                //如果非流形半边数量大于0，则表示找到非流形环
                if (nonManifoldEdgeIndices.Count > 0)
                {
                    //非流形环
                    //去除所有非流型半边后继续搜索
                    //TODO: 找到所有非流型半边并去除
                    //去除所有非流型半边后继续搜索
                    foreach (var edgeIndex in nonManifoldEdgeIndices)
                    {
                        _nonManifoldEdgeIndices.Add(edgeIndex);
                    }
                }
                else
                {
                    //如果找到一个流形环，则返回true
                    List<TrimIndex> trimIndices = new List<TrimIndex>();
                    //遍历半边，获取半边的索引
                    foreach (var trim in trims)
                    {
                        trimIndices.Add(trim.TrimIndex);
                    }

                    //将流形环添加到结果中
                    Result.AddLoop(trimIndices);
                    //遍历半边，将半边的索引添加到已处理半边索引中
                    foreach (var trimIndex in trimIndices)
                    {
                        _processedTrimIndices.Add(trimIndex);
                    }
                }
                return true;
            }
            else
            {
                //如果找到一个流形环，则返回true
                return false;
            }
        }
        /// <summary>
        /// 是否为环
        /// </summary>
        private bool IsClosed(List<TTrim> trims)
        {
            if (trims.Count < 2)
            {
                return false;
            }

            return trims.First().StartVertexIndex == trims.Last().EndVertexIndex;
        }
        /// <summary>
        /// 获取半边集合内的非流型边
        /// </summary>
        private List<int> GetNonManifoldHalfEdges(List<TTrim> trims)
        {
            Dictionary<int, List<TTrim>> stat = new Dictionary<int, List<TTrim>>();
            foreach (var trim in trims)
            {
                if (!stat.ContainsKey(trim.EdgeIndex))
                {
                    stat.Add(trim.EdgeIndex, new List<TTrim>());
                }

                stat[trim.EdgeIndex].Add(trim);
            }
            var nonManifoldEdges = new List<int>();
            foreach (var item in stat)
            {
                if (item.Value.Count > 1)
                {
                    nonManifoldEdges.Add(item.Key);
                }
            }

            return nonManifoldEdges;
        }
        /// <summary>
        /// 搜索环
        /// </summary>
        private bool FindLoop_Work(List<TTrim> loop)
        {
            // 检查循环是否闭合
            if (IsClosed(loop))
            {
                return false;
            }
            // 获取最后一个边
            var lastTrim = loop.Last();
            // 获取最后一个边的相连边列表
            var trims = _vertexTrimDict[lastTrim.EndVertexIndex];
            // 获取最后一个边的反向边
            var negativeTrim = lastTrim.Model.Trims[new TrimIndex(lastTrim.EdgeIndex, !lastTrim.IsReserve)];
            // 查找反向边在边表中的索引
            int find = trims.IndexOf(negativeTrim);
            // 遍历边表
            for (int i = 0; i < trims.Count; i++)
            {
                // 向前搜索
                find = trims.PrevIndex(find);
                var findTrim = trims[find];
                // 检查边是否为非 manifold 边
                if (_nonManifoldEdgeIndices.Contains(findTrim.EdgeIndex))
                {
                    continue;
                }
                // 检查边是否已被处理
                if (_processedTrimIndices.Contains(findTrim.TrimIndex))
                {
                    continue;
                }

                // 检查边是否在环中
                if (loop.Contains(findTrim))
                {
                    return false;
                }

                // 将边添加到环中
                loop.Add(findTrim);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 半边比较器（根据角度排序）
        /// </summary>
        private class TrimComparer : IComparer<TTrim>
        {
            public TrimComparer(double angleTol)
            {
                AngleTol = angleTol;
            }

            public readonly double AngleTol;

            public int Compare(TTrim x, TTrim y)
            {
                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }
                double xAngle = x.StartAngle;
                double yAngle = y.StartAngle;
                int angleFlag = CompareValue(RoundTo(xAngle, AngleTol), RoundTo(yAngle, AngleTol));
                if (angleFlag != 0)
                {
                    return angleFlag;
                }
                return CompareValue(x.StartDegree, y.StartDegree);
            }
            /// <summary>
            /// 比较double数值的大小
            /// </summary>
            private int CompareValue(double a, double b)
            {
                if (a + AngleTol > b && b > a - AngleTol)
                    return 0;
                if (a < b)
                    return -1;
                if (a > b)
                    return 1;
                if (!double.IsNaN(a))
                    return 1;
                return !double.IsNaN(b) ? -1 : 0;
            }

            /// <summary>
            /// 将浮点数以<paramref name="tol"/>进行四舍五入
            /// </summary>
            /// <param name="tol">误差或间隔，不可为0</param>
            private static double RoundTo(double value, double tol)
            {
                double t = value / tol;
                t = Math.Round(t);
                return t * tol;
            }
        }

        private bool CreateModel()
        {
            Result = new TModel();
            SortedDictionary<Point2d, int> pointDict = new SortedDictionary<Point2d, int>(new Point2dComparer(_tolerance.EqualPoint));
            try
            {
                foreach (var curve2d in _input)
                {
                    int startVIndex;
                    if (pointDict.ContainsKey(curve2d.StartPoint))
                    {
                        startVIndex = pointDict[curve2d.StartPoint];
                    }
                    else
                    {
                        startVIndex = pointDict.Count;
                        pointDict.Add(curve2d.StartPoint, startVIndex);
                        Result.AddVertex(curve2d.StartPoint, startVIndex);
                    }

                    int endIndex;
                    if (pointDict.ContainsKey(curve2d.EndPoint))
                    {
                        endIndex = pointDict[curve2d.EndPoint];
                    }
                    else
                    {
                        endIndex = pointDict.Count;
                        pointDict.Add(curve2d.EndPoint, endIndex);
                        Result.AddVertex(curve2d.EndPoint, endIndex);
                    }

                    Result.AddEdge(curve2d, startVIndex, endIndex);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}
