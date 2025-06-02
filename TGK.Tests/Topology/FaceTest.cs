using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Topology;

namespace TGK.Tests.Topology;

[TestFixture]
[TestOf(typeof(Face))]
public class FaceTest
{
    [Test]
    public void TestProjectBoundaryToParameterSpaceSingleFace()
    {
        var solid = new Solid();

        // Create a triangular face in the XY plane
        Face face = solid.AddPlanarFace([
            Xyz.Zero,
            new Xyz(1, 0, 0),
            new Xyz(0, 1, 0),
        ]);
        var mesh = new Mesh();
        List<Node> nodes = face.ProjectBoundaryToParameterSpace(mesh, 0.1);
        Assert.That(nodes, Has.Count.EqualTo(3), "Expected 3 nodes for the triangular face.");
        Assert.That(nodes.Select(n => n.WorldPositionIndex), Is.EquivalentTo([0, 1, 2]));
    }

    [Test]
    public void TestProjectBoundaryToParameterSpaceBox()
    {
        var cube = Solid.CreateBox(2, 2, 2);
        var mesh = new Mesh();
        List<Node> nodes0 = cube.Faces.First().ProjectBoundaryToParameterSpace(mesh, 0.1);
        Assert.That(nodes0, Has.Count.EqualTo(4), "Expected 4 nodes for the first face of the cube.");
        Assert.That(nodes0.Select(n => n.WorldPositionIndex), Is.EquivalentTo([0, 1, 2, 3]));

        List<Node> nodes1 = cube.Faces.ElementAt(1).ProjectBoundaryToParameterSpace(mesh, 0.1);
        Assert.That(nodes1, Has.Count.EqualTo(4), "Expected 4 nodes for the second face of the cube.");
        Assert.That(nodes1.Select(n => n.WorldPositionIndex), Is.EquivalentTo([4, 5, 6, 7]));

        // 4 vertices per face, 2 faces = 8 vertices. We cannot share vertices between faces because the normal directions differ.
        Assert.That(mesh.Positions, Has.Count.EqualTo(8), "Expected 8 positions for the two faces.");
        Assert.That(mesh.Normals, Has.Count.EqualTo(8), "Expected 8 normals for the two faces.");

        IEnumerable<int> indices = nodes0.Concat(nodes1).Select(n => n.WorldPositionIndex);
        Assert.That(indices.Distinct(), Is.EquivalentTo(Enumerable.Range(0, 8)), "Expected indices to be in the range of 0 to 7.");
    }

    [Test]
    public void TestCalculateAreaPlanarFace()
    {
        var solid = new Solid();
        Face face = solid.AddPlanarFace([Xyz.Zero, new Xyz(1, 0, 0), new Xyz(0.5, 2, 0)]);
        double area = face.CalculateArea(0.1);
        Assert.That(area, Is.EqualTo(1).Within(1e-6), "Expected area of the triangular face to be 1.");
    }

    [Test]
    public void TestCalculateAreaCube()
    {
        var cube = Solid.CreateBox(10);
        double area = cube.Faces.First().CalculateArea(0.1);
        Assert.That(area, Is.EqualTo(100).Within(1e-6), "Expected area of the first face of the cube to be 100.");
    }
}