using TGK.Geometry;

namespace TGK.Dxf;

sealed class Polyline3d : DxfEntity
{
    public List<PolylineVertex3d> Vertices { get; } = [];

    public bool IsClosed { get; set; }

    public Polyline3d() : base("POLYLINE")
    {
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(66, 1);
        int flags = IsClosed ? 1 : 0;
        flags += 8; // 8 = This is a 3D polyline
        dxfWriter.WritePair(70, flags);
        foreach (PolylineVertex3d vertex in Vertices)
        {
            vertex.WriteCommonGroupCodes(dxfWriter);
            vertex.Write(dxfWriter);
            vertex.WriteExtendedData(dxfWriter);
        }
        dxfWriter.WritePair(0, "SEQEND");
    }

    public Xyz CalculateBaryCenter()
    {
        if (Vertices.Count == 0)
            throw new InvalidOperationException("Cannot calculate barycenter of an empty polyline.");

        double sumX = 0, sumY = 0, sumZ = 0;
        int count = Vertices.Count;
        foreach (PolylineVertex3d vertex in Vertices)
        {
            sumX += vertex.Position.X / count;
            sumY += vertex.Position.Y / count;
            sumZ += vertex.Position.Z / count;
        }
        return new Xyz(sumX, sumY, sumZ);
    }

    public Xyz CalculateNormalVector()
    {
        if (Vertices.Count < 3)
            throw new InvalidOperationException("At least three vertices are required to calculate a normal vector.");

        Xyz v0 = Vertices[0].Position.GetVectorTo(Vertices[1].Position);
        Xyz v1 = Vertices[0].Position.GetVectorTo(Vertices[2].Position);
        return v0.CrossProduct(v1).ToUnit();
    }
}