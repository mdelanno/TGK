using static System.Math;

namespace TGK.Geometry;

public readonly struct Xyz
{
    public static Xyz XAxis { get; } = new(1, 0, 0);

    public static Xyz YAxis { get; } = new(0, 1, 0);

    public static Xyz ZAxis { get; } = new(0, 0, 1);

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public static Xyz Origin { get; } = new(0, 0, 0);

    public double Length => Sqrt(DotProduct(this));

    public Xyz(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public bool IsZero(double tolerance = 1e-10)
    {
        return Abs(X) < tolerance && Abs(Y) < tolerance && Abs(Z) < tolerance;
    }

    public bool IsUnitLength(double tolerance = 1e-10)
    {
        return Abs(Length - 1) < tolerance;
    }

    public bool IsAlmostEqualTo(in Xyz other)
    {
        return IsAlmostEqualTo(other, Tolerance.Default.Point);
    }

    public bool IsAlmostEqualTo(in Xyz other, double tolerance)
    {
        return GetDistanceTo(other) < tolerance;
    }

    public double GetDistanceTo(in Xyz other)
    {
        double dx = other.X - X;
        double dy = other.Y - Y;
        double dz = other.Z - Z;
        return Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public Xyz GetVectorTo(in Xyz other)
    {
        return new Xyz(other.X - X, other.Y - Y, other.Z - Z);
    }

    public Xyz CrossProduct(in Xyz other)
    {
        return new Xyz(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);
    }

    public double DotProduct(in Xyz other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    public Xyz GetNormal()
    {
        return new Xyz(X / Length, Y / Length, Z / Length);
    }

    public static Xyz operator +(in Xyz p, in Xyz v)
    {
        return new Xyz(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
    }

    public static Xyz operator *(Xyz v, double a)
    {
        return new Xyz(v.X * a, v.Y * a, v.Z * a);
    }

    public static Xyz operator /(Xyz v, double a)
    {
        return new Xyz(v.X / a, v.Y / a, v.Z / a);
    }

    public override string ToString()
    {
        return FormattableString.Invariant($"({X:0.###}, {Y:0.###}, {Z:0.###})");
    }
}