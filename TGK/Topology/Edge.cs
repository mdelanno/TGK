using TGK.Geometry.Curves;

namespace TGK.Topology;

public sealed class Edge : BRepEntity
{
    readonly List<EdgeUse> _uses = [];

    public Curve? Curve { get; }

    public Vertex? StartVertex { get; }

    public Vertex? EndVertex { get; }

    public IReadOnlyList<EdgeUse> Uses { get; }

    public Curve GetCurve() => Curve ?? new Line(StartVertex!.Position, StartVertex.Position.GetVectorTo(EndVertex!.Position).GetNormal());

    public Edge(int id) : base(id)
    {
        Uses = _uses.AsReadOnly();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="startVertex"></param>
    /// <param name="endVertex"></param>
    /// <param name="curve">Can be null if it is a straight edge</param>
    internal Edge(int id, Vertex startVertex, Vertex endVertex, Curve? curve = null) : this(id)
    {
        ArgumentNullException.ThrowIfNull(startVertex);
        ArgumentNullException.ThrowIfNull(endVertex);
        if (curve == null && startVertex == endVertex)
            throw new ArgumentException("Start and end vertex can not be the same if the edge is straight.");

        StartVertex = startVertex;
        StartVertex.AddEdgeInternal(this);

        EndVertex = endVertex;
        EndVertex.AddEdgeInternal(this);

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
        if (StartVertex != null && EndVertex != null)
            return $"e{Id}, {StartVertex} → {EndVertex}";
        return $"e{Id}";
    }
}