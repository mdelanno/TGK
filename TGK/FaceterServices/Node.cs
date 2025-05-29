using TGK.Geometry;

namespace TGK.FaceterServices;

sealed class Node
{
    public int WorldPositionIndex { get; }

    public Uv ParametricSpacePosition { get; }

    public Node(int worldPositionIndex, Uv parametricSpacePosition)
    {
        WorldPositionIndex = worldPositionIndex;
        ParametricSpacePosition = parametricSpacePosition;
    }

    public override string ToString()
    {
        return $"{ParametricSpacePosition} → {WorldPositionIndex}";
    }
}