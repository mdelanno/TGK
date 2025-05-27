using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TGK.Geometry;
using TGK.Geometry.Curves;
using TGK.Geometry.Surfaces;

namespace TGK.Tests.Geometry;

[TestFixture]
[TestOf(typeof(Plane))]
public class PlaneTest
{
    [Test]
    public void TestGetSignedDistance()
    {
        var plane = new Plane(Xyz.ZAxis, 1);
        var pointAbove = new Xyz(0, 0, 2);
        var pointBelow = new Xyz(0, 0, 0);
        Assert.That(plane.GetSignedDistanceTo(pointAbove), Is.EqualTo(1));
        Assert.That(plane.GetSignedDistanceTo(pointBelow), Is.EqualTo(-1));
    }

    [Test]
    public void TestIntersectWithLine()
    {
        var origin = new Xyz(1, 2, 3);
        var line = new Line(origin, Xyz.ZAxis);
        IEnumerable<CurveSurfaceIntersectionResult> results = Plane.XY.IntersectWith(line);
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(1));
        CurveSurfaceIntersectionResult result = results.First();
        Assert.That(result, Is.InstanceOf<PointCurveSurfaceIntersectionResult>());
        var pointResult = (PointCurveSurfaceIntersectionResult)result;
        var expected = new Xyz(1, 2, 0);
        Assert.That(pointResult.Point.IsAlmostEqualTo(expected));
    }
}