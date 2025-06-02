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

    public double CalculateSignedArea()
    {
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