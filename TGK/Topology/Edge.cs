using TGK.Geometry;
using TGK.Geometry.Curves;

namespace TGK.Topology;

public sealed class Edge : BRepEntity
{
    readonly List<EdgeUse> _uses = [];

    public Curve? Curve { get; }

    public Vertex StartVertex { get; }

    public Vertex EndVertex { get; }

    internal IReadOnlyList<EdgeUse> Uses { get; }

    public EdgeFlags Flags { get; }

    public Curve GetCurve() => Curve ?? new Line(StartVertex!.Position, StartVertex.Position.GetVectorTo(EndVertex!.Position).ToUnit());

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="startVertex"></param>
    /// <param name="endVertex"></param>
    /// <param name="curve">Can be null if it is a straight edge</param>
    internal Edge(int id, Vertex startVertex, Vertex endVertex, Curve? curve = null, EdgeFlags flags = EdgeFlags.None) : base(id)
    {
        ArgumentNullException.ThrowIfNull(startVertex);
        ArgumentNullException.ThrowIfNull(endVertex);
        if (curve == null && startVertex == endVertex)
            throw new ArgumentException("Start and end vertex can not be the same if the edge is straight.");

        Uses = _uses.AsReadOnly();

        StartVertex = startVertex;
        StartVertex.AddEdgeInternal(this);

        EndVertex = endVertex;
        if (EndVertex != StartVertex)
            EndVertex.AddEdgeInternal(this);

        Curve = curve;
        Flags = flags;
    }

    internal void AddUseInternal(EdgeUse edgeUse)
    {
        ArgumentNullException.ThrowIfNull(edgeUse);

        _uses.Add(edgeUse);
    }

    /// <summary>
    /// Returns the stroke points of the edge (excluding the start and end vertices).
    ///
    /// Returns an empty list if the edge is straight or if the edge is very short.
    /// </summary>
    /// <param name="chordHeight"></param>
    /// <returns></returns>
    public IList<Xyz> GetStrokePoints(double chordHeight)
    {
        switch (Curve)
        {
            case Circle circle:
                IList<Xyz> strokePoints;
                if (StartVertex == EndVertex)
                {
                    // Full circle
                    strokePoints = circle.GetStrokePoints(chordHeight);
                }
                else
                {
                    double startParameter = circle.GetParameterAtPoint(StartVertex.Position);
                    double endParameter = circle.GetParameterAtPoint(EndVertex.Position);
                    strokePoints = circle.GetStrokePoints(chordHeight, startParameter, endParameter);
                }
                return strokePoints.Skip(1).Take(strokePoints.Count - 2).ToArray();

            case null:
                return [];

            default:
                throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        return $"e{Id}, {StartVertex} → {EndVertex}";
    }
}