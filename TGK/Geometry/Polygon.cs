namespace TGK.Geometry;

public sealed class Polygon
{
    readonly bool? _isAntiClockwise;

    public List<Uv> Vertices { get; }

    public Polygon(List<Uv> vertices, bool? isAntiClockwise = true)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Count < 3) throw new ArgumentException("A polygon must have at least three vertices.", nameof(vertices));

        Vertices = vertices;
        _isAntiClockwise = isAntiClockwise;
    }
}