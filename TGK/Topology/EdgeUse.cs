namespace TGK.Topology;

sealed class EdgeUse
{
    readonly EdgeUseFlags _flags;

    public Face Face { get; }

    public Edge Edge { get; }

    public bool SameSenseAsEdge { get; internal set; }

    public Vertex StartVertex => SameSenseAsEdge ? Edge.StartVertex : Edge.EndVertex;

    public Vertex EndVertex => SameSenseAsEdge ? Edge.EndVertex : Edge.StartVertex;

    public bool IsHighSeam => _flags.HasFlag(EdgeUseFlags.HighSeam);

    public bool IsLowSeam => _flags.HasFlag(EdgeUseFlags.LowSeam);

    public EdgeUse(Face face, Edge edge, bool sameSenseAsEdge = true, EdgeUseFlags flags = EdgeUseFlags.None)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(edge);

        Face = face;
        Edge = edge;
        SameSenseAsEdge = sameSenseAsEdge;
        _flags = flags;

        edge.AddUseInternal(this);
    }

    public override string ToString()
    {
        string sameSense = SameSenseAsEdge ? "⇉" : "⇆";
        return $"eu, e{Edge.Id} {sameSense}, {StartVertex} → {EndVertex} ({Face})";
    }
}