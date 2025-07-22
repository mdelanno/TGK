using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Tests.Geometry.Surfaces;

[TestFixture]
[TestOf(typeof(Cylinder))]
public class CylinderTest
{
    [Test]
    public void TestProjectCircleToParameterSpace()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        const double RADIUS = 10;
        var cylinder = new Cylinder(axis, RADIUS);
        var circle = new Circle(Xyz.Zero, Xyz.ZAxis, RADIUS);
        const double CHORD_HEIGHT = 0.1;
        Uv[] parameters = cylinder.ProjectCurveToParametricSpace(circle, CHORD_HEIGHT);
        Assert.That(parameters, Has.Length.EqualTo(23));
        Assert.That(parameters[0], Is.EqualTo(new Uv(0, 0)));
        Assert.That(parameters[1].IsAlmostEqualTo(new Uv(double.Tau / 23, 0)));
        Assert.That(parameters.All(p => p.V.IsAlmostEqualTo(0)), Is.True,
            "All V parameters should be zero since the center of the circle has its z at zero.");
    }

    [Test]
    public void TestIntersectWithLineTwoIntersections()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        var cylinder = new Cylinder(axis, radius: 10);
        var line = new Line(Xyz.Zero, Xyz.XAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = cylinder.IntersectWith(line);
        Assert.That(results.Count(), Is.EqualTo(2));
        Assert.That(results.First(), Is.InstanceOf<PointCurveSurfaceIntersectionResult>());
        Assert.That(results.Last(), Is.InstanceOf<PointCurveSurfaceIntersectionResult>());
        var pointResults = results.Cast<PointCurveSurfaceIntersectionResult>().ToList();
        Xyz[] expectedPoints = [new Xyz(-10, 0, 0), new Xyz(10, 0, 0)];
        foreach (Xyz expectedPoint in expectedPoints)
        {
            bool foundPoint = pointResults.Any(result => result!.Point.IsAlmostEqualTo(expectedPoint));
            Assert.That(foundPoint, Is.True, $"Expected point {expectedPoint} not found in results");
        }
    }

    [Test]
    public void TestIntersectWithLineNoIntersection()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        var cylinder = new Cylinder(axis, radius: 10);
        var line = new Line(new Xyz(15, 0, 0), Xyz.YAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = cylinder.IntersectWith(line);
        Assert.That(results.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TestIntersectWithLineSingleIntersection()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        var cylinder = new Cylinder(axis, radius: 10);
        var line = new Line(new Xyz(10, 0, 0), Xyz.YAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = cylinder.IntersectWith(line);
        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.First(), Is.InstanceOf<PointCurveSurfaceIntersectionResult>());
        var pointResult = (PointCurveSurfaceIntersectionResult)results.First();
        var expectedPoint = new Xyz(10, 0, 0);
        Assert.That(pointResult.Point.IsAlmostEqualTo(expectedPoint), Is.True, $"Expected point {expectedPoint}, got {pointResult.Point}");
    }

    [Test]
    public void TestIntersectWithLineParallelToAxisNoIntersection()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        var cylinder = new Cylinder(axis, radius: 10);
        var line = new Line(new Xyz(15, 0, 0), Xyz.ZAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = cylinder.IntersectWith(line);
        Assert.That(results.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TestIntersectWithLineOnCylinderSurface()
    {
        var axis = new Line(Xyz.Zero, Xyz.ZAxis);
        var cylinder = new Cylinder(axis, radius: 10);
        var line = new Line(new Xyz(10, 0, 0), Xyz.ZAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = cylinder.IntersectWith(line);
        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.First(), Is.InstanceOf<OverlapCurveSurfaceIntersectionResult>());
        var overlapResult = (OverlapCurveSurfaceIntersectionResult)results.First();
        Assert.That(overlapResult.IntervalOnCurve.IsUnbounded, Is.True);
    }
}