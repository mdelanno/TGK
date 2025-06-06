using TGK.Geometry;

namespace TGK.Dxf;

sealed class Line : DxfEntity
{
    public Uv StartPoint { get; set; }

    public Uv EndPoint { get; set; }

    public Line() : base("LINE")
    {
    }

    public Line(Uv startPoint, Uv endPoint) : base("LINE")
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
    }

    public override void Write(DxfWriter dxfWriter)
    {
        ArgumentNullException.ThrowIfNull(dxfWriter);

        dxfWriter.WritePair(10, StartPoint.U);
        dxfWriter.WritePair(20, StartPoint.V);
        dxfWriter.WritePair(30, 0.0);
        dxfWriter.WritePair(11, EndPoint.U);
        dxfWriter.WritePair(21, EndPoint.V);
        dxfWriter.WritePair(31, 0.0);
    }
}