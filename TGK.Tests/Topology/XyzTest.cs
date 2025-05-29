using NUnit.Framework;
using TGK.Geometry;
using static System.Math;

namespace TGK.Tests.Topology;

[TestFixture]
[TestOf(typeof(Xyz))]
public class XyzTest
{
    [Test]
    public void TestToString()
    {
        var xyz = new Xyz(1, 2, 3.6);
        string result = xyz.ToString();
        Assert.That(result, Is.EqualTo("(1, 2, 3.6)"));
    }

    [Test]
    public void TestGetAngleTo()
    {
        Assert.That(Xyz.XAxis.GetAngleTo(Xyz.YAxis), Is.EqualTo(PI / 2.0));
    }

    [Test]
    public void TestGetCoordinateSystem()
    {
        CoordinateSystem coordinateSystem = Xyz.Zero.GetCoordinateSystem(Xyz.ZAxis);
        Assert.That(coordinateSystem.XAxis, Is.EqualTo(Xyz.XAxis));
        Assert.That(coordinateSystem.YAxis, Is.EqualTo(Xyz.YAxis));
    }
}