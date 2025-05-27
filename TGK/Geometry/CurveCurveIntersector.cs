using TGK.Geometry.Curves;

namespace TGK.Geometry;

public class CurveCurveIntersector
{
    public Curve Curve0 { get; }

    public Curve Curve1 { get; }

    public IEnumerable<IntersectionResult> GetIntersections()
    {
        if (Curve0 is Line line0 && Curve1 is Line line1)
            return IntersectionLineLine(line0, line1);
        throw new NotImplementedException();
    }

    static IEnumerable<IntersectionResult> IntersectionLineLine(Line line0, Line line1)
    {
        ArgumentNullException.ThrowIfNull(line0);
        ArgumentNullException.ThrowIfNull(line1);

        Xyz directionCross = line0.Direction.CrossProduct(line1.Direction);
        if (directionCross.IsZero())
        {
            // TODO Check if lines are collinear
            throw new NotImplementedException();
        }

        double parameter = (line1.Origin - line0.Origin).CrossProduct(line1.Direction).DotProduct(directionCross) / directionCross.LengthSquared;
        Xyz intersectionPoint = line0.GetPointAtParameter(parameter);
        yield return new PointIntersectionResult(line0, line1, intersectionPoint);
    }

    public CurveCurveIntersector(Curve curve0, Curve curve1)
    {
        ArgumentNullException.ThrowIfNull(curve0);
        ArgumentNullException.ThrowIfNull(curve1);

        Curve0 = curve0;
        Curve1 = curve1;
    }
}