using TGK.Geometry.Curves;

namespace TGK.Geometry.Surfaces;

public abstract class Surface : Entity3d
{
    public abstract IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10);

    public abstract Xyz GetNormal(in Xyz point);

    public abstract bool PassesThrough(in Xyz point);

    public abstract Uv GetParametersAtPoint(in Xyz point);

    public abstract Interval2d GetDomain();

    internal abstract Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight);
}