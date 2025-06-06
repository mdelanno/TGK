namespace TGK.Dxf;

sealed class Polyline2d : DxfEntity, IPolyline2d
{
    public List<IPolylineVertex2d> Vertices { get; }

    public bool IsClosed { get; set; }

    public Polyline2d() : base("POLYLINE")
    {
        Vertices = [];
    }

    public Polyline2d(int numberOfVertices) : base("POLYLINE")
    {
        Vertices = new List<IPolylineVertex2d>(capacity: numberOfVertices);
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(66, 1);
        dxfWriter.WritePair(70, IsClosed ? 1 : 0);
        foreach (IPolylineVertex2d vertex in Vertices)
        {
            var polylineVertex2d = (PolylineVertex2d)vertex;
            polylineVertex2d.WriteCommonGroupCodes(dxfWriter);
            polylineVertex2d.Write(dxfWriter);
            polylineVertex2d.WriteExtendedData(dxfWriter);
        }
        dxfWriter.WritePair(0, "SEQEND");
    }
}