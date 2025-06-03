using TGK.Geometry.Curves;

namespace TGK.Geometry.Surfaces;

public abstract class Surface : Entity3d
{
    public abstract IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10);

    public abstract Xyz GetNormal(in Xyz point);

    public abstract bool PassesThrough(in Xyz point);

    public abstract Uv GetParametersAtPoint(in Xyz point);

    /// <summary>
    /// Use this method to get the parameters at multiple points on the surface
    /// (it's more efficient than calling <see cref="GetParametersAtPoint"/> for each point).
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public virtual IEnumerable<Uv> GetParametersAtPoints(IEnumerable<Xyz> points)
    {
        foreach (Xyz point in points)
            yield return GetParametersAtPoint(point);
    }

    internal abstract Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight);
}