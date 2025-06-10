using static System.Math;

namespace TGK.Geometry;

public readonly struct Uv
{
    public static Uv XAxis { get; } = new(1, 0);

    public static Uv YAxis { get; } = new(0, 1);

    public static Uv Zero => new Uv(0, 0);

    public Uv(double u, double v)
    {
        U = u;
        V = v;
    }

    public double U { get; }

    public double V { get; }

    public bool IsAlmostEqualTo(in Uv other)
    {
        return IsAlmostEqualTo(other, Tolerance.Default.Points);
    }

    public bool IsAlmostEqualTo(in Uv other, double tolerance)
    {
        return GetSquaredDistanceTo(other) < tolerance * tolerance;
    }

    public double GetDistanceTo(in Uv other)
    {
        return Sqrt(GetSquaredDistanceTo(other));
    }

    public double GetSquaredDistanceTo(in Uv other)
    {
        double du = other.U - U;
        double dv = other.V - V;
        return du * du + dv * dv;
    }

    public Uv Lerp(in Uv other, double t)
    {
        if (t is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(t), "t must be between 0 and 1.");

        return new Uv(U + (other.U - U) * t, V + (other.V - V) * t);
    }

    /// <summary>
    /// Returns true if the next point is to the left of the line defined by this point and the previous point.
    /// Return false if the next point is to the right.
    /// Return null if the points are collinear.
    /// </summary>
    /// <param name="previous"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public bool? TurnLeft(in Uv previous, in Uv next, double tolerance)
    {
        double determinate = (U - previous.U) * (next.V - V) - (next.U - U) * (V - previous.V);
        if (Abs(determinate) < tolerance) return null;
        return determinate > 0;
    }

    public static Uv operator -(in Uv p, in Uv v)
    {
        return new Uv(p.U - v.U, p.V - v.V);
    }

    public override string ToString()
    {
        return FormattableString.Invariant($"({U:0.###}, {V:0.###})");
    }

    public Xyz ToXyz(double z = 0)
    {
        return new Xyz(U, V, z);
    }
}