using TGK.Geometry;

namespace TGK.CenterOfInscribedCircleServices;

public sealed class CenterOfInscribedCircle
{
    public Uv Position { get; }
    
    public double Distance { get; }

    public CenterOfInscribedCircle(in Uv position, double distance)
    {
        Position = position;
        Distance = distance;
    }
}