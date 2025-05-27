using static System.Math;

namespace TGK.Geometry;

public sealed class Interval1d
{
    public static Interval1d Unbounded { get; } = new(double.NegativeInfinity, double.PositiveInfinity);

    public double Min { get; set; }

    public double Max { get; set; }

    public double Length => Max - Min;

    public double Center => Min / 2.0 + Max / 2.0;

    public bool IsUnbounded => double.IsPositiveInfinity(Max) && double.IsNegativeInfinity(Min);

    public Interval1d()
    {
        Min = double.NegativeInfinity;
        Max = double.PositiveInfinity;
    }

    public Interval1d(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public double GePointAtFraction(double fraction = 0.5)
    {
        return Min * (1 - fraction) + Max * fraction;
    }

    public bool Contains(double value, double tolerance)
    {
        if (Max < Min) return false;
        return Min - tolerance <= value && value <= Max + tolerance;
    }

    public double IntersectingLength(Interval1d other)
    {
        double length = Min(Max, other.Max) - Max(Min, other.Min);
        if (length > 0)
            return length;
        return 0;
    }

    public double SquaredDistanceFromCenter(Interval1d other)
    {
        double distance = (Min + Max - (other.Min + other.Max)) / 2.0;
        return distance * distance;
    }

    public bool OnBoundary(double pos, double tolerance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tolerance);

        if (Max < Min) return false;
        return Abs(pos - Max) < tolerance || Abs(pos - Min) < tolerance;
    }
}