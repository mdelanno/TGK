namespace TGK.Dxf;

sealed class LightWeightPolyline : DxfEntity, IPolyline2d
{
    public List<IPolylineVertex2d> Vertices { get; }

    public bool IsClosed { get; set; }

    public LightWeightPolyline() : base("LWPOLYLINE")
    {
        Vertices = [];
    }

    public LightWeightPolyline(int numberOfVertices) : base("LWPOLYLINE")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(numberOfVertices);

        Vertices = new List<IPolylineVertex2d>(capacity: numberOfVertices);
    }

    public override void Write(DxfWriter dxfWriter)
    {
        ArgumentNullException.ThrowIfNull(dxfWriter);

        dxfWriter.WritePair(90, Vertices.Count);
        foreach (IPolylineVertex2d vertex in Vertices)
        {
            dxfWriter.WritePair(10, vertex.Position.U);
            dxfWriter.WritePair(20, vertex.Position.V);
            dxfWriter.WritePair(42, vertex.Bulge);
        }
    }
}