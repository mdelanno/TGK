namespace TGK;

public readonly struct Tolerance
{
    const double DEFAULT_FOR_POINTS = 1e-10;

    const double DEFAULT_FOR_ANGLES = 1e-12;

    /// <summary>
    /// The minimum distance between distinct points.
    /// </summary>
    public double Points { get; }

    public double Angles { get; }

    public static Tolerance Default { get; } = new(DEFAULT_FOR_POINTS, DEFAULT_FOR_ANGLES);

    public Tolerance(double points, double angles) : this()
    {
        Points = points;
        Angles = angles;
    }
}