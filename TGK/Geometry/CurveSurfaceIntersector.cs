using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Geometry;

public sealed class CurveSurfaceIntersector
{
    public Surface Surface { get; set; }

    public Curve Curve { get; set; }

    public CurveSurfaceIntersector(Curve curve, Surface surface)
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(surface);

        Curve = curve;
        Surface = surface;
    }

    public IEnumerable<CurveSurfaceIntersectionResult> GetIntersections()
    {
        {
            if (Curve is Line line && Surface is Plane plane)
                return plane.IntersectWith(line);
        }

        throw new NotImplementedException();
    }
}