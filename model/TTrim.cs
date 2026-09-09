using Autodesk.AutoCAD.Geometry;

namespace PDPArxCommon.DataStructure.Topology2d
{
    /// <summary>
    /// 半边
    /// </summary>
    public class TTrim : TModelObject
    {
        /// <summary>
        /// 所在环Id
        /// </summary>
        public int LoopIndex { get; set; }
        /// <summary>
        /// 是环中的第几条半边
        /// </summary>
        public int IndexOfLoop { get; set; }
        /// <summary>
        /// 半边Id
        /// </summary>
        public readonly TrimIndex TrimIndex;
        /// <summary>
        /// 边Index
        /// </summary>
        public int EdgeIndex => TrimIndex.Index;
        /// <summary>
        /// 对于边而言、是否为反向的半边
        /// </summary>
        public bool IsReserve => TrimIndex.IsReserve;

        /// <inheritdoc />
        public TTrim(TModel model, TrimIndex trimIndex) : base(model)
        {
            TrimIndex = trimIndex;
        }
        /// <summary>
        /// 起点Index
        /// </summary>
        public int StartVertexIndex
        {
            get
            {
                if (IsReserve)
                {
                    return Model.Edges[EdgeIndex].EndVertexIndex;
                }
                else
                {
                    return Model.Edges[EdgeIndex].StartVertexIndex;
                }
            }
        }
        /// <summary>
        /// 终点Index
        /// </summary>
        public int EndVertexIndex
        {
            get
            {
                if (IsReserve)
                {
                    return Model.Edges[EdgeIndex].StartVertexIndex;
                }
                else
                {
                    return Model.Edges[EdgeIndex].EndVertexIndex;
                }
            }
        }
        /// <summary>
        /// 2dCurve
        /// </summary>
        public Curve2d Curve2d => Model.GetCurve2d(EdgeIndex);
        /// <summary>
        /// 起点角度
        /// </summary>
        public double StartAngle
        {
            get
            {
                using (PointOnCurve2d pointOnCurve2d = new PointOnCurve2d(Curve2d))
                {
                    if (IsReserve)
                    {
                        return (-pointOnCurve2d.GetDerivative(1, Curve2d.GetInterval().UpperBound)).Angle;
                    }
                    else
                    {
                        return pointOnCurve2d.GetDerivative(1, Curve2d.GetInterval().LowerBound).Angle;
                    }
                }
            }
        }
        /// <summary>
        /// 起点凸度
        /// </summary>
        public double StartDegree
        {
            get
            {
                using (PointOnCurve2d pointOnCurve2d = new PointOnCurve2d(Curve2d))
                {
                    if (IsReserve)
                    {
                        var interval = Curve2d.GetInterval();
                        var startDirection = -pointOnCurve2d.GetDerivative(1, interval.UpperBound).GetNormal();
                        var refDirection = startDirection.GetPerpendicularVector();
                        return refDirection.DotProduct(pointOnCurve2d.GetDerivative(2, interval.UpperBound));
                    }
                    else
                    {
                        var interval = Curve2d.GetInterval();
                        var startDirection = pointOnCurve2d.GetDerivative(1, interval.LowerBound).GetNormal();
                        var refDirection = startDirection.GetPerpendicularVector();
                        return refDirection.DotProduct(pointOnCurve2d.GetDerivative(2, interval.LowerBound));
                    }
                }
            }
        }
 
        /// <inheritdoc />
        public override string ToString()
        {
            if (IsReserve)
            {
                return $"{nameof(Curve2d.EndPoint)}:{Curve2d.EndPoint} + \r\n" +
                       $"{nameof(Curve2d.StartPoint)}:{Curve2d.StartPoint} + \r\n" +
                       $"{nameof(StartAngle)}:{StartAngle} + \r\n " +
                       $"{nameof(StartDegree)}:{StartDegree} + \r\n";
            }
            else
            {
                return $"{nameof(Curve2d.StartPoint)}:{Curve2d.StartPoint} + \r\n" +
                       $"{nameof(Curve2d.EndPoint)}:{Curve2d.EndPoint} + \r\n" +
                       $"{nameof(StartAngle)}:{StartAngle} + \r\n " +
                       $"{nameof(StartDegree)}:{StartDegree} + \r\n";
            }
        }
    }
}
