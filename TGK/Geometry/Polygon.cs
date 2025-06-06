namespace TGK.Geometry;

/// <summary>
/// A closed polygonal chain in the plane.
/// </summary>
public sealed class Polygon
{
    public List<Uv> Vertices { get; }

    public DirectionOfRotation DirectionOfRotation { get; }

    /// <summary>
    /// The polygon is simple if it does not intersect itself.
    /// </summary>
    public bool? IsSimple { get; }

    public Polygon(List<Uv> vertices, DirectionOfRotation directionOfRotation = DirectionOfRotation.Unknown, bool isSimple = false)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Count < 3) throw new ArgumentException("A polygon must have at least three vertices.", nameof(vertices));

        Vertices = vertices;
        DirectionOfRotation = directionOfRotation;
        IsSimple = isSimple;
    }

    public double CalculateSignedArea()
    {
        if (IsSimple != true)
            throw new NotSupportedException("Only simple polygons are supported.");

        if (DirectionOfRotation != DirectionOfRotation.CounterClockwise)
            throw new NotImplementedException();

        double sum = 0.0;

        int numberOfVertices = Vertices.Count;

        // Start at the end vertex.
        int vertexIndex = numberOfVertices - 1;

        for (int nextVertexIndex = 0; nextVertexIndex < numberOfVertices; vertexIndex = nextVertexIndex++)
        {
            Uv vertex = Vertices[vertexIndex];
            Uv nextVertex = Vertices[nextVertexIndex];
            sum += (vertex.U - nextVertex.U) * (vertex.V + nextVertex.V);
        }

        return sum / 2;
    }
}