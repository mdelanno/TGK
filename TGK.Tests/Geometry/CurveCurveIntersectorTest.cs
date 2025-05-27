using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TGK.Geometry;
using TGK.Geometry.Curves;

namespace TGK.Tests.Geometry;

[TestFixture]
[TestOf(typeof(CurveCurveIntersector))]
public class CurveCurveIntersectorTest
{
    [Test]
    public void TestLineLineNoIntersection()
    {
        var line0 = new Line(Xyz.Origin, Xyz.XAxis);
        var line1 = new Line(new Xyz(0, 1, 0), Xyz.XAxis);
        var intersector = new CurveCurveIntersector(line0, line1);
        IEnumerable<CurveCurveIntersectionResult> results = intersector.GetIntersections();
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(0));
    }

    [Test]
    public void TestLineLinePoint()
    {
        var line0 = new Line(Xyz.Origin, Xyz.XAxis);
        var line1 = new Line(new Xyz(0, 1, 0), new Xyz(1, 1, 0).GetNormal());
        var intersector = new CurveCurveIntersector(line0, line1);
        IEnumerable<CurveCurveIntersectionResult> results = intersector.GetIntersections();
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.First(), Is.InstanceOf<PointCurveCurveIntersectionResult>());
        var result = (PointCurveCurveIntersectionResult)results.First();
        Assert.That(result.Curve0, Is.EqualTo(line0));
        Assert.That(result.Curve1, Is.EqualTo(line1));
        Assert.That(result.Point, Is.EqualTo(new Xyz(-1, 0, 0)));
    }

    [Test]
    public void TestLineLineOverlap()
    {
        var line0 = new Line(Xyz.Origin, new Xyz(1, 0, 0));
        var line1 = new Line(new Xyz(1, 0, 0), new Xyz(1, 0, 0));
        var intersector = new CurveCurveIntersector(line0, line1);
        IEnumerable<CurveCurveIntersectionResult> results = intersector.GetIntersections();
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count(), Is.EqualTo(1));
        Assert.That(results.First(), Is.InstanceOf<OverlapCurveCurveIntersectionResult>());
        var result = (OverlapCurveCurveIntersectionResult)results.First();
        Assert.That(result.Curve0, Is.EqualTo(line0));
        Assert.That(result.Curve1, Is.EqualTo(line1));
        Assert.That(result.IntervalOnCurve0.IsUnbounded);
    }
}