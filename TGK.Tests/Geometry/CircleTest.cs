using NUnit.Framework;
using System.Collections.Generic;
using TGK.Geometry;
using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Tests.Geometry;

[TestFixture]
[TestOf(typeof(Circle))]
public class CircleTest
{
    [Test]
    public void TestGetPointAtParameter()
    {
        var center = new Xyz(1, 2, 3);
        const double RADIUS = 4;
        var circle = new Circle(center, Xyz.ZAxis, RADIUS);
        Xyz p0 = circle.GetPointAtParameter(0.0);
        Xyz p1 = circle.GetPointAtParameter(PI / 2.0);
        Xyz v0 = center.GetVectorTo(p0);
        Xyz v1 = center.GetVectorTo(p1);
        Assert.That(v0.Length, Is.EqualTo(RADIUS).Within(1e-10));
        Assert.That(v1.Length, Is.EqualTo(RADIUS).Within(1e-10));
        Assert.That(v0.GetAngleTo(v1), Is.EqualTo(PI / 2.0).Within(1e-12));
    }

    [Test]
    public void TestGetStrokePointsWithInterval()
    {
        var center = new Xyz(1, 2, 3);
        const double RADIUS = 4;
        var circle = new Circle(center, Xyz.ZAxis, RADIUS);
        IList<Xyz> points = circle.GetStrokePoints(0.1, 0, PI / 2);

        var expected0 = new Xyz(5, 2, 3);
        Assert.That(points[0].IsAlmostEqualTo(expected0), $"Expected: {expected0}, actual: {points[0]}.");

        var expected1 = new Xyz(1, 6, 3);
        Assert.That(points[^1].IsAlmostEqualTo(expected1), $"Expected: {expected1}, actual: {points[^1]}.");
    }
}