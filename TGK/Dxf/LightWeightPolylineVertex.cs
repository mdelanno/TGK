using TGK.Geometry;

namespace TGK.Dxf;

sealed class LightWeightPolylineVertex : IPolylineVertex2d
{
    public Uv Position { get; set; }

    public double Bulge { get; set; }
}