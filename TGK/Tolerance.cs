namespace TGK;

public readonly struct Tolerance
{
    const double DEFAULT_FOR_POINTS = 1e-10;

    /// <summary>
    /// The minimum distance between distinct points.
    /// </summary>
    public double Point { get; }

    public static Tolerance Default { get; } = new(DEFAULT_FOR_POINTS);

    public Tolerance(double point) : this()
    {
        Point = point;
    }
}