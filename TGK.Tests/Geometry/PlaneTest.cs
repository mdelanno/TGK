using NUnit.Framework;
using TGK.Geometry;
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
}