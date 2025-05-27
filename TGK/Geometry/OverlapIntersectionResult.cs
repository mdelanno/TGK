using TGK.Geometry.Curves;

namespace TGK.Geometry;

public class OverlapIntersectionResult : IntersectionResult
{
    public Curve Curve0 { get; }

    public Curve Curve1 { get; }

    public Interval1d IntervalOnCurve0 { get; }

    public OverlapIntersectionResult(Curve curve0, Curve curve1, Interval1d intervalOnCurve0)
    {
        Curve0 = curve0;
        Curve1 = curve1;
        IntervalOnCurve0 = intervalOnCurve0;
    }
}