using System;
using Autodesk.AutoCAD.Geometry;
using PDPArxCommon.Arithmetic;
using PDPArxCommon.DataStructure;
using PDPArxCommon.DataStructure.Conditions;
using PDPNetCommon.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using PDPArxCommon.Extensions;
using PDPNetCommon.DataStructure.Enums;
using PDPNetCommon.Extensions;

namespace PDPArxCommon.Geometry
{
    /// <summary>
    /// 连续轮廓线创建工具
    /// </summary>
    public static class ContinuousContourBuildTools
    {
        /// <summary>
        /// 根据一组轮廓线创建连续轮廓线
        /// </summary>
        private static ContinuousContour CreateFrom_Single(List<Curve3dRecord> curve3dRecords, Point3dEqualCondition pec)
        {
            Debug.Assert(curve3dRecords.Count > 0);
            if (curve3dRecords.Count == 1)
            {
                var single = curve3dRecords[0];
                if (pec.IsValid(single.Curve3d.StartPoint,single.Curve3d.EndPoint))
                {
                    ContinuousContourVertex vertex = new ContinuousContourVertex(
                        single.Curve3d.StartPoint.GetMiddlePoint(single.Curve3d.EndPoint),
                        single,
                        single,
                        pec.Tol);
                    return new ContinuousContour(new List<ContinuousContourVertex>(){ vertex });
                }
                else
                {
                    
                }
            }
            else
            {
                
            }


            List<Point3d> points = new List<Point3d>();
            Dictionary<int, List<Curve3dRecord>> groups = new Dictionary<int, List<Curve3dRecord>>();
            foreach (var curve3dRecord in curve3dRecords)
            {
                var startIndex = CollectionTools.AddNonRedundantValueToList(points, curve3dRecord.Curve3d.StartPoint, pec.IsValid);
                if (!groups.ContainsKey(startIndex))
                {
                    groups.Add(startIndex, new List<Curve3dRecord>());
                }
                groups[startIndex].Add(curve3dRecord);
                var endIndex = CollectionTools.AddNonRedundantValueToList(points, curve3dRecord.Curve3d.EndPoint, pec.IsValid);
                if (!groups.ContainsKey(endIndex))
                {
                    groups.Add(endIndex, new List<Curve3dRecord>());
                }
                groups[endIndex].Add(curve3dRecord);
            }
            List<ContinuousContourVertex> vertices = new List<ContinuousContourVertex>();
            for (int i = 0; i < points.Count; i++)
            {
                var curve3ds = groups[i];
                var point = points[i];
                ContinuousContourVertex continuousContourVertex = new ContinuousContourVertex(point, null, null, pec.Tol);
                if (curve3ds.Count == 1)
                {
                    continuousContourVertex.PreCurve3dRecord = curve3ds[0];
                }
                else
                {
                    if (curve3ds.Count == 2)
                    {
                        continuousContourVertex.PreCurve3dRecord = curve3ds[0];
                        continuousContourVertex.NextCurve3dRecord = curve3ds[1];
                    }
                    else
                    {
                        return null;
                    }
                }

                vertices.Add(continuousContourVertex);
            }
            if (vertices.Count < 0)
            {
                return null;
            }
            return BuildContinuousContour(vertices);

        }
        /// <summary>
        /// 根据一组顶点创建连续轮廓线
        /// </summary>
        private static ContinuousContour BuildContinuousContour(List<ContinuousContourVertex> vertices)
        {
            ContinuousContourVertex start = null;
            foreach (var vertex in vertices)
            {
                if (vertex.IsBoundaryVertex)
                {
                    start = vertex;
                    break;
                }
            }

            if (start == null)
            {
                start = vertices[0];
            }

            vertices.Remove(start);
            var currentVertex = start;
            if (currentVertex.IsBoundaryVertex && currentVertex.NextCurve3dRecord == null)
            {
                currentVertex.Reverse();
            }
            List<ContinuousContourVertex> orderResult = new List<ContinuousContourVertex>() { currentVertex };
            while (vertices.Count > 0)
            {
                bool rt = false;
                for (int i = 0; i < vertices.Count; i++)
                {
                    var nextVertex = vertices[i];
                    if (nextVertex.PreCurve3dRecord != null && currentVertex.NextCurve3dRecord == nextVertex.PreCurve3dRecord)
                    {
                        currentVertex = nextVertex;
                        orderResult.Add(currentVertex);
                        vertices.RemoveAt(i);
                        rt = true;
                        break;
                    }

                    if (nextVertex.NextCurve3dRecord != null && currentVertex.NextCurve3dRecord == nextVertex.NextCurve3dRecord)
                    {
                        nextVertex.Reverse();
                        currentVertex = nextVertex;
                        orderResult.Add(currentVertex);
                        vertices.RemoveAt(i);
                        rt = true;
                        break;
                    }
                }

                if (!rt)
                {
                    return null;
                }

            }

            return new ContinuousContour(orderResult);
        }

        /// <summary>
        /// 根据已序的曲线创建
        /// </summary>
        private static ContinuousContour BuildFrom(Curve3dRecord[] curve3dRecords, bool[] isReserves, Tolerance tolerance)
        {
            Debug.Assert(curve3dRecords.Length > 0 && curve3dRecords.Length == isReserves.Length);
            int length = curve3dRecords.Length;
            Point3d firstP;
            if (isReserves[0])
            {
                firstP = curve3dRecords[0].Curve3d.EndPoint;
            }
            else
            {
                firstP = curve3dRecords[0].Curve3d.StartPoint;
            }

            Point3d lastP;
            if (isReserves[length - 1])
            {
                lastP = curve3dRecords[length - 1].Curve3d.StartPoint;
            }
            else
            {
                lastP = curve3dRecords[length - 1].Curve3d.EndPoint;
            }

            if (firstP.IsEqualTo(lastP,tolerance))
            {
                // loop
                if (length == 1)
                {
                    ContinuousContourVertex vertex = new ContinuousContourVertex(
                        firstP.GetMiddlePoint(lastP),
                        curve3dRecords[0],
                        curve3dRecords[0],
                        tolerance);
                    return new ContinuousContour(new List<ContinuousContourVertex>() { vertex });
                }

                ContinuousContourVertex[] vs = new ContinuousContourVertex[length];
                for (int cIndex = 0; cIndex < length; cIndex++)
                {
                    var c = curve3dRecords[cIndex];
                    Point3d p;
                    if (isReserves[cIndex])
                    {
                        p = c.Curve3d.EndPoint;
                    }
                    else
                    {
                        p = c.Curve3d.StartPoint;
                    }

                    var preCIndex = curve3dRecords.PrevIndex(cIndex);
                    var preC = curve3dRecords[preCIndex];
                    Point3d preP;
                    if (isReserves[preCIndex])
                    {
                        preP = preC.Curve3d.StartPoint;
                    }
                    else
                    {
                        preP = preC.Curve3d.EndPoint;
                    }

                    Debug.Assert(p.IsEqualTo(preP, tolerance));
                    ContinuousContourVertex v = new ContinuousContourVertex(
                        p.GetMiddlePoint(preP),
                        preC,
                        c,
                        tolerance);
                    vs[cIndex] = v;

                }

                return new ContinuousContour(vs);
            }
            else
            {
                ContinuousContourVertex[] vs = new ContinuousContourVertex[length + 1];
                var firstC = curve3dRecords[0];
                var lastC = curve3dRecords[length - 1];
                if (isReserves[0])
                {
                    vs[0] = new ContinuousContourVertex(
                        firstC.Curve3d.EndPoint,
                        null,
                        firstC,
                        tolerance);
                }
                else
                {
                    vs[0] = new ContinuousContourVertex(
                        firstC.Curve3d.StartPoint,
                        null,
                        firstC,
                        tolerance);
                }

                if (isReserves[length - 1])
                {
                    vs[length] = new ContinuousContourVertex(
                        lastC.Curve3d.StartPoint,
                        lastC,
                        null,
                        tolerance);
                }
                else
                {
                    vs[length] = new ContinuousContourVertex(
                        lastC.Curve3d.EndPoint,
                        lastC,
                        null,
                        tolerance);
                }

                for (int nextIndex = 1; nextIndex < length; nextIndex++)
                {
                    var nextC = curve3dRecords[nextIndex];
                    Point3d nextP;
                    if (isReserves[nextIndex])
                    {
                        nextP = nextC.Curve3d.EndPoint;
                    }
                    else
                    {
                        nextP = nextC.Curve3d.StartPoint;
                    }

                    var preCIndex = nextIndex - 1;
                    var preC = curve3dRecords[preCIndex];
                    Point3d preP;
                    if (isReserves[preCIndex])
                    {
                        preP = preC.Curve3d.StartPoint;
                    }
                    else
                    {
                        preP = preC.Curve3d.EndPoint;
                    }

                    Debug.Assert(nextP.IsEqualTo(preP, tolerance));
                    ContinuousContourVertex v = new ContinuousContourVertex(
                        nextP.GetMiddlePoint(preP),
                        preC,
                        nextC,
                        tolerance);
                    vs[nextIndex] = v;
                }

                return new ContinuousContour(vs);
            }
        }

        /// <summary>
        /// 根据曲线创建
        /// </summary>
        public static List<ContinuousContour> CreateFrom(List<Curve3dRecord> curve3dRecords, Tolerance tolerance)
        {
            List<ContinuousContour> continuousContours = new List<ContinuousContour>();
            Curve3dRecordConnectCondition crcc = new Curve3dRecordConnectCondition(tolerance);
            var connectGroups = CollectionTools.GroupBy(curve3dRecords, crcc.IsConnect, GroupType.Any);
            foreach (var connectGroup in connectGroups)
            {
                try
                {
                    Dictionary<Curve2d, Curve3dRecord> dict = new Dictionary<Curve2d, Curve3dRecord>();
                    foreach (var curve3dRecord in connectGroup)
                    {
                        var curve2d = curve3dRecord.Curve3d.ConvertTo2d();
                        Debug.Assert(curve2d != null);
                        dict.Add(curve2d, curve3dRecord);
                    }

                    var model = RegionSearcher.GetModel(dict.Keys, tolerance);
                    if (model is null)
                    {
                        continue;
                    }
                    var array = RegionSearcher.GetContinuousContourEdges(model);

                    foreach (var group in array)
                    {
                        Curve3dRecord[] groupCs = new Curve3dRecord[group.Count];
                        bool[] groupIsReserves = new bool[group.Count];
                        for (int i = 0; i < group.Count; i++)
                        {
                            var trim = group[i];
                            groupCs[i] = dict[trim.Curve2d];
                            groupIsReserves[i] = trim.IsReserve;
                        }

                        var rt = BuildFrom(groupCs, groupIsReserves, tolerance);
                        continuousContours.Add(rt);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                }
               
            }
            return continuousContours;
        }
        /// <summary>
        /// 根据轮廓线（闭合曲线）创建自环的连续轮廓线
        /// </summary>
        public static ContinuousContour CreateSingleFrom(Curve3dRecord single,Tolerance tolerance)
        {
            Point3dEqualCondition point3dEqualCondition = new Point3dEqualCondition(tolerance);
            if (point3dEqualCondition.IsValid(single.Curve3d.StartPoint, single.Curve3d.EndPoint))
            {
                ContinuousContourVertex vertex = new ContinuousContourVertex(
                    single.Curve3d.StartPoint.GetMiddlePoint(single.Curve3d.EndPoint),
                    single,
                    single,
                    tolerance);
                return new ContinuousContour(new List<ContinuousContourVertex>() { vertex });
            }
            else
            {
                ContinuousContourVertex pre =
                    new ContinuousContourVertex(single.Curve3d.StartPoint, null, single, tolerance);
                ContinuousContourVertex next =
                    new ContinuousContourVertex(single.Curve3d.EndPoint, single, null, tolerance);
                return new ContinuousContour(new[] {pre, next});
            }
        }
    }
}
