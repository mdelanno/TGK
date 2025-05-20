namespace TGK.Geometry;

public sealed class Edge : BRepEntity
{
    readonly List<EdgeUse> _uses = [];

    public Vertex End { get; }

    public Vertex Start { get; }

    public IReadOnlyList<EdgeUse> Uses { get; }

    internal Edge(Vertex start, Vertex end, int id) : base(id)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        Uses = _uses.AsReadOnly();
        Start = start;
        Start.AddEdgeInternal(this);

        End = end;
        End.AddEdgeInternal(this);
    }

    internal void AddUseInternal(EdgeUse edgeUse)
    {
        ArgumentNullException.ThrowIfNull(edgeUse);

        _uses.Add(edgeUse);
    }

    public override string ToString()
    {
        return $"e{Id}, {Start} → {End}";
    }
}