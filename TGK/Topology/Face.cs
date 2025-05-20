using TGK.Geometry;

namespace TGK.Topology;

public sealed class Face : BRepEntity
{
    readonly List<EdgeUse> _edgeUses = [];

    public Surface Surface { get; }

    public IReadOnlyList<EdgeUse> EdgeUses { get; }

    public bool SameSenseAsSurface { get; set; }

    public Face(int id, Surface surface, bool sameSenseAsSurface = true) : base(id)
    {
        ArgumentNullException.ThrowIfNull(surface);

        Surface = surface;
        SameSenseAsSurface = sameSenseAsSurface;
        EdgeUses = _edgeUses.AsReadOnly();
    }

    public void AddEdgeUse(Edge edge, bool sameSenseAsEdge = true)
    {
        ArgumentNullException.ThrowIfNull(edge);

        var edgeUse = new EdgeUse(this, edge, sameSenseAsEdge);
        if (_edgeUses.Count > 0)
        {
            if (_edgeUses[^1].EndVertex != edgeUse.StartVertex)
                throw new ArgumentException("The edge use does not connect to the previous edge use.");
        }
        _edgeUses.Add(edgeUse);
    }

    public IEnumerable<Vertex> GetVertices()
    {
        foreach (EdgeUse edgeUse in EdgeUses)
            yield return edgeUse.StartVertex;
    }

    public override string ToString()
    {
        return $"f{Id}: {string.Join(", ", EdgeUses.Select(eu => eu.StartVertex))}";
    }

    internal void ReverseEdgeUsesInternal()
    {
        _edgeUses.Reverse();
    }
}