using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

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
        throw new NotImplementedException();
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        // Special case of poles
        if (point.IsAlmostEqualTo(NorthPole)) return new Uv(PI / 2, -PI);
        if (point.IsAlmostEqualTo(SouthPole)) return new Uv(-PI / 2, -PI);

        // Project point on equatorial plane
        double d = Center.GetVectorTo(point).DotProduct(AxisDirection);
        Xyz projectedPoint = point - AxisDirection * d;

        // Angle / equatorial plane [-Pi/2, Pi/2]
        double u = PI / 2 - AxisDirection.GetAngleTo(Center.GetVectorTo(point).ToUnit());

        // Angle / reference axis in equatorial plane [-Pi, Pi]
        double v = ReferenceAxis.GetAngleTo(Center.GetVectorTo(projectedPoint).ToUnit(), AxisDirection);

        return new Uv(u, v);
    }

    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        throw new NotImplementedException();
    }
}