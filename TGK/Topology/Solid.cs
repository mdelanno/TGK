using TGK.Topology;

namespace TGK.Geometry;

public sealed class Solid
{
    public HashSet<Vertex> Vertices { get; } = [];

    public HashSet<Edge> Edges { get; } = [];

    public HashSet<Face> Faces { get; } = [];

    public Vertex AddVertex(in Xyz position)
    {
        int id = Vertices.Count;
        var vertex = new Vertex(position, id);
        Vertices.Add(vertex);
        return vertex;
    }

    public Edge AddEdge(in Vertex start, in Vertex end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        int id = Edges.Count;
        var edge = new Edge(start, end, id);
        Edges.Add(edge);
        return edge;
    }

    public Face AddFace(IReadOnlyList<Vertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        int id = Faces.Count;
        var face = new Face(id);
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex = vertices[i];
            Vertex next = vertices[(i + 1) % vertices.Count];
            Edge edge = AddEdge(vertex, next);
            face.AddEdgeUse(edge);
        }
        Faces.Add(face);
        return face;
    }

    public void Extrude(Face face, Xyz extrusionVector)
    {
        ArgumentNullException.ThrowIfNull(face);

        throw new NotImplementedException();
    }
}