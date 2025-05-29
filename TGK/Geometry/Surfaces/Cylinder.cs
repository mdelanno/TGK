using TGK.Geometry.Curves;

namespace TGK.Geometry.Surfaces;

public sealed class Cylinder : Surface
{
    public Line Axis { get; }

    public double Radius { get; }

    public Cylinder(Line axis, double radius)
    {
        ArgumentNullException.ThrowIfNull(axis);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        Axis = axis;
        Radius = radius;
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10)
    {
        throw new NotImplementedException();
    }

    public override Xyz GetNormal(in Xyz point)
    {
        if (!PassesThrough(point)) throw new ArgumentException($"{nameof(point)} is not on the cylinder.");

        return Axis.GetClosestPointTo(point).GetVectorTo(point).GetNormal();
    }

    public override bool PassesThrough(in Xyz point)
    {
        Xyz pointOnAxis = Axis.GetClosestPointTo(point);
        return pointOnAxis.GetDistanceTo(point).IsAlmostEqualTo(Radius);
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        throw new NotImplementedException();
    }
}