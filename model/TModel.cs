using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using PDPArxCommon.Extensions;
using PDPArxCommon.Geometry;

namespace PDPArxCommon.DataStructure.Topology2d
{
    /// <summary>
    /// 模型类
    /// </summary>
    public class TModel : TObject
    {
        /// <summary>
        /// 无效Index
        /// </summary>
        public const int InValidIndex = -1;

        /// <summary>
        /// 边
        /// </summary>
        public readonly SortedDictionary<int, TEdge> Edges;
        /// <summary>
        /// 环
        /// </summary>
        public readonly SortedDictionary<int, TLoop> Loops;
        /// <summary>
        /// 顶点
        /// </summary>
        public readonly SortedDictionary<int, TVertex> Vertices;
        /// <summary>
        /// 半边
        /// </summary>
        public readonly SortedDictionary<TrimIndex, TTrim> Trims;
        /// <summary>
        /// index-顶点
        /// </summary>
        private readonly SortedDictionary<int, Point2d> _indexPointDict;
        /// <summary>
        /// index-curve
        /// </summary>
        private readonly SortedDictionary<int, Curve2d> _curve2ds;
        /// <summary>
        /// 顶点-边邻接
        /// </summary>
        private readonly SortedDictionary<int, SortedSet<int>> _vertexEdgeAdjacency;




        internal TModel()
        {
            _indexPointDict = new SortedDictionary<int, Point2d>();
            Vertices = new SortedDictionary<int, TVertex>();
            _curve2ds = new SortedDictionary<int, Curve2d>();
            Trims = new SortedDictionary<TrimIndex, TTrim>();
            _vertexEdgeAdjacency = new SortedDictionary<int, SortedSet<int>>();
            Edges = new SortedDictionary<int, TEdge>();
            Loops = new SortedDictionary<int, TLoop>();
        }


        #region vertex Adjacency

        /// <summary>
        /// 获取点的邻接边Index
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetVertexAdjacencyEdgeIndices(int vIndex)
        {
            return _vertexEdgeAdjacency[vIndex];
        }

        #endregion

        /// <summary>
        /// 添加顶点
        /// </summary>
        internal void AddVertex(Point2d v, int vIndex)
        {
            _indexPointDict.Add(vIndex, v);
            _vertexEdgeAdjacency.Add(vIndex, new SortedSet<int>());
        }
        /// <summary>
        /// 添加边
        /// </summary>
        internal TEdge AddEdge(Curve2d curve2d, int startVIndex, int endVIndex)
        {
            int eIndex;
            if (_curve2ds.Any())
            {
                eIndex = _curve2ds.Last().Key + 1;

            }
            else
            {
                eIndex = 0;
            }
            _curve2ds.Add(eIndex, curve2d);
            TEdge edge = new TEdge(this, eIndex, startVIndex, endVIndex);
            Edges.Add(eIndex, edge);
            _vertexEdgeAdjacency[startVIndex].Add(eIndex);
            _vertexEdgeAdjacency[endVIndex].Add(eIndex);
            TrimIndex pTrimIndex = new TrimIndex(eIndex, true);
            Trims.Add(pTrimIndex, new TTrim(this, pTrimIndex));
            TrimIndex nTrimIndex = new TrimIndex(eIndex, false);
            Trims.Add(nTrimIndex, new TTrim(this, nTrimIndex));
            return edge;
        }
        /// <summary>
        /// 添加环
        /// </summary>
        internal TLoop AddLoop(IEnumerable<TrimIndex> trimIndices)
        {
            int lIndex;
            if (Loops.Any())
            {
                lIndex = Loops.Last().Key + 1;

            }
            else
            {
                lIndex = 0;
            }

            TLoop loop = new TLoop(this, lIndex);
            int indexOfLoop = 0;
            foreach (var trimIndex in trimIndices)
            {
                loop.TrimIndices.Add(trimIndex);
                Trims[trimIndex].LoopIndex = lIndex;
                Trims[trimIndex].IndexOfLoop = indexOfLoop;
                indexOfLoop++;
            }

            Loops.Add(lIndex, loop);
            return loop;
        }
        /// <summary>
        /// 获取顺时针环
        /// </summary>
        public IEnumerable<TLoop> GetClockwiseLoops()
        {
            foreach (var loopItem in Loops)
            {
                if (!IsAnticlockwise(loopItem.Value))
                {
                    yield return loopItem.Value;
                }
            }
        }

        
        /// <summary>
        /// 获取逆时针环
        /// </summary>
        public IEnumerable<TLoop> GetAnticlockwiseLoops()
        {
            foreach (var loopItem in Loops)
            {
                if (IsAnticlockwise(loopItem.Value))
                {
                    yield return loopItem.Value;
                }
            }
        }
        public IEnumerable<TLoop> GetLoops()
        {
            foreach (var loopItem in Loops)
            {
                yield return loopItem.Value;
            }
        }
        /// <summary>
        /// 获取游离的边
        /// </summary>


        public IEnumerable<TEdge> GetDissociativeEdges()
        {
            Dictionary<int, TEdge> dict = new Dictionary<int, TEdge>(this.Edges);
            foreach (var loop in Loops)
            {
                foreach (var edgeIndex in loop.Value.GetEdgeIndices())
                {
                    if (dict.ContainsKey(edgeIndex))
                    {
                        dict.Remove(edgeIndex);
                    }
                }
            }

            return dict.Values;
        }
        /// <summary>
        /// 获取curve
        /// </summary>
        public Curve2d GetCurve2d(int eIndex)
        {
            return _curve2ds[eIndex];
        }

        #region private

        /// <summary>
        /// 是否为逆时针
        /// </summary>
        /// <param name="tLoop"></param>
        /// <returns></returns>
        private bool IsAnticlockwise(TLoop tLoop)
        {
            int edgeCount = tLoop.TrimIndices.Count;
            Curve2d[] curve2ds = new Curve2d[edgeCount];
            bool[] isReserves = new bool[edgeCount];
            int maxLengthIndex = -1;
            double maxLength = 0.0;
            List<Point2d> vertexes = new List<Point2d>();
            for (int i = 0; i < tLoop.TrimIndices.Count; i++)
            {
                var trim = Trims[tLoop.TrimIndices[i]];
                curve2ds[i] = trim.Curve2d;
                isReserves[i] = trim.IsReserve;
                double length = trim.Curve2d.GetTotalLength();
                if (length > maxLength)
                {
                    maxLength = length;
                    maxLengthIndex = i;
                }

                vertexes.Add(_indexPointDict[trim.StartVertexIndex]);
            }
            var interval = curve2ds[maxLengthIndex].GetInterval();
            var middleParameter = (interval.LowerBound + interval.UpperBound) / 2;
            PointOnCurve2d pointOnCurve2d = new PointOnCurve2d(curve2ds[maxLengthIndex], middleParameter);
            var direction = pointOnCurve2d.GetDerivative(1);
            var perpendicularVector = direction.GetPerpendicularVector().GetNormal();
            if (isReserves[maxLengthIndex])
            {
                perpendicularVector = -perpendicularVector;
            }

            var middleP = pointOnCurve2d.Point;
            double offset = Math.Min(maxLength / 2, 1.0);
            var testP = middleP + perpendicularVector * offset;
            List<double> angles = new List<double>();
            foreach (var point2d in vertexes)
            {
                var toVertex = point2d - testP;
                angles.Add(toVertex.Angle);
            }

            angles.Sort();
            Vector2d testDirection = new Vector2d(17,29).GetNormal();
            double maxAngleGap = 0.0;
            for (int i = 0; i < angles.Count; i++)
            {
                double nextAngle;
                if (i == angles.Count - 1)
                {
                    nextAngle = angles[0] + Math.PI * 2;
                }
                else
                {
                    nextAngle = angles[i + 1];
                }
                double gap = nextAngle - angles[i];
                if (gap > maxAngleGap)
                {
                    maxAngleGap = gap;
                    double middle = (nextAngle + angles[i]) / 2;
                    testDirection = new Vector2d(Math.Cos(middle), Math.Sin(middle));
                }
            }

            int intersectCount = 0;

            Ray2d testRay = new Ray2d(testP, testDirection);
            foreach (var curve2d in curve2ds)
            {
                var intersectRt = IntersectTools.Intersect(testRay, curve2d);
                intersectCount += intersectRt.Count;
            }

            return intersectCount % 2 == 1;
        }
        #endregion
    }
}
