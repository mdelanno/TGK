using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

/// <summary>
/// Spherical surface.
///
/// Parameter range is [-π, π) for u, [-π/2, π/2] for v.
/// </summary>
public sealed class Sphere : Surface
{
    public Xyz AxisDirection { get; }

    public Xyz Center { get; private set; }

    public double Radius { get; }

    public Xyz NorthPole => Center + AxisDirection * Radius;

    public Xyz SouthPole => Center - AxisDirection * Radius;

    public Xyz ReferenceAxis => AxisDirection.GetPerpendicularDirection();

    public Sphere(Xyz center, double radius, Xyz axisDirection)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        Center = center;
        Radius = radius;
        AxisDirection = axisDirection;
    }

    public Sphere(Xyz center, double radius) : this(center, radius, Xyz.ZAxis)
    {
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Center += translateVector;
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1E-10)
    {
        throw new NotImplementedException();
    }

    public override Xyz GetNormal(in Xyz point)
    {
        return Center.GetVectorTo(point).ToUnit();
    }

    public override bool PassesThrough(in Xyz point)
    {
        double distance = Center.GetDistanceTo(point);
        return distance.IsAlmostEqualTo(Radius);
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        // Special case of poles
        if (point.IsAlmostEqualTo(NorthPole)) return new Uv(0, PI / 2);
        if (point.IsAlmostEqualTo(SouthPole)) return new Uv(0, -PI / 2);

        // Project point on equatorial plane
        double d = Center.GetVectorTo(point).DotProduct(AxisDirection);
        Xyz projectedPoint = point - AxisDirection * d;

        // Angle / reference axis in equatorial plane [-Pi, Pi]
        double u = ReferenceAxis.GetAngleTo(Center.GetVectorTo(projectedPoint).ToUnit(), AxisDirection);

        // Angle / equatorial plane [-Pi/2, Pi/2]
        double v = PI / 2 - AxisDirection.GetAngleTo(Center.GetVectorTo(point).ToUnit());

        return new Uv(u, v);
    }

    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        Xyz[] points3d = curve.GetStrokePoints(chordHeight);
        var points2d = new Uv[points3d.Length];
        
        for (int i = 0; i < points3d.Length; i++)
        {
            points2d[i] = GetParametersAtPoint(points3d[i]);
        }
        
        return points2d;
    }

    public override Interval2d GetDomain()
    {
        return new Interval2d(-PI, PI.GetPrevious(), -PI / 2, PI / 2);
    }
}