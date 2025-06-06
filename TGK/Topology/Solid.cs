using System.Diagnostics;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;
using static TGK.FaceterServices.TriangulationUtils;

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

    /// <summary>
    /// A convenience method to add a planar face defined by a list of positions.
    /// </summary>
    /// <param name="positions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Face AddPlanarFace(IReadOnlyList<Xyz> positions)
    {
        ArgumentNullException.ThrowIfNull(Vertices);
        if (positions.Count < 3)
            throw new ArgumentException("A face must have at least 3 vertices.");

        Xyz u = positions[0].GetVectorTo(positions[1]);
        Xyz v = positions[0].GetVectorTo(positions[2]);
        Xyz planeNormal = u.CrossProduct(v).ToUnit();
        var plane = new Plane(planeNormal, positions[0]);
        var vertices = new List<Vertex>(positions.Count);
        foreach (Xyz position in positions)
        {
            Vertex vertex = AddVertex(position);
            vertices.Add(vertex);
        }
        int id = Faces.Count;
        var face = new Face(id, plane);
        for (int i = 0; i < ((IReadOnlyList<Vertex>)vertices).Count; i++)
        {
            Vertex vertex = ((IReadOnlyList<Vertex>)vertices)[i];
            Vertex next = ((IReadOnlyList<Vertex>)vertices)[(i + 1) % ((IReadOnlyList<Vertex>)vertices).Count];
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
            Edge circularEdge = edgeUse.Edge;
            Curve curve = circularEdge.GetCurve();
            switch (curve)
            {
                case Circle circle:
                    {
                        // Create the opposite face edge as a circle.
                        Curve oppositeCircle = new Circle(circle.Center + extrusionVector, circle.Radius, circle.Normal);
                        var axis = new Line(circle.Center, extrusionVector.ToUnit());
                        var cylinder = new Cylinder(axis, circle.Radius);
                        var sideFace = new Face(Faces.Count, cylinder);
                        Faces.Add(sideFace);
                        sideFace.AddEdgeUse(circularEdge);
                        Vertex oppositeEdgeVertex = AddVertex(oppositeCircle.GetPointAtParameter(0));
                        var oppositeFaceEdge = new Edge(Edges.Count, oppositeEdgeVertex, oppositeEdgeVertex, oppositeCircle);
                        var seam = new Edge(Edges.Count, circularEdge.StartVertex, oppositeEdgeVertex);
                        Edges.Add(seam);
                        sideFace.AddEdgeUse(seam);
                        Edges.Add(oppositeFaceEdge);
                        oppositeFace.AddEdgeUse(oppositeFaceEdge);
                        sideFace.AddEdgeUse(oppositeFaceEdge);
                        sideFace.AddEdgeUse(seam, sameSenseAsEdge: false);
                        break;
                    }

                default:
                    throw new NotSupportedException("Unsupported curve type for extrusion.");
            }
        }
        else
        {
            // Add the first edge for the first side of the extrusion.
            Edge faceFirstEdge = face.EdgeUses[0].Edge;
            Vertex? faceStartVertex = faceFirstEdge.StartVertex;
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
                    oppositeFaceEndVertex = new Vertex(Vertices.Count, edgeUse.EndVertex!.Position + extrusionVector);
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
                    sideEdgeRight = new Edge(Edges.Count, edgeUse.EndVertex!, oppositeFaceEndVertex);
                    Edges.Add(sideEdgeRight);
                }

                Surface sideFaceSurface;
                switch (edge.Curve)
                {
                    case null:
                        {
                            sideFaceSurface = new Plane(edgeUse.StartVertex!.Position, edgeUse.EndVertex!.Position, oppositeFaceStartVertex.Position);
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

        // We flip the face to have the normal pointing outwards.
        face.Flip();
    }

    public static Solid CreateBox(double sizeX, double sizeY = 0, double sizeZ = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeX);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeY);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeZ);

        if (sizeY == 0) sizeY = sizeX;
        if (sizeZ == 0) sizeZ = sizeX;

        var solid = new Solid();
        double x = sizeX / 2;
        double y = sizeY / 2;
        Face face = solid.AddPlanarFace([
            new Xyz(-x, -y, 0),
            new Xyz(x, -y, 0),
            new Xyz(x, y, 0),
            new Xyz(-x, y, 0)
        ]);
        solid.Extrude(face, new Xyz(0, 0, sizeZ));
        return solid;
    }

    public Edge AddCircularEdge(Circle circle)
    {
        var vertex = new Vertex(Vertices.Count, circle.GetPointAtParameter(0));
        Vertices.Add(vertex);
        var edge = new Edge(Edges.Count, vertex, vertex, circle);
        Edges.Add(edge);
        return edge;
    }

    public Face AddCircularFace(Xyz origin, double radius, Xyz normal)
    {
        var circle = new Circle(origin, radius, normal);
        Edge edge = AddCircularEdge(circle);
        Plane surface = circle.GetPlane();
        var face = new Face(Faces.Count, surface);
        Faces.Add(face);
        face.AddEdgeUse(edge);
        return face;
    }

    public Mesh GetMesh(double chordHeight, bool fillEdgeIndices = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        var mesh = new Mesh();

        var adapter = new NodeListAdapter();
        foreach (Face face in Faces)
        {
            List<Node> nodes = face.ProjectBoundaryToParameterSpace(mesh, chordHeight, fillEdgeIndices);
            adapter.Set(nodes);
            int[] triangleIndices = EarClipping(adapter, chordHeight);
            mesh.TriangleIndices.Add(face, triangleIndices);
        }

        Debug.Assert(mesh.Positions.Count == mesh.Normals.Count, "Mesh positions and normals count should match.");

        return mesh;
    }

    public static Solid CreateCylinder(double radius, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var solid = new Solid();
        Face face = solid.AddCircularFace(Xyz.Zero, radius, Xyz.ZAxis);
        solid.Extrude(face, new Xyz(0, 0, height));
        return solid;
    }
}