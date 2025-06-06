namespace TGK.Dxf;

interface IPolyline2d
{
    List<IPolylineVertex2d> Vertices { get; }

    bool IsClosed { get; set; }
}