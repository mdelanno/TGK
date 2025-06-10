using TGK.Geometry;

namespace TGK.Dxf;

public sealed class Point : DxfEntity
{
    public Xyz Location { get; set; }

    public Xyz NormalVector { get; init; } = Xyz.ZAxis;

    public Point() : base("POINT")
    {
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(10, Location.X);
        dxfWriter.WritePair(20, Location.Y);
        dxfWriter.WritePair(30, Location.Z);
        dxfWriter.WritePair(210, NormalVector.X);
        dxfWriter.WritePair(220, NormalVector.Y);
        dxfWriter.WritePair(230, NormalVector.Z);
    }
}