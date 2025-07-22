using NUnit.Framework;
using TGK.Geometry;
using TGK.Geometry.Surfaces;
using static System.Math;

namespace TGK.Tests.Geometry.Surfaces;

[TestFixture]
[TestOf(typeof(Torus))]
public class TorusTest
{
    [Test]
    public void TestGetParametersAtPoint()
    {
        var torus = new Torus(new Xyz(10, 0, 0), majorRadius: 5, minorRadius: 2);
        {
            Uv uv = torus.GetParametersAtPoint(new Xyz(15, 0, 0));
            Assert.That(uv.U, Is.EqualTo(0).Within(1E-10));
            Assert.That(uv.V, Is.EqualTo(0).Within(1E-10));
        }
        {
            Uv uv = torus.GetParametersAtPoint(new Xyz(12, 0, 0));
            Assert.That(uv.U, Is.EqualTo(0).Within(1E-10));
            Assert.That(uv.V, Is.EqualTo(PI).Within(1E-10));
        }
    }
}