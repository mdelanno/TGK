﻿using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;
using static System.Math;

namespace TGK.Topology;

public sealed class Face : BRepEntity
{
    readonly List<EdgeUse> _edgeUses = [];

    public Surface Surface { get; }

    internal IReadOnlyList<EdgeUse> EdgeUses { get; }

    public bool SameSenseAsSurface { get; set; }

    internal Face(int id, Surface surface, bool sameSenseAsSurface = true) : base(id)
    {
        ArgumentNullException.ThrowIfNull(surface);

        Surface = surface;
        SameSenseAsSurface = sameSenseAsSurface;
        EdgeUses = _edgeUses.AsReadOnly();
    }

    internal void AddEdgeUse(Edge edge, bool sameSenseAsEdge = true)
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
            yield return edgeUse.StartVertex!;
        }
    }

    public override string ToString()
    {
        if (EdgeUses.Count == 1)
            return $"f{Id}, {EdgeUses[0].Edge}";
        if (EdgeUses.Count <= 4)
            return $"f{Id}: {string.Join(", ", EdgeUses.Select(eu => eu.StartVertex))}";
        return $"f{Id}: {EdgeUses[0].StartVertex}, {EdgeUses[1].StartVertex}, ..., {EdgeUses[^2].StartVertex}, {EdgeUses[^1].StartVertex}";
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

    public Xyz GetNormal(in Xyz point)
    {
        Xyz normal = Surface.GetNormal(point);
        if (SameSenseAsSurface) return normal;
        return normal.Negate();
    }

    public PointContainment Contains(in Xyz point)
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

    internal double GetDistanceToBoundary(in Xyz point)
    {
        if (EdgeUses.Count == 1)
        {
            Curve? curve = EdgeUses[0].Edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            return curve.GetDistanceTo(point);
        }

        throw new NotImplementedException("Distance to boundary is only implemented for single edge faces.");
    }

    internal List<Node> ProjectBoundaryToParameterSpace(Mesh mesh, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        if (EdgeUses.Count == 1)
        {
            Edge edge = EdgeUses[0].Edge;
            Curve? curve = edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            switch (curve)
            {
                case Circle circle:
                    {
                        Xyz[] strokePoints = circle.GetStrokePoints(chordHeight);
                        CoordinateSystem coordinateSystem = circle.Center.GetCoordinateSystem(circle.Normal);
                        var nodes = new List<Node>(strokePoints.Length);
                        int[]? edgeIndices = null;
                        for (int i = 0; i < strokePoints.Length; i++)
                        {
                            Xyz point = strokePoints[i];
                            Uv uv = coordinateSystem.Convert2d(point);
                            int pointIndex = mesh.Positions.Count;
                            if (edgeIndices != null)
                                edgeIndices[i] = pointIndex;
                            mesh.Positions.Add(point);
                            mesh.Normals.Add(circle.Normal);
                            var node = new Node(pointIndex, uv);
                            nodes.Add(node);
                        }
                        return nodes;
                    }

                default:
                    throw new NotImplementedException("Projecting boundary to parameter space is only implemented for circular edges.");
            }
        }

        {
            var nodes = new List<Node>();

            // For each edge use, we omit the last point to make a loop.
            foreach (EdgeUse edgeUse in EdgeUses)
            {
                Edge edge = edgeUse.Edge;
                int pointIndex = mesh.Positions.Count;
                switch (edge.Curve)
                {
                    case Circle circle:
                        {
                            if (Surface is Cylinder cylinder && cylinder.Axis.Direction.IsParallelTo(circle.Normal))
                            {
                                Xyz[] strokePoints = circle.GetStrokePoints(chordHeight);
                                Uv[] parametricSpacePolyline = cylinder.ProjectCurveToParametricSpace(circle, chordHeight);
                                for (int i = 0; i < strokePoints.Length; i++)
                                {
                                    int j = edgeUse.SameSenseAsEdge ? i : strokePoints.Length - i - 1;
                                    int k = edgeUse.SameSenseAsEdge ? i : strokePoints.Length - i;
                                    Xyz point = strokePoints[j];
                                    mesh.Positions.Add(point);
                                    mesh.Normals.Add(cylinder.GetNormal(point));
                                    var node = new Node(pointIndex + i, parametricSpacePolyline[k]);
                                    nodes.Add(node);
                                }
                            }
                            else
                                throw new NotImplementedException();
                            break;
                        }

                    case null:
                        {
                            // Straight edge
                            Xyz point = edgeUse.StartVertex.Position;
                            Uv parametricSpacePosition = Surface.GetParametersAtPoint(point);
                            if (edge.Flags.HasFlag(EdgeFlags.Seam) && edgeUse.SameSenseAsEdge)
                                parametricSpacePosition = new Uv(double.Tau, parametricSpacePosition.V);
                            var node = new Node(pointIndex, parametricSpacePosition);
                            nodes.Add(node);
                            mesh.Positions.Add(point);
                            mesh.Normals.Add(Surface.GetNormal(point));
                            break;
                        }
                }
            }
            return nodes;
        }
        throw new NotImplementedException("Projecting boundary to parameter space is only implemented for single edge faces.");
    }

    public double CalculateArea(double chordHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        if (EdgeUses.Count == 1)
        {
            Edge edge = EdgeUses[0].Edge;
            Curve? curve = edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            switch (curve)
            {
                case Circle circle:
                    {
                        return circle.CalculateArea();
                    }

                default:
                    throw new NotImplementedException("Calculating area is only implemented for circular faces.");
            }
        }

        if (Surface is Plane plane)
        {
            // When the surface is a plane, we can calculate the area by discretizing the edges, project the 3D vertices on the plane
            // and calculating the resulting polygon area.
            return CalculateArea(plane, chordHeight);
        }

        throw new NotImplementedException();
    }

    double CalculateArea(Plane plane, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(plane);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        List<Uv> vertices = [];
        foreach (EdgeUse edgeUse in EdgeUses)
        {
            Edge edge = edgeUse.Edge;
            Curve? curve = edge.Curve;
            switch (curve)
            {
                case null:
                    // If the edge is a straight line, we can just add the start vertex.
                    vertices.Add( plane.CoordinateSystem.Convert2d(edgeUse.StartVertex.Position));
                    break;

                case Circle circle:
                    Xyz[] strokePoints = circle.GetStrokePoints(chordHeight);
                    // Exclude the last point to avoid duplicates
                    vertices.AddRange(strokePoints[..^1].Select(xyz => plane.CoordinateSystem.Convert2d(xyz)));
                    break;

                default:
                    throw new NotImplementedException("Calculating area is only implemented for circular edges.");
            }
        }
        var polygon = new Polygon(vertices, DirectionOfRotation.CounterClockwise, isSimple: true);
        return Abs(polygon.CalculateSignedArea());
    }
}