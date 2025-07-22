using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

public sealed class Plane : Surface
{
    public static Plane XY { get; } = new(Xyz.ZAxis);

    public static Plane YZ { get; } = new(Xyz.XAxis);

    public static Plane ZX { get; } = new(Xyz.YAxis);

    public double DistanceFromOrigin => GetSignedDistanceTo(Origin);

    public Xyz Normal => CoordinateSystem.ZAxis;

    public CoordinateSystem CoordinateSystem { get; }

    public Xyz Origin => CoordinateSystem.Origin;

    public Plane(Xyz normal, double distanceFromOrigin = 0)
    {
        Xyz origin = Xyz.Zero + normal * distanceFromOrigin;
        CoordinateSystem = origin.GetCoordinateSystem(normal);
    }

    /// <summary>
    /// Construct a plane from 3 points.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    public Plane(in Xyz origin, in Xyz p0, in Xyz p1)
    {
        if (origin.IsAlmostEqualTo(p0))
            throw new ArgumentException("origin and p0 must be different.");
        if (origin.IsAlmostEqualTo(p1))
            throw new ArgumentException("origin and p1 must be different.");
        if (p0.IsAlmostEqualTo(p1))
            throw new ArgumentException("p0 and p1 must be different.");

        Xyz normal = origin.GetVectorTo(p0).CrossProduct(origin.GetVectorTo(p1)).ToUnit();
        CoordinateSystem = origin.GetCoordinateSystem(normal);
    }

    public Plane(Xyz normal, Xyz origin)
    {
        ArgumentNullException.ThrowIfNull(normal);
        ArgumentNullException.ThrowIfNull(origin);

        if (!normal.IsUnitLength())
            throw new ArgumentException("Normal must be a unit vector.", nameof(normal));

        CoordinateSystem = origin.GetCoordinateSystem(normal);
    }

    public double GetSignedDistanceTo(in Xyz point)
    {
        return Normal.DotProduct(CoordinateSystem.Origin.GetVectorTo(point));
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10)
    {
        ArgumentNullException.ThrowIfNull(line);

        // Calculate the distance from the line origin to the plane.
        double distance = GetSignedDistanceTo(line.Origin);
        if (Abs(distance) < tolerance)
        {
            // The line is on the plane.
            yield return new OverlapCurveSurfaceIntersectionResult(line, this, Interval1d.Unbounded);
            yield break;
        }

        // Calculate the direction of the line
        double directionDot = Normal.DotProduct(line.Direction);
        if (Abs(directionDot) < tolerance)
        {
            // The line is parallel to the plane, no intersection.
            yield break;
        }

        // Calculate the parameter at which the intersection occurs.
        double parameter = -distance / directionDot;
        Xyz intersectionPoint = line.GetPointAtParameter(parameter);
        yield return new PointCurveSurfaceIntersectionResult(line, this, intersectionPoint);
    }

    public override Xyz GetNormal(in Xyz point)
    {
        return Normal;
    }

    public override bool PassesThrough(in Xyz point)
    {
        throw new NotImplementedException();
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        return CoordinateSystem.Convert2d(point);
    }

    [Obsolete("Use GetParametersAtPoint instead.")]
    public IEnumerable<Uv> GetParametersAtPoints(IEnumerable<Xyz> points)
    {
        foreach (Xyz point in points)
            yield return CoordinateSystem.Convert2d(point);
    }

    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        throw new NotImplementedException();
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        CoordinateSystem.TranslateBy(translateVector);
    }

    public override Interval2d GetDomain()
    {
        return Interval2d.Unbounded;
    }
}