namespace TGK.Topology;

public sealed class EdgeUse
{
    public Face Face { get; }

    public Edge Edge { get; }

    public bool SameSenseAsEdge { get; internal set; }

    public Vertex? StartVertex => SameSenseAsEdge ? Edge.Start : Edge.End;

    public Vertex? EndVertex => SameSenseAsEdge ? Edge.End : Edge.Start;

    public EdgeUse(Face face, Edge edge, bool sameSenseAsEdge = true)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(edge);

        Face = face;
        Edge = edge;
        SameSenseAsEdge = sameSenseAsEdge;

        edge.AddUseInternal(this);
    }

    public override string ToString()
    {
        string sameSense = SameSenseAsEdge ? "⇉" : "⇆";
        if (StartVertex != null && EndVertex != null)
            return $"eu, e{Edge.Id} {sameSense}, {StartVertex} → {EndVertex} ({Face})";
        return $"eu, e{Edge.Id} {sameSense}";
    }
}