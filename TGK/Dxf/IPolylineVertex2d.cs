using TGK.Geometry;

namespace TGK.Dxf;

interface IPolylineVertex2d
{
    Uv Position { get; set; }

    double Bulge { get; set; }
}