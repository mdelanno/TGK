using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Geometry;

public class OverlapCurveSurfaceIntersectionResult : CurveSurfaceIntersectionResult
{
    public Curve Curve { get; }

    public Surface Surface { get; }

    public Interval1d IntervalOnCurve { get; }

    public OverlapCurveSurfaceIntersectionResult(Curve curve, Surface surface, Interval1d intervalOnCurve)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(intervalOnCurve);

        Curve = curve;
        Surface = surface;
        IntervalOnCurve = intervalOnCurve;
    }
}