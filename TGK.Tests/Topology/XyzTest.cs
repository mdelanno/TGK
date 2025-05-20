using NUnit.Framework;
using TGK.Geometry;

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
}