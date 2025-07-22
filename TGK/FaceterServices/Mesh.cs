using System.Diagnostics;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;
using TGK.Topology;

namespace TGK.FaceterServices;

public sealed class Mesh
{
    readonly double _chordHeight;

    public List<Xyz> Positions { get; } = [];

    public List<Xyz> Normals { get; } = [];

    public Dictionary<Face, int[]> TriangleIndices { get; } = [];

    public Mesh(double chordHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chordHeight);

        _chordHeight = chordHeight;
    }

    public void AddSolid(Solid solid)
    {
        ArgumentNullException.ThrowIfNull(solid);

        var adapter = new NodeListAdapter();
        foreach (Face face in solid.Faces)
        {
            List<Node> nodes = ProjectFaceBoundaryToParameterSpace(face);
            adapter.Set(nodes);
            int[] triangleIndices = TriangulationUtils.EarClipping(adapter);
            TriangleIndices.Add(face, triangleIndices);
        }

        Debug.Assert(Positions.Count == Normals.Count, "Mesh positions and normals count should match.");
    }

    internal List<Node> ProjectFaceBoundaryToParameterSpace(Face face)
    {
        ArgumentNullException.ThrowIfNull(face);

        if (face.EdgeUses.Count == 1)
        {
            EdgeUse edgeUse = face.EdgeUses[0];
            Edge edge = edgeUse.Edge;
            Curve? curve = edge.Curve;
            if (curve == null) throw new NullReferenceException($"{nameof(curve)} is null");
            return curve switch
            {
                Circle circle => ProjectCircle(face, edgeUse, circle),
                _ => throw new NotImplementedException("Projecting boundary to parameter space is only implemented for circular edges.")
            };
        }

        {
            var nodes = new List<Node>();

            // For each edge use, we omit the last point to make a loop.
            foreach (EdgeUse edgeUse in face.EdgeUses)
            {
                Edge edge = edgeUse.Edge;
                if (edge.IsPole)
                {
                    // Projecting a pole is like projecting the equatorial circle on a cylinder.
                    var sphere = (Sphere)face.Surface;
                    var cylinderAxis = new Line(sphere.Center, sphere.AxisDirection);
                    var cylinder = new Cylinder(cylinderAxis, sphere.Radius);
                    var circle = new Circle(sphere.Center, sphere.AxisDirection, sphere.Radius);
                    if (edge.StartVertex.Position.IsAlmostEqualTo(sphere.SouthPole))
                    {
                        // South Pole
                        circle.TranslateBy(sphere.AxisDirection * -Math.PI / 2);
                        ProjectCircleOnCylinder(face, cylinder, edgeUse, circle, nodes);
                    }
                    else
                    {
                        // North Pole
                        circle.TranslateBy(sphere.AxisDirection * Math.PI / 2);
                        ProjectCircleOnCylinder(face, cylinder, edgeUse, circle, nodes);
                    }
                }
                else
                {
                    switch (edge.Curve)
                    {
                        case Circle circle:
                            {
                                switch (face.Surface)
                                {
                                    case Cylinder cylinder:
                                        ProjectCircleOnCylinder(face, cylinder, edgeUse, circle, nodes);
                                        break;

                                    case Sphere sphere:
                                        ProjectCircleOnSphere(face, sphere, edgeUse, circle, nodes);
                                        break;

                                    default:
                                        throw new NotImplementedException();
                                }
                                break;
                            }

                        case null:
                            {
                                // Straight edge
                                ProjectStraightEdge(face, edgeUse, edge, nodes);
                                break;
                            }
                    }
                }
            }
            return nodes;
        }
    }

    void ProjectStraightEdge(Face face, EdgeUse edgeUse, Edge edge, List<Node> nodes)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(edgeUse);
        ArgumentNullException.ThrowIfNull(edge);
        if (edge.Curve != null)
            throw new InvalidOperationException("Edge curve should be null for straight edges.");
        ArgumentNullException.ThrowIfNull(nodes);

        Xyz point = edgeUse.StartVertex.Position;
        Uv parametricSpacePosition = face.Surface.GetParametersAtPoint(point);
        if (edge.IsSeam && edgeUse.SameSenseAsEdge)
            parametricSpacePosition = new Uv(double.Tau, parametricSpacePosition.V);
        var node = new Node(Positions.Count, parametricSpacePosition);
        nodes.Add(node);
        Positions.Add(point);
        Normals.Add(face.Surface.GetNormal(point));
    }

    void ProjectCircleOnCylinder(Face face, Cylinder cylinder, EdgeUse edgeUse, Circle circle, List<Node> nodes)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(cylinder);
        ArgumentNullException.ThrowIfNull(edgeUse);
        ArgumentNullException.ThrowIfNull(circle);
        ArgumentNullException.ThrowIfNull(nodes);

        if (!cylinder.Axis.Direction.IsParallelTo(circle.Normal))
            throw new InvalidOperationException("Circle is not aligned with the cylinder axis.");

        Xyz[] points3d = circle.GetStrokePoints(_chordHeight);
        Uv[] points2d = cylinder.ProjectCurveToParametricSpace(circle, _chordHeight);
        if (!edgeUse.SameSenseAsEdge)
        {
            // It's a bit complicated here: you have to go through the points in reverse order, but you also have to
            // start at u = 2 Pi and omit u = zero. So we shift the 2D points to the left with Array.Copy(),
            // then add the last point of the seam at the end.
            Array.Copy(points2d, 1, points2d, 0, points2d.Length - 1);
            EdgeUse previousEdgeUse = face.GetPreviousEdgeUse(edgeUse);
            Uv uv = cylinder.GetParametersAtPoint(previousEdgeUse.EndVertex.Position);
            points2d[^1] = new Uv(double.Tau, uv.V);
            Array.Reverse(points2d);

            // We do the same with 3D points.
            Array.Copy(points3d, 1, points3d, 0, points3d.Length - 1);
            points3d[^1] = previousEdgeUse.EndVertex.Position;
            Array.Reverse(points3d);
        }
        int pointIndex = Positions.Count;
        for (int i = 0; i < points3d.Length; i++)
        {
            Xyz point = points3d[i];
            Positions.Add(point);
            Normals.Add(cylinder.GetNormal(point));
            var node = new Node(pointIndex + i, points2d[i]);
            nodes.Add(node);
        }
    }

    void ProjectCircleOnSphere(Face face, Sphere sphere, EdgeUse edgeUse, Circle circle, List<Node> nodes)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(sphere);
        ArgumentNullException.ThrowIfNull(edgeUse);
        ArgumentNullException.ThrowIfNull(circle);
        ArgumentNullException.ThrowIfNull(nodes);

        Xyz[] points3d = circle.GetStrokePoints(_chordHeight);
        Uv[] points2d = sphere.ProjectCurveToParametricSpace(circle, _chordHeight);
        
        if (!edgeUse.SameSenseAsEdge)
        {
            Array.Reverse(points3d);
            Array.Reverse(points2d);
        }
        
        int pointIndex = Positions.Count;
        for (int i = 0; i < points3d.Length; i++)
        {
            Xyz point = points3d[i];
            Positions.Add(point);
            Normals.Add(sphere.GetNormal(point));
            var node = new Node(pointIndex + i, points2d[i]);
            nodes.Add(node);
        }
    }

    List<Node> ProjectCircle(Face face, EdgeUse edgeUse, Circle circle)
    {
        ArgumentNullException.ThrowIfNull(face);
        ArgumentNullException.ThrowIfNull(edgeUse);
        ArgumentNullException.ThrowIfNull(circle);

        Xyz[] strokePoints = circle.GetStrokePoints(_chordHeight);
        if (!edgeUse.SameSenseAsEdge)
            Array.Reverse(strokePoints);
        CoordinateSystem coordinateSystem = circle.Center.GetCoordinateSystem(face.GetNormal(circle.Center));
        var nodes = new List<Node>(strokePoints.Length);
        foreach (Xyz point in strokePoints)
        {
            Uv uv = coordinateSystem.Convert2d(point);
            int pointIndex = Positions.Count;
            Positions.Add(point);
            Normals.Add(face.GetNormal(point));
            var node = new Node(pointIndex, uv);
            nodes.Add(node);
        }
        return nodes;
    }
}