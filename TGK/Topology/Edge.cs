using TGK.Geometry.Curves;

namespace TGK.Topology;

public sealed class Edge : BRepEntity
{
    readonly List<EdgeUse> _uses = [];

    public Curve? Curve { get; }

    public Vertex? End { get; }

    public Vertex? Start { get; }

    public IReadOnlyList<EdgeUse> Uses { get; }

    public Curve GetCurve() => Curve ?? new Line(Start!.Position, Start.Position.GetVectorTo(End!.Position).GetNormal());

    public Edge(int id) : base(id)
    {
        Uses = _uses.AsReadOnly();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="curve">Can be null if it is a straight edge</param>
    internal Edge(int id, Vertex start, Vertex end, Curve? curve = null) : this(id)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        if (curve == null && start == end)
            throw new ArgumentException("Start and end vertex can not be the same if the edge is straight.");

        Start = start;
        Start.AddEdgeInternal(this);

        End = end;
        End.AddEdgeInternal(this);

        Curve = curve;
    }

    public Edge(int id, Curve curve) : this(id)
    {
        ArgumentNullException.ThrowIfNull(curve);

        Curve = curve;
    }

    internal void AddUseInternal(EdgeUse edgeUse)
    {
        ArgumentNullException.ThrowIfNull(edgeUse);

        _uses.Add(edgeUse);
    }

    public override string ToString()
    {
        if (Start != null && End != null)
            return $"e{Id}, {Start} → {End}";
        return $"e{Id}";
    }
}