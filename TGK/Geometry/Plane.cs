namespace TGK.Geometry;

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

    public double GetSignedDistanceTo(in Xyz point)
    {
        return Normal.DotProduct(point) - DistanceFromOrigin;
    }
}