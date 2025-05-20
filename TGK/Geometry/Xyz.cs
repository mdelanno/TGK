namespace TGK.Topology;

public readonly struct Xyz
{
    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public Xyz(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return FormattableString.Invariant($"({X:0.###}, {Y:0.###}, {Z:0.###})");
    }
}