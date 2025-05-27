using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Geometry;

public class PointCurveSurfaceIntersectionResult : CurveSurfaceIntersectionResult
{
    public Xyz Point { get; set; }

    public PointCurveSurfaceIntersectionResult(Curve curve, Surface surface, Xyz point)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(surface);

        Point = point;
    }
}