namespace TGK.Topology;

sealed class EdgeUse
{
    public Face Face { get; }

    public Edge Edge { get; }

    public bool SameSenseAsEdge { get; internal set; }

    public Vertex StartVertex => SameSenseAsEdge ? Edge.StartVertex : Edge.EndVertex;

    public Vertex EndVertex => SameSenseAsEdge ? Edge.EndVertex : Edge.StartVertex;

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
        return $"eu, e{Edge.Id} {sameSense}, {StartVertex} → {EndVertex} ({Face})";
    }
}