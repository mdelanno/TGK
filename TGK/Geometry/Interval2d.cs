namespace TGK.Geometry;

public sealed class Interval2d
{
    public static Interval2d Unbounded { get; } = new(Interval1d.Unbounded, Interval1d.Unbounded);

    public Interval1d U { get; set; }

    public Interval1d V { get; set; }

    public double ULength => U.Length;

    public double VLength => V.Length;

    public double Area => ULength * VLength;

    public Uv Center => new(U.Center, V.Center);

    public bool IsUnbounded => U.IsUnbounded && V.IsUnbounded;

    public Interval2d()
    {
        U = new Interval1d();
        V = new Interval1d();
    }

    public Interval2d(Interval1d u, Interval1d v)
    {
        ArgumentNullException.ThrowIfNull(u);
        ArgumentNullException.ThrowIfNull(v);

        U = u;
        V = v;
    }

    public Interval2d(double uMin, double uMax, double vMin, double vMax)
    {
        U = new Interval1d(uMin, uMax);
        V = new Interval1d(vMin, vMax);
    }

    public Uv GetPointAtFraction(double uFraction = 0.5, double vFraction = 0.5)
    {
        return new Uv(U.GePointAtFraction(uFraction), V.GePointAtFraction(vFraction));
    }

    public bool Contains(in Uv point, double tolerance)
    {
        return U.Contains(point.U, tolerance) && V.Contains(point.V, tolerance);
    }

    public double IntersectingArea(Interval2d other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return U.IntersectingLength(other.U) * V.IntersectingLength(other.V);
    }

    public double SquaredDistanceFromCenter(Interval2d other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return U.SquaredDistanceFromCenter(other.U) + V.SquaredDistanceFromCenter(other.V);
    }

    public bool OnBoundary(in Uv point, double tolerance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tolerance);

        return U.OnBoundary(point.U, tolerance) || V.OnBoundary(point.V, tolerance);
    }
}