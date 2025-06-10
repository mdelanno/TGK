using NUnit.Framework;
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
        var circle = new Circle(Xyz.Zero, RADIUS, Xyz.ZAxis);
        const double CHORD_HEIGHT = 0.1;
        Uv[] parameters = cylinder.ProjectCurveToParametricSpace(circle, CHORD_HEIGHT);
        Assert.That(parameters, Has.Length.EqualTo(23));
        Assert.That(parameters[0], Is.EqualTo(new Uv(0, 0)));
        Assert.That(parameters[1].IsAlmostEqualTo(new Uv(double.Tau / 23, 0)));
        Assert.That(parameters.All(p => p.V.IsAlmostEqualTo(0)), Is.True,
            "All V parameters should be zero since the center of the circle has its z at zero.");
    }
}