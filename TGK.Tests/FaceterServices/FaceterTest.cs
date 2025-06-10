using EmptyFiles;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TGK.Dxf;
using TGK.FaceterServices;
using TGK.Geometry;
using TGK.Geometry.Surfaces;
using TGK.Topology;
using VerifyNUnit;
using VerifyTests;

namespace TGK.Tests.FaceterServices;

[TestFixture]
[TestOf(typeof(Faceter))]
public class FaceterTest
{
    static FaceterTest()
    {
        FileExtensions.AddTextExtension("dxf");
        VerifierSettings.UseUtf8NoBom();
    }

    [Test]
    public void TestCube()
    {
        var cube = Solid.CreateBox(2, 2, 2);
        Mesh mesh = Faceter.FacetSolid(cube, chordHeight: 0.1);

        // 4 vertices per face, 6 faces = 24 vertices. We cannot share vertices between faces because the normal directions differ.
        Assert.That(mesh.Positions, Has.Count.EqualTo(24), "Expected 24 vertices for the cube mesh.");

        // One normal per vertex, 24 vertices = 24 normals.
        Assert.That(mesh.Normals, Has.Count.EqualTo(24), "Expected 24 normals for the cube mesh.");

        // Each face has 2 triangles, 6 faces = 12 triangles, 3 indices per triangle = 36 indices.
        IEnumerable<int> triangleIndices = mesh.TriangleIndices.Values.SelectMany(v => v);
        Assert.That(triangleIndices.Count(), Is.EqualTo(36), "Expected 36 indices for the cube mesh.");

        Assert.That(triangleIndices.Distinct(), Is.EquivalentTo(Enumerable.Range(0, 24)), "Expected indices to be in the range of 0 to 23.");
    }

    [Test(Description = "Two faces sharing an edge in a L-shape configuration.")]
    public void TestGetLMesh()
    {
        var solid = new Solid();

        // Face in the XY plane.
        solid.AddPlanarFace([
            new Xyz(0, 0, 0),
            new Xyz(0, -1, 0),
            new Xyz(1, -1, 0),
            new Xyz(1, 0, 0)
        ]);

        // Face in the XZ plane.
        Vertex v4 = solid.AddVertex(new Xyz(1, 0, 1));
        Vertex v5 = solid.AddVertex(new Xyz(0, 0, 1));
        Edge e4 = solid.AddEdge(solid.Vertices.ElementAt(3), v4);
        Edge e5 = solid.AddEdge(v4, v5);
        Edge e6 = solid.AddEdge(v5, solid.Vertices.ElementAt(0));
        var faceInXZ = new Face(solid.Faces.Count, Plane.ZX);
        solid.Faces.Add(faceInXZ);
        faceInXZ.AddEdgeUse(solid.Edges.ElementAt(3), sameSenseAsEdge: false);
        faceInXZ.AddEdgeUse(e4);
        faceInXZ.AddEdgeUse(e5);
        faceInXZ.AddEdgeUse(e6);

        Mesh mesh = Faceter.FacetSolid(solid, chordHeight: 0.1);

        // 4 vertices for the XY face, 4 vertices for the XZ face = 8 vertices.
        Assert.That(mesh.Positions, Has.Count.EqualTo(8), "Expected 8 vertices for the L-shape mesh.");

        // 4 normals for the XY face, 4 normals for the XZ face = 8 normals.
        Assert.That(mesh.Normals, Has.Count.EqualTo(8), "Expected 8 normals for the L-shape mesh.");

        // Each face has 2 triangles, 2 faces = 4 triangles, 3 indices per triangle = 12 indices.
        IEnumerable<int> triangleIndices = mesh.TriangleIndices.Values.SelectMany(v => v);
        Assert.That(triangleIndices.Count(), Is.EqualTo(12), "Expected 12 indices for the L-shape mesh.");

        Assert.That(triangleIndices.Distinct(), Is.EquivalentTo(Enumerable.Range(0, 8)), "Expected indices to be in the range of 0 to 7.");
    }

    [Test]
    public Task TestCylinder()
    {
        var cylinder = Solid.CreateCylinder(radius: 10, height: 20);
        Mesh mesh = Faceter.FacetSolid(cylinder, chordHeight: 0.1);
        var writer = new StringWriter(CultureInfo.InvariantCulture);
        var dxfWriter = new DxfWriter(writer);

        foreach ((Face face, int[] indices) in mesh.TriangleIndices)
        {
            for (int i = 0; i < indices!.Length; i += 3)
            {
                var polyline = new Polyline3d
                {
                    IsClosed = true
                };
                dxfWriter.Entities.Add(polyline);
                polyline.Vertices.Add(new PolylineVertex3d { Position = mesh.Positions[indices[i]] });
                polyline.Vertices.Add(new PolylineVertex3d { Position = mesh.Positions[indices[i + 1]] });
                polyline.Vertices.Add(new PolylineVertex3d { Position = mesh.Positions[indices[i + 2]] });

                Xyz baryCenter = polyline.CalculateBaryCenter();
                Xyz normalVector = polyline.CalculateNormalVector();

                // Alignment points must be in OCS (Object Coordinate System) of the text entity.
                // Z is the elevation of the text.
                CoordinateSystem coordinateSystem = Xyz.Zero.GetCoordinateSystem(normalVector);
                var xy = new Plane(baryCenter, baryCenter + coordinateSystem.XAxis, baryCenter + coordinateSystem.YAxis);
                double elevation = -xy.GetSignedDistanceTo(Xyz.Zero);
                var baryCenterInOcs = coordinateSystem.Convert2d(baryCenter).ToXyz(elevation);
                var text = new DbText($"f{face!.Id}, t{i / 3}")
                {
                    Height = 0.2,
                    FirstAlignmentPoint = baryCenterInOcs,
                    SecondAlignmentPoint = baryCenterInOcs,
                    NormalVector = normalVector,
                    HorizontalJustification = HorizontalJustification.Center,
                    VerticalJustification = VerticalJustification.Middle
                };
                dxfWriter.Entities.Add(text);
            }
        }
        dxfWriter.Write();
        string dxf = writer.ToString();
        return Verifier.Verify(dxf, extension: "dxf");
    }
}