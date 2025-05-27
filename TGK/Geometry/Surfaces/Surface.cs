using TGK.Geometry.Curves;

namespace TGK.Geometry.Surfaces;

public abstract class Surface
{
    public abstract IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10);

    public abstract Xyz GetNormal(Xyz point);
}