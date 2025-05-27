using NUnit.Framework;
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
        var circle = new Circle(center, RADIUS, Xyz.ZAxis);
        Xyz p0 = circle.GetPointAtParameter(0.0);
        Xyz p1 = circle.GetPointAtParameter(PI / 2.0);
        Xyz v0 = center.GetVectorTo(p0);
        Xyz v1 = center.GetVectorTo(p1);
        Assert.That(v0.Length, Is.EqualTo(RADIUS).Within(1e-10));
        Assert.That(v1.Length, Is.EqualTo(RADIUS).Within(1e-10));
        Assert.That(v0.GetAngleTo(v1), Is.EqualTo(PI / 2.0).Within(1e-12));
    }
}