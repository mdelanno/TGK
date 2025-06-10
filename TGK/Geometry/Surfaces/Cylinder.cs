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

            // As we unroll the curve, we need to add a point.
            var uvPoints = new Uv[strokePoints.Length + 1];

            for (int i = 0; i < strokePoints.Length; i++) uvPoints[i] = GetParametersAtPoint(strokePoints[i]);
            uvPoints[^1] = new Uv(double.Tau, uvPoints[0].V);
            return uvPoints;
        }

        throw new NotImplementedException();
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Axis.TranslateBy(translateVector);
    }
}