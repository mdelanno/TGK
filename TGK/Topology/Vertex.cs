using TGK.Geometry;

namespace TGK.Topology;

public sealed class Vertex : BRepEntity
{
    readonly HashSet<Edge> _edges = [];

    public Xyz Position { get; set; }

    public IReadOnlySet<Edge> Edges { get; }

    internal Vertex(int id, Xyz position) : base(id)
    {
        Edges = new ReadOnlySet<Edge>(_edges);

        Position = position;
    }

    internal void AddEdgeInternal(Edge edge)
    {
        ArgumentNullException.ThrowIfNull(edge);

        _edges.Add(edge);
    }

    public override string ToString()
    {
        return $"v{Id}";
    }
}