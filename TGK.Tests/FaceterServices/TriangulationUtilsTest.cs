using NUnit.Framework;
using System.Collections.Generic;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Topology;

namespace TGK.Tests.FaceterServices;

[TestFixture]
[TestOf(typeof(TriangulationUtils))]
public class TriangulationUtilsTest
{
    [Test]
    public void TestEarClippingTriangle()
    {
        var polygon = new NodeListAdapter();
        polygon.Set([
            new Node(12, new Uv(0, 0)),
            new Node(17, new Uv(1, 0)),
            new Node(21, new Uv(0, 1))
        ]);
        int[] triangleIndices = TriangulationUtils.EarClipping(polygon);
        Assert.That(triangleIndices, Is.EqualTo([12, 17, 21]), "Expected triangle indices for a triangle.");
    }

    [Test]
    public void TestEarClippingRectangle()
    {
        var polygon = new NodeListAdapter();
        polygon.Set([
            new Node(12, new Uv(0, 0)),
            new Node(17, new Uv(1, 0)),
            new Node(21, new Uv(1, 1)),
            new Node(22, new Uv(0, 1))
        ]);
        int[] triangleIndices = TriangulationUtils.EarClipping(polygon);
        Assert.That(triangleIndices, Is.EqualTo([12, 17, 21, 21, 22, 12]), "Expected triangle indices for a rectangle.");
    }

    [Test]
    public void TestEarClippingConvexPentagon()
    {
        var polygon = new NodeListAdapter();
        polygon.Set([
            new Node(12, new Uv(0, 0)),
            new Node(17, new Uv(1, 0)),
            new Node(21, new Uv(1, 1)),
            new Node(22, new Uv(0.5, 1.5)),
            new Node(23, new Uv(0, 1))
        ]);
        int[] triangleIndices = TriangulationUtils.EarClipping(polygon);
        Assert.That(triangleIndices.Length, Is.EqualTo(9), "Expected triangle indices for a pentagon.");
    }

    [Test]
    public void TestEarClippingT()
    {
        var solid = new Solid();

        // Draw a T-shape face
        Face face = solid.AddPlanarFace([
            Xyz.Zero,
            new Xyz(1, 0, 0),
            new Xyz(1, 4, 0),
            new Xyz(3, 4, 0),
            new Xyz(3, 5, 0),
            new Xyz(-2, 5, 0),
            new Xyz(-2, 4, 0),
            new Xyz(0, 4, 0)
        ]);

        List<Node> nodes = face.ProjectBoundaryToParameterSpace(new Mesh(), 0.1);
        var polygon = new NodeListAdapter();
        polygon.Set(nodes);
        int[] triangleIndices = TriangulationUtils.EarClipping(polygon);

        // 8 vertices in the T-shape face, which should result in 6 triangles (18 indices).
        Assert.That(triangleIndices.Length, Is.EqualTo(18), "Expected triangle indices for a T-shape face.");
    }
}