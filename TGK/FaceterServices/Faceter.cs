using TGK.Topology;

namespace TGK.FaceterServices;

public class Faceter
{
    public Mesh FacetSolid(Solid solid, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(solid);
        ArgumentOutOfRangeException.ThrowIfNegative(chordHeight);

        var mesh = new Mesh();
        return mesh;
    }
}