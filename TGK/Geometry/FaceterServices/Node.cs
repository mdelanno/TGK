using TGK.Topology;

namespace TGK.Geometry.FaceterServices;

sealed class Node
{
    public Edge Edge { get; }

    PointOnCurve PointOnCurve { get; }

    Uv UvPosition { get; }

    public Node? PreviousNode { get; set; }

    public Node? NextNode { get; set; }

    public Node(Edge edge, PointOnCurve pointOnCurve, Uv uvPosition)
    {
        ArgumentNullException.ThrowIfNull(edge);
        ArgumentNullException.ThrowIfNull(pointOnCurve);

        Edge = edge;
        PointOnCurve = pointOnCurve;
        UvPosition = uvPosition;
    }
}