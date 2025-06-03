using TGK.Geometry;

namespace TGK.FaceterServices;

sealed class NodeListAdapter : IPolygonForTriangulation
{
    List<Node>? _nodes;

    public int VerticesCount => _nodes?.Count ?? 0;

    public Uv GetPosition(int index)
    {
        if (_nodes == null) throw new NullReferenceException($"{nameof(_nodes)} is null.");
#if DEBUG
        return _nodes.Single(n => n.WorldPositionIndex == index).ParametricSpacePosition;
#else
        // Faster version for release builds, with first, we stop enumerating the entire list.
        return _nodes.First(n => n.WorldPositionIndex == index).ParametricSpacePosition;
#endif
    }

    public List<int> GetIndices()
    {
        if (_nodes == null) throw new NullReferenceException($"{nameof(_nodes)} is null.");

        return _nodes.Select(n => n.WorldPositionIndex).ToList();
    }

    public void Set(List<Node> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        _nodes = nodes;
    }
}