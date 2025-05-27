using TGK.Geometry.Curves;

namespace TGK.Geometry.Surfaces;

public sealed class Plane : Surface
{
    public static Plane XY { get; } = new(Xyz.ZAxis);

    public double DistanceFromOrigin { get; set; }

    public Xyz Normal { get; }

    public Xyz GetOrigin()
    {
        return Xyz.Origin + Normal * DistanceFromOrigin;
    }

    public Plane(Xyz normal, double distanceFromOrigin = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(distanceFromOrigin);

        DistanceFromOrigin = distanceFromOrigin;
        Normal = normal;
    }

    /// <summary>
    /// Construct a plane from 3 points.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    public Plane(in Xyz p0, in Xyz p1, in Xyz p2)
    {
        if (p0.IsAlmostEqualTo(p1))
            throw new ArgumentException("p0 and p1 must be different.");
        if (p0.IsAlmostEqualTo(p2))
            throw new ArgumentException("p0 and p2 must be different.");
        if (p1.IsAlmostEqualTo(p2))
            throw new ArgumentException("p1 and p2 must be different.");

        Normal = p0.GetVectorTo(p1).CrossProduct(p0.GetVectorTo(p2)).GetNormal();
        DistanceFromOrigin = Normal.DotProduct(p0);
    }

    public Plane(Xyz normal, Xyz pointOnPlane)
    {
        ArgumentNullException.ThrowIfNull(normal);
        ArgumentNullException.ThrowIfNull(pointOnPlane);

        if (normal.IsZero())
            throw new ArgumentException("Normal vector cannot be zero.");

        Normal = normal.GetNormal();
        DistanceFromOrigin = Normal.DotProduct(pointOnPlane);
    }

    public double GetSignedDistanceTo(in Xyz point)
    {
        return Normal.DotProduct(point) - DistanceFromOrigin;
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10)
    {
        ArgumentNullException.ThrowIfNull(line);

        // Calculate the distance from the line origin to the plane.
        double distance = GetSignedDistanceTo(line.Origin);
        if (Math.Abs(distance) < tolerance)
        {
            // The line is on the plane.
            yield return new OverlapCurveSurfaceIntersectionResult(line, this, Interval1d.Unbounded);
            yield break;
        }

        // Calculate the direction of the line
        double directionDot = Normal.DotProduct(line.Direction);
        if (Math.Abs(directionDot) < tolerance)
        {
            // The line is parallel to the plane, no intersection.
            yield break;
        }

        // Calculate the parameter at which the intersection occurs.
        double parameter = -distance / directionDot;
        Xyz intersectionPoint = line.GetPointAtParameter(parameter);
        yield return new PointCurveSurfaceIntersectionResult(line, this, intersectionPoint);
    }

    public override Xyz GetNormal(Xyz point)
    {
        return Normal;
    }
}