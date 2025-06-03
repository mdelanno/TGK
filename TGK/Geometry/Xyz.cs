using static System.Math;

namespace TGK.Geometry;

public readonly struct Xyz
{
    const double EPSILON = 1 / 64d;

    public static Xyz XAxis { get; } = new(1, 0, 0);

    public static Xyz YAxis { get; } = new(0, 1, 0);

    public static Xyz ZAxis { get; } = new(0, 0, 1);

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public double Length => Sqrt(DotProduct(this));

    public double LengthSquared => DotProduct(this);

    public static Xyz Zero { get; } = new(0, 0, 0);

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
        return IsAlmostEqualTo(other, Tolerance.Default.Points);
    }

    public bool IsAlmostEqualTo(in Xyz other, double tolerance)
    {
        return GetSquaredDistanceTo(other) < tolerance * tolerance;
    }

    public double GetDistanceTo(in Xyz other)
    {
        return Sqrt(GetSquaredDistanceTo(other));
    }

    public double GetSquaredDistanceTo(in Xyz other)
    {
        double dx = other.X - X;
        double dy = other.Y - Y;
        double dz = other.Z - Z;
        return dx * dx + dy * dy + dz * dz;
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

    public Xyz ToUnit()
    {
        return new Xyz(X / Length, Y / Length, Z / Length);
    }

    public Xyz GetPerpendicularDirection()
    {
        if (Abs(X) < EPSILON && Abs(Y) < EPSILON)
            return YAxis.CrossProduct(this);
        return ZAxis.CrossProduct(this);
    }

    public double GetAngleTo(in Xyz other)
    {
        if (IsZero() || other.IsZero())
            throw new InvalidOperationException("Cannot calculate angle with zero vector.");

        double cosAngle = DotProduct(other) / (Length * other.Length);

        // To avoid rounding errors.
        cosAngle = double.Clamp(cosAngle, -1.0, 1.0);

        return Acos(cosAngle);
    }

    /// <summary>
    /// Return the angle between this vector and the other in the range [0, 2 x Pi[.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="planeNormalVector">Normal vector of the plane in which the angle to be measured</param>
    /// <returns></returns>
    public double GetAngleTo(Xyz other, Xyz planeNormalVector)
    {
        double angle = GetAngleTo(other);
        if (angle < Tolerance.Default.Angles || angle.IsAlmostEqualTo(PI)) return angle;

        // Creates a normal vector to the plane formed by this vector and the other and checks the sign of the dot product.
        // If greater or equal to zero, the other vector is above this vector, and the angle is between 0 and PI. Else
        // it is between PI and 2 x PI.
        Xyz v = CrossProduct(other);
        if (planeNormalVector.DotProduct(v) >= 0) return angle;
        return 2 * PI - angle;
    }

    public Xyz Negate()
    {
        return new Xyz(-X, -Y, -Z);
    }

    public bool IsParallelTo(in Xyz other)
    {
        return IsAlmostEqualTo(other) || IsAlmostEqualTo(other.Negate());
    }

    public bool IsPerpendicularTo(Xyz other, double tolerance = 1e-10)
    {
        return Abs(DotProduct(other)) < tolerance;
    }

    public CoordinateSystem GetCoordinateSystem(in Xyz normal)
    {
        Xyz xAxis = normal.GetPerpendicularDirection();
        Xyz yAxis = normal.CrossProduct(xAxis).ToUnit();
        return new CoordinateSystem(this, xAxis, yAxis);
    }

    public static Xyz operator +(in Xyz p, in Xyz v)
    {
        return new Xyz(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
    }

    public static Xyz operator -(in Xyz p, in Xyz v)
    {
        return new Xyz(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
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