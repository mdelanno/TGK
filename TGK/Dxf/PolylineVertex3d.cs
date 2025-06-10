using TGK.Geometry;

namespace TGK.Dxf;

sealed class PolylineVertex3d : DxfEntity
{
    public Xyz Position { get; set; }

    public double Bulge { get; set; }

    public PolylineVertex3d() : base("VERTEX")
    {
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(10, Position.X);
        dxfWriter.WritePair(20, Position.Y);
        dxfWriter.WritePair(30, Position.Z);
        if (Bulge != 0) dxfWriter.WritePair(42, Bulge);
    }
}