namespace TGK.Geometry;

public readonly struct Uv
{
    public Uv(double u, double v)
    {
        U = u;
        V = v;
    }

    public double U { get; }

    public double V { get; }

    public static Uv Zero => new Uv(0.0, 0.0);
}