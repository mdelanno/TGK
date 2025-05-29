using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Topology;

public sealed class Face : BRepEntity
{
    readonly List<EdgeUse> _edgeUses = [];

    public Surface Surface { get; }

    public IReadOnlyList<EdgeUse> EdgeUses { get; }

    public bool SameSenseAsSurface { get; set; }

    public Face(int id, Surface surface, bool sameSenseAsSurface = true) : base(id)
    {
        ArgumentNullException.ThrowIfNull(surface);

        Surface = surface;
        SameSenseAsSurface = sameSenseAsSurface;
        EdgeUses = _edgeUses.AsReadOnly();
    }

    public void AddEdgeUse(Edge edge, bool sameSenseAsEdge = true)
    {
        ArgumentNullException.ThrowIfNull(edge);

        var edgeUse = new EdgeUse(this, edge, sameSenseAsEdge);
        if (_edgeUses.Count > 0)
        {
            if (_edgeUses[^1].EndVertex != edgeUse.StartVertex)
                throw new ArgumentException("The edge use does not connect to the previous edge use.");
        }
        _edgeUses.Add(edgeUse);
    }

    public IEnumerable<Vertex> GetVertices()
    {
        foreach (EdgeUse edgeUse in EdgeUses)
        {
            if (edgeUse.StartVertex == null) continue;
            yield return edgeUse.StartVertex!;
        }
    }

    public override string ToString()
    {
        if (EdgeUses.Count == 0)
            return $"f{Id}, {EdgeUses[0].Edge}";
        return $"f{Id}: {string.Join(", ", EdgeUses.Select(eu => eu.StartVertex))}";
    }

    /// <summary>
    /// Reverses the orientation of the face. After flipping, the normal of the face will point in the opposite direction.
    /// The surface, the edges and the vertices will not be changed. Only the edge uses will be reversed.
    /// </summary>
    internal void Flip()
    {
        SameSenseAsSurface = !SameSenseAsSurface;
        foreach (EdgeUse edgeUse in EdgeUses)
            edgeUse.SameSenseAsEdge = !edgeUse.SameSenseAsEdge;
        _edgeUses.Reverse();
    }

    public Xyz GetPointOnFace()
    {
        if (EdgeUses.Count == 1)
        {
            EdgeUse edgeUse = EdgeUses[0];
            switch (edgeUse.Edge.Curve)
            {
                case Circle circle:
                    return circle.Center;

                default:
                    throw new NotImplementedException();
            }
        }
        // TODO We should project the face boundary into parameter space and use the algorithm to find the polygon pole of inaccessibility.
        if (Surface is Plane)
        {
            Xyz baryCenter = Xyz.Zero;
            Vertex[] vertices = GetVertices().ToArray();
            int count = vertices.Length;
            foreach (Vertex v in vertices)
                baryCenter += v.Position / count;
            return baryCenter;
        }
        Vertex? vertex = EdgeUses[0].StartVertex;
        if (vertex == null) throw new NullReferenceException($"{nameof(vertex)} is null.");
        return vertex.Position;
    }

    public Xyz GetNormal(Xyz point)
    {
        Xyz normal = Surface.GetNormal(point);
        if (SameSenseAsSurface) return normal;
        return normal.Negate();
    }

    public PointContainment Contains(Xyz point)
    {
        if (EdgeUses.Count == 1)
        {
            Curve? curve = EdgeUses[0].Edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            return curve switch
            {
                Circle circle => circle.Contains(point),
                _ => throw new NotImplementedException("Only circular faces are supported for point containment check.")
            };
        }
        throw new NotImplementedException();
    }

    internal double GetDistanceToBoundary(Xyz point)
    {
        if (EdgeUses.Count == 1)
        {
            Curve? curve = EdgeUses[0].Edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            return curve.GetDistanceTo(point);
        }

        throw new NotImplementedException("Distance to boundary is only implemented for single edge faces.");
    }

    internal List<Node> ProjectBoundaryToParameterSpace(Mesh mesh, double chordHeight, bool fillEdgeIndices = false)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        // if (EdgeUses.Count == 1)
        // {
        //     Edge edge = EdgeUses[0].Edge;
        //     int[] indices = edgesIndices[edge];
        //     Curve? curve = edge.Curve;
        //     if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
        //     switch (curve)
        //     {
        //         case Circle circle:
        //             {
        //                 IList<Xyz> strokePoints = circle.GetStrokePoints(chordHeight);
        //                 CoordinateSystem coordinateSystem = circle.Center.GetCoordinateSystem(circle.Normal);
        //                 var nodes = new List<Node>(strokePoints.Count);
        //                 for (int i = 0; i < strokePoints.Count; i++)
        //                 {
        //                     Xyz point = strokePoints[i];
        //                     Uv uv = coordinateSystem.Convert2d(point);
        //                     var node = new Node(indices[i], uv);
        //                     nodes.Add(node);
        //                 }
        //                 return nodes;
        //             }
        //
        //         default:
        //             throw new NotImplementedException("Projecting boundary to parameter space is only implemented for circular edges.");
        //     }
        // }

        // TODO Handle different curves, this code only works for straight edges.
        {
            var points = new List<Xyz>();
            var indices = new List<int>();
            EdgeUse lastEdgeUse = EdgeUses[^1];
            int firstPointIndex = mesh.Positions.Count;
            foreach (EdgeUse edgeUse in EdgeUses)
            {
                Edge edge = edgeUse.Edge;
                int pointIndex = mesh.Positions.Count;
                if (fillEdgeIndices)
                {
                    int[] edgeIndices = new int[2];
                    edgeIndices[0] = pointIndex;
                    if (edgeUse == lastEdgeUse)
                    {
                        // Last EdgeUse, we need to connect the last point to the first point.
                        edgeIndices[1] = firstPointIndex;
                    }
                    else
                    {
                        // Not the last EdgeUse, we can just take the next point.
                        edgeIndices[1] = pointIndex + 1;
                    }
                    mesh.EdgesIndices.TryAdd(edge, edgeIndices);
                }
                indices.Add(pointIndex);
                Xyz position = edgeUse.StartVertex!.Position;
                mesh.Positions.Add(position);
                points.Add(position);
                Xyz normal = Surface.GetNormal(position);
                mesh.Normals.Add(normal);
            }
            IEnumerable<Uv> uvs = Surface.GetParametersAtPoints(points);
            var nodes = new List<Node>();
            for (int i = 0; i < points.Count; i++)
            {
                Uv uv = uvs.ElementAt(i);
                var node = new Node(indices[i], uv);
                nodes.Add(node);
            }
            return nodes;
        }
        throw new NotImplementedException("Projecting boundary to parameter space is only implemented for single edge faces.");
    }
}