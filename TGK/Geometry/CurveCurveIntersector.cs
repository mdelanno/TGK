using TGK.Geometry.Curves;

namespace TGK.Geometry;

public sealed class CurveCurveIntersector
{
    public Curve Curve0 { get; }

    public Curve Curve1 { get; }

    public CurveCurveIntersector(Curve curve0, Curve curve1)
    {
        ArgumentNullException.ThrowIfNull(curve0);
        ArgumentNullException.ThrowIfNull(curve1);

        Curve0 = curve0;
        Curve1 = curve1;
    }

    public IEnumerable<CurveCurveIntersectionResult> GetIntersections()
    {
        {
            if (Curve0 is Line line0 && Curve1 is Line line1)
                return IntersectionLineLine(line0, line1);
        }
        {
            if (Curve0 is Line line && Curve1 is Circle circle)
                return IntersectionLineCircle(line, circle);
        }
        {
            if (Curve0 is Circle circle && Curve1 is Line line)
                return IntersectionLineCircle(line, circle);
        }
        throw new NotImplementedException();
    }

    static IEnumerable<CurveCurveIntersectionResult> IntersectionLineLine(Line line0, Line line1)
    {
        ArgumentNullException.ThrowIfNull(line0);
        ArgumentNullException.ThrowIfNull(line1);

        Xyz directionCross = line0.Direction.CrossProduct(line1.Direction);
        if (directionCross.IsZero())
        {
            if (line0.PassesThrough(line1.Origin))
                yield return new OverlapCurveCurveIntersectionResult(line0, line1, Interval1d.Unbounded);
            yield break;
        }

        double parameter = (line1.Origin - line0.Origin).CrossProduct(line1.Direction).DotProduct(directionCross) / directionCross.LengthSquared;
        Xyz intersectionPoint = line0.GetPointAtParameter(parameter);
        yield return new PointCurveCurveIntersectionResult(line0, line1, intersectionPoint);
    }

    IEnumerable<CurveCurveIntersectionResult> IntersectionLineCircle(Line line, Circle circle)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(circle);

        throw new NotImplementedException();
    }
}