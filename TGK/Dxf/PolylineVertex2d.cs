using TGK.Geometry;

namespace TGK.Dxf;

sealed class PolylineVertex2d : DxfEntity, IPolylineVertex2d
{
    public Uv Position { get; set; }

    public double Bulge { get; set; }

    public PolylineVertex2d() : base("VERTEX")
    {
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(10, Position.U);
        dxfWriter.WritePair(20, Position.V);
        dxfWriter.WritePair(30, 0);
        if (Bulge != 0) dxfWriter.WritePair(42, Bulge);
    }
}