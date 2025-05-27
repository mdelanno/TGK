using TGK.Geometry.Curves;

namespace TGK.Geometry;

public sealed class PointIntersectionResult : IntersectionResult
{
    public Curve Curve0 { get; }

    public Curve Curve1 { get; }

    public Xyz Point { get; }

    public PointIntersectionResult(Curve curve0, Curve curve1, Xyz point)
    {
        Curve0 = curve0;
        Curve1 = curve1;
        Point = point;
    }
}