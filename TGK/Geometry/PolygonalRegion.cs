namespace TGK.Geometry;

/// <summary>
/// A 2D space bounded by a simple polygon, possibly with other simples polygons inside that may be nested.
///
/// A simple polygon is one that does not intersect itself.
/// </summary>
public sealed class PolygonalRegion
{
    public Polygon OuterBoundary { get; }

    public List<Polygon> InnerContours { get; } = [];

    public PolygonalRegion(Polygon outerBoundary)
    {
        ArgumentNullException.ThrowIfNull(outerBoundary);

        OuterBoundary = outerBoundary;
    }
}