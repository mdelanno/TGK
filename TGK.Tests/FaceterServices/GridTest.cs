using NUnit.Framework;
using System.Collections.Generic;
using TGK.FaceterServices;
using TGK.Geometry;

namespace TGK.Tests.FaceterServices;

[TestFixture]
[TestOf(typeof(Grid))]
public class GridTest
{
    [Test]
    public void TestBuild()
    {
        var grid = new Grid(Uv.Zero, rowHeight: 1);
        Grid.Cell cell0 = grid.AddColumn(1);
        Assert.That(cell0.LeftBottom.IsAlmostEqualTo(Uv.Zero), "Expected cell0 to be at (0, 0).");
        Assert.That(cell0.RightTop.IsAlmostEqualTo(new Uv(1, 1)), "Expected cell0 to be at (1, 1).");

        Grid.Cell cell1 = grid.AddColumn(2);
        {
            var expected = new Uv(1, 0);
            Assert.That(cell1.LeftBottom.IsAlmostEqualTo(expected), $"Expected: {expected}, actual: {cell1.LeftBottom}.");
        }
        Assert.That(cell1.RightTop.IsAlmostEqualTo(new Uv(2, 1)), "Expected cell1 to be at (2, 1).");

        Assert.That(cell0.East, Is.EqualTo(cell1));
        Assert.That(cell1.West, Is.EqualTo(cell0));
    }

    [Test]
    public void TestGetPolygonsNoCell()
    {
        var grid = new Grid(Uv.Zero, rowHeight: 1);
        Grid.Cell cell = grid.AddColumn(1);
        cell.Ignore = true;

        List<Polygon> polygons = grid.GetPolygons();
        Assert.That(polygons, Has.Count.EqualTo(0), "Expected no polygon.");
    }

    [Test]
    public void TestGetPolygonsSingleCell()
    {
        var grid = new Grid(Uv.Zero, rowHeight: 1);
        grid.AddColumn(1);

        List<Polygon> polygons = grid.GetPolygons();
        Assert.That(polygons, Has.Count.EqualTo(1), "Expected one polygon.");
        List<Uv> vertices = polygons[0].Vertices;
        Assert.That(vertices, Is.EqualTo([Uv.Zero, new Uv(0, 1), new Uv(1, 1), new Uv(1, 0)]));
    }

    [Test]
    public void TestGetPolygons4Cells()
    {
        var grid = new Grid(Uv.Zero, rowHeight: 1);

        // Create four cells (one row, four columns)
        grid.AddColumn(1);
        grid.AddColumn(2);
        Grid.Cell cell2 = grid.AddColumn(3);
        cell2.Ignore = true;
        grid.AddColumn(4);

        List<Polygon> polygons = grid.GetPolygons();
        Assert.That(polygons, Has.Count.EqualTo(2), "Expected 2 polygons.");
        List<Uv> vertices0 = polygons[0].Vertices;
        Assert.That(vertices0, Is.EqualTo([Uv.Zero, new Uv(0, 1), new Uv(1, 1), new Uv(2, 1), new Uv(2, 0), new Uv(1, 0)]));
        List<Uv> vertices1 = polygons[1].Vertices;
        Assert.That(vertices1, Is.EqualTo([new Uv(3, 0), new Uv(3, 1), new Uv(4, 1), new Uv(4, 0)]));
    }
}