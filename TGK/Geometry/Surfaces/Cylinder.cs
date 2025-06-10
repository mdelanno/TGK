using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

public sealed class Cylinder : Surface
{
    public Line Axis { get; }

    public double Radius { get; }

    public Xyz ReferenceDirection { get; }

    public Cylinder(Line axis, double radius)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        Axis = axis;
        Radius = radius;
        ReferenceDirection = axis.Direction.GetPerpendicularDirection();
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10)
    {
        throw new NotImplementedException();
    }

    public override Xyz GetNormal(in Xyz point)
    {
        if (!PassesThrough(point)) throw new ArgumentException($"{nameof(point)} is not on the cylinder.");

        return Axis.GetClosestPointTo(point).GetVectorTo(point).ToUnit();
    }

    public override bool PassesThrough(in Xyz point)
    {
        Xyz pointOnAxis = Axis.GetClosestPointTo(point);
        return pointOnAxis.GetDistanceTo(point).IsAlmostEqualTo(Radius);
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        Xyz pointOnAxis = Axis.GetClosestPointTo(point, out double v);
        Xyz vector = pointOnAxis.GetVectorTo(point);
        if (!vector.Length.IsAlmostEqualTo(Radius))
        {
            double distance = Abs(vector.Length - Radius);
            throw new ArgumentException($"Point {point} is not on the cylinder (distance: {distance}).");
        }
        double u = ReferenceDirection.GetAngleTo(vector.ToUnit(), Axis.Direction);
        if (double.IsNaN(u))
            throw new InvalidOperationException("GetAngleTo() returned NaN.");
        return new Uv(u, v / Radius);
    }

    /// <remarks>
    /// When the curve in 3D space is closed, we do not return the last point in parametric space, because we use the points obtained to create a loop
    /// (the last point will be ignored anyway, so we do not return it).
    /// </remarks>
    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        switch (curve)
        {
            case Circle circle:
                return ProjectCircleToParameterSpace(circle, chordHeight);

            default:
                throw new NotImplementedException("Only Circle curves are supported for projection to parameter space.");
        }
    }

    Uv[] ProjectCircleToParameterSpace(Circle circle, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(circle);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        if (Axis.Direction.IsParallelTo(circle.Normal))
        {
            Xyz[] strokePoints = circle.GetStrokePoints(chordHeight);
            var uvPoints = new Uv[strokePoints.Length];
            for (int i = 0; i < strokePoints.Length; i++) uvPoints[i] = GetParametersAtPoint(strokePoints[i]);
            return uvPoints;
        }

        throw new NotImplementedException();
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Axis.TranslateBy(translateVector);
    }
}