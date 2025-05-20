using TGK.Geometry;

namespace TGK.Topology;

public sealed class Edge : BRepEntity
{
    readonly List<EdgeUse> _uses = [];

    public Curve? Curve { get; }

    public Vertex End { get; }

    public Vertex Start { get; }

    public IReadOnlyList<EdgeUse> Uses { get; }

    public Curve GetCurve() => Curve ?? throw new InvalidOperationException();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="curve">Can be null if it is a straight edge</param>
    internal Edge(int id, Vertex start, Vertex end, Curve? curve = null) : base(id)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        if (curve == null && start == end)
            throw new ArgumentException("Start and end vertex can not be the same if the edge is straight.");

        Uses = _uses.AsReadOnly();
        Start = start;
        Start.AddEdgeInternal(this);

        End = end;
        End.AddEdgeInternal(this);

        Curve = curve;
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