using System.Diagnostics;
using TGK.Geometry.Surfaces;
using static System.Math;

namespace TGK.Geometry.Curves;

public sealed class Circle : Curve, IClosedCurve
{
    public Xyz Normal { get; }

    public double Radius { get; set; }

    public Xyz Center { get; set; }

    public Xyz ReferenceVector => Normal.GetPerpendicularDirection();

    public Circle(Xyz center, double radius, Xyz normal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        Center = center;
        Radius = radius;
        Normal = normal;
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Center += translateVector;
    }

    public override Circle Clone()
    {
        return new Circle(Center, Radius, Normal);
    }

    public IList<Xyz> GetStrokePoints(double chordHeight)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);

        double step = CircularEntityUtils.GetParametricStep(chordHeight, Radius, out int numberOfSegments);
        var points = new Xyz[numberOfSegments];

        for (int i = 0; i < numberOfSegments; i++) points[i] = GetPointAtParameter(i * step);

        return points;
    }

    public IList<Xyz> GetStrokePoints(double chordHeight, double startParameter, double endParameter)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chordHeight);
        if (startParameter < 0 || endParameter >= Tau || startParameter >= endParameter)
            throw new ArgumentOutOfRangeException(nameof(startParameter), "Parameters should be in the range [0, 2π) and start should be less than end.");

        double step = CircularEntityUtils.GetParametricStep(chordHeight, Radius);
        var points = new List<Xyz>();
        double intervalLength = endParameter - startParameter;
        int numberOfSegments = (int)Ceiling(intervalLength / step);
        double adjustedStep = intervalLength / numberOfSegments;
        for (int i = 0; i < numberOfSegments; i++) points.Add(GetPointAtParameter(i * adjustedStep + startParameter));
        points.Add(GetPointAtParameter(endParameter));
        Debug.Assert(points.Count >= 2, "At least two points should be returned for a valid circle segment.");
        return points;
    }

    public override Xyz GetPointAtParameter(double parameter)
    {
        Xyz u = Normal.GetPerpendicularDirection();
        Xyz v = Normal.CrossProduct(u);
        return Center + u * Radius * Cos(parameter) + v * Radius * Sin(parameter);
    }

    public override double GetParameterAtPoint(Xyz point)
    {
        throw new NotImplementedException();
    }

    public override double GetDistanceTo(Xyz point)
    {
        return point.GetDistanceTo(Center) - Radius;
    }

    public Plane GetPlane()
    {
        return new Plane(Normal, Center);
    }

    public double CalculateArea() => PI * Radius * Radius;

    public PointContainment Contains(Xyz point, double tolerance = 1e-10)
    {
        double distance = Center.GetDistanceTo(point);
        if (Abs(distance - Radius) < tolerance) return PointContainment.OnBoundary;
        return distance < Radius ? PointContainment.Inside : PointContainment.Outside;
    }
}