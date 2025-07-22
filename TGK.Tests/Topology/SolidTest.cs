using EmptyFiles;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TGK.Geometry;
using TGK.Geometry.Surfaces;
using TGK.Topology;

namespace TGK.Tests.Topology;

[TestFixture]
[TestOf(typeof(Solid))]
public class SolidTest
{
    static SolidTest()
    {
        FileExtensions.AddTextExtension("dxf");
    }

    [Test]
    public void TestAddPlanarFace()
    {
        var solid = new Solid();
        solid.AddPlanarFace([
            new Xyz(-1, -1, 0),
            new Xyz(1, -1, 0),
            new Xyz(1, 1, 0),
            new Xyz(-1, 1, 0)
        ]);
        Assert.That(solid.Faces.Count, Is.EqualTo(1));
        Assert.That(solid.Vertices.Count, Is.EqualTo(4));
        Assert.That(solid.Edges.Count, Is.EqualTo(4));
        Face face = solid.Faces.First();
        Assert.That(face.Surface, Is.InstanceOf<Plane>());
        Assert.That(face.EdgeUses.Count, Is.EqualTo(4));
    }

    [Test]
    public void TestCreateCylinder()
    {
        var cylinder = Solid.CreateCylinder(radius: 10, height: 20);
        Assert.That(cylinder.Faces, Has.Count.EqualTo(3));
        IEnumerable<Face> cylindricalFaces = cylinder.Faces.Where(f => f.Surface is Cylinder);
        Assert.That(cylindricalFaces.Count(), Is.EqualTo(1));
        Face cylindricalFace = cylindricalFaces.Single();
        Assert.That(cylindricalFace.EdgeUses.Count, Is.EqualTo(4));

        // The bottom edge use and the left edge use of the cylindrical face are in the same sense as the edge. The two other edge uses are in
        // the opposite sense.
        Assert.That(cylindricalFace.EdgeUses.Count(eu => eu.SameSenseAsEdge), Is.EqualTo(2));
    }

    [Test]
    public void TestCreateSphere()
    {
        var sphere = Solid.CreateSphere(radius: 10);
        Assert.That(sphere.Faces, Has.Count.EqualTo(1), "Expected 1 face for the sphere.");
        Face sphericalFace = sphere.Faces.Single();
        Assert.That(sphericalFace.Surface, Is.InstanceOf<Sphere>(), "Expected the face to be a spherical surface.");
        Assert.That(sphericalFace.EdgeUses.Count, Is.EqualTo(4), "Expected 4 edge uses for the spherical face.");
        Assert.That(sphericalFace.EdgeUses.Count(eu => eu.SameSenseAsEdge), Is.EqualTo(3), "Expected 3 edge uses in the same sense as the edge.");
        Assert.That(sphericalFace.EdgeUses.Count(eu => !eu.SameSenseAsEdge), Is.EqualTo(1), "Expected 1 edge uses in the opposite sense as the edge.");
        IEnumerable<Vertex> vertices = sphericalFace.GetVertices();
        Assert.That(vertices.Count(), Is.EqualTo(2), "Expected 2 vertices for the spherical face.");
    }
}