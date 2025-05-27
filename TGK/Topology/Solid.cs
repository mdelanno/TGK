using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Topology;

public sealed class Solid
{
    public HashSet<Vertex> Vertices { get; } = [];

    public HashSet<Edge> Edges { get; } = [];

    public HashSet<Face> Faces { get; } = [];

    public Vertex AddVertex(in Xyz position)
    {
        int id = Vertices.Count;
        var vertex = new Vertex(id, position);
        Vertices.Add(vertex);
        return vertex;
    }

    public Edge AddEdge(in Vertex start, in Vertex end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        int id = Edges.Count;
        var edge = new Edge(id, start, end);
        Edges.Add(edge);
        return edge;
    }

    public Face AddPlanarFace(IReadOnlyList<Xyz> positions)
    {
        ArgumentNullException.ThrowIfNull(Vertices);
        if (positions.Count < 3)
            throw new ArgumentException("A face must have at least 3 vertices.");

        var vertices = new List<Vertex>(positions.Count);
        foreach (Xyz position in positions)
        {
            Vertex vertex = AddVertex(position);
            vertices.Add(vertex);
        }
        return AddPlanarFace(vertices);
    }

    public Face AddPlanarFace(IReadOnlyList<Vertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Count < 3)
            throw new ArgumentException("A face must have at least 3 vertices.");
        Xyz u = vertices[0].Position.GetVectorTo(vertices[1].Position);
        Xyz v = vertices[0].Position.GetVectorTo(vertices[2].Position);
        Xyz planeNormal = u.CrossProduct(v).GetNormal();
        var plane = new Plane(planeNormal);
        plane.DistanceFromOrigin = plane.GetSignedDistanceTo(vertices[0].Position);

        int id = Faces.Count;
        var face = new Face(id, plane);
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
        if (extrusionVector.IsZero())
            throw new ArgumentException("Extrusion vector cannot be zero.");

        if (face.Surface is not Plane facePlane)
            throw new InvalidOperationException("Cannot extrude non-planar face.");

        // We create the opposite face.
        double offset;
        if (facePlane.Normal.DotProduct(extrusionVector) > 0)
            offset = extrusionVector.Length;
        else
            offset = -extrusionVector.Length;
        var oppositeFacePlane = new Plane(facePlane.Normal, facePlane.DistanceFromOrigin + offset);
        var oppositeFace = new Face(Faces.Count, oppositeFacePlane);
        Faces.Add(oppositeFace);

        if (face.EdgeUses.Count == 1)
        {
            // Closed edge (circle, ellipse, etc.)
            EdgeUse edgeUse = face.EdgeUses[0];
            Curve curve = edgeUse.Edge.GetCurve();
            Curve oppositeEdgeCurve;
            switch (curve)
            {
                case Circle circle:
                    {
                        // Create the opposite face edge as a circle.
                        oppositeEdgeCurve = new Circle(circle.Center + extrusionVector, circle.Radius, circle.Normal);
                        break;
                    }

                default:
                    throw new NotSupportedException("Unsupported curve type for extrusion.");
            }
            var oppositeFaceEdge = new Edge(Edges.Count, oppositeEdgeCurve);
            Edges.Add(oppositeFaceEdge);
            oppositeFace.AddEdgeUse(oppositeFaceEdge);
        }
        else
        {
            // Add the first edge for the first side of the extrusion.
            Edge faceFirstEdge = face.EdgeUses[0].Edge;
            Vertex? faceStartVertex = faceFirstEdge.Start;
            if (faceStartVertex == null) throw new NullReferenceException($"{nameof(faceStartVertex)} is null.");
            var oppositeFaceStartVertex = new Vertex(Vertices.Count, faceStartVertex.Position + extrusionVector);
            Vertices.Add(oppositeFaceStartVertex);
            Vertex firstOppositeFaceVertex = oppositeFaceStartVertex;
            var firstEdge = new Edge(Edges.Count, faceStartVertex, oppositeFaceStartVertex);
            Edges.Add(firstEdge);
            Edge sideEdgeLeft = firstEdge;

            // Add the other edges for the sides of the extrusion and the opposite face.
            for (int i = 0; i < face.EdgeUses.Count; i++)
            {
                bool isLastFace = i == face.EdgeUses.Count - 1;

                EdgeUse edgeUse = face.EdgeUses[i];
                Edge edge = edgeUse.Edge;
                Vertex oppositeFaceEndVertex;
                if (isLastFace)
                    oppositeFaceEndVertex = firstOppositeFaceVertex;
                else
                {
                    oppositeFaceEndVertex = new Vertex(Vertices.Count, edgeUse.EndVertex.Position + extrusionVector);
                    Vertices.Add(oppositeFaceEndVertex);
                }
                Curve? oppositeFaceCurve;
                switch (edgeUse.Edge.Curve)
                {
                    case null:
                        {
                            oppositeFaceCurve = null;
                            break;
                        }

                    default:
                        throw new NotSupportedException("Unsupported curve type.");
                }
                var oppositeFaceEdge = new Edge(Edges.Count, oppositeFaceStartVertex, oppositeFaceEndVertex, oppositeFaceCurve);
                Edges.Add(oppositeFaceEdge);
                oppositeFace.AddEdgeUse(oppositeFaceEdge);
                oppositeFaceStartVertex = oppositeFaceEndVertex;

                // Side face
                Edge sideEdgeRight;
                if (isLastFace)
                    sideEdgeRight = firstEdge;
                else
                {
                    sideEdgeRight = new Edge(Edges.Count, edgeUse.EndVertex, oppositeFaceEndVertex);
                    Edges.Add(sideEdgeRight);
                }

                Surface sideFaceSurface;
                switch (edge.Curve)
                {
                    case null:
                        {
                            sideFaceSurface = new Plane(edgeUse.StartVertex.Position, edgeUse.EndVertex.Position, oppositeFaceStartVertex.Position);
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
                var sideFace = new Face(Faces.Count, sideFaceSurface);
                Faces.Add(sideFace);
                sideFace.AddEdgeUse(edge);
                sideFace.AddEdgeUse(sideEdgeRight);
                sideFace.AddEdgeUse(oppositeFaceEdge, sameSenseAsEdge: false);
                sideFace.AddEdgeUse(sideEdgeLeft, sameSenseAsEdge: false);

                sideEdgeLeft = sideEdgeRight;
            }
        }

        // We reverse the face to have the normal pointing outwards.
        face.SameSenseAsSurface = !face.SameSenseAsSurface;

        // We need also to reverse the edge uses.
        foreach (EdgeUse edgeUse in face.EdgeUses)
            edgeUse.SameSenseAsEdge = !edgeUse.SameSenseAsEdge;
        face.ReverseEdgeUsesInternal();
    }

    public Edge AddEdge(Circle circle)
    {
        var edge = new Edge(Edges.Count, circle);
        Edges.Add(edge);
        return edge;
    }

    public Face AddCircularFace(Xyz origin, double radius, Xyz normal)
    {
        var circle = new Circle(origin, radius, normal);
        Edge edge = AddEdge(circle);
        Plane surface = circle.GetPlane();
        var face = new Face(Faces.Count, surface);
        Faces.Add(face);
        face.AddEdgeUse(edge);
        return face;
    }
}