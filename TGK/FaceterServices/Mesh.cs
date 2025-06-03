using TGK.Geometry;
using TGK.Topology;

namespace TGK.FaceterServices;

public sealed class Mesh
{
    public List<Xyz> Positions { get; } = [];

    public List<Xyz> Normals { get; } = [];

    public Dictionary<Face, int[]> TriangleIndices { get; } = [];

    /// <summary>
    /// Can be used to display edges in the scene.
    /// </summary>
    public Dictionary<Edge, int[]> EdgesIndices { get; } = new();
}