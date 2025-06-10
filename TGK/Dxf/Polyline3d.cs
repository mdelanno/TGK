using TGK.Geometry;

namespace TGK.Dxf;

sealed class Polyline3d : DxfEntity
{
    public List<PolylineVertex3d> Vertices { get; } = [];

    public Polyline3d() : base("POLYLINE")
    {
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(66, 1);
        // 8 = This is a 3D polyline
        dxfWriter.WritePair(70, 8);
        foreach (PolylineVertex3d vertex in Vertices)
        {
            var polylineVertex3d = (PolylineVertex3d)vertex;
            polylineVertex3d.WriteCommonGroupCodes(dxfWriter);
            polylineVertex3d.Write(dxfWriter);
            polylineVertex3d.WriteExtendedData(dxfWriter);
        }
        dxfWriter.WritePair(0, "SEQEND");
    }
}