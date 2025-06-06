using TGK.Geometry;

namespace TGK.Dxf;

public sealed class Circle : DxfEntity
{
    public Uv Center { get; }

    public double Radius { get; }

    public Circle(Uv center, double radius) : base("CIRCLE")
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        Center = center;
        Radius = radius;
    }

    public override void Write(DxfWriter dxfWriter)
    {
        ArgumentNullException.ThrowIfNull(dxfWriter);

        dxfWriter.WritePair(10, Center.U);
        dxfWriter.WritePair(20, Center.V);
        dxfWriter.WritePair(30, 0.0);
        dxfWriter.WritePair(40, Radius);
    }
}