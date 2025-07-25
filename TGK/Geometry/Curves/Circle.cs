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

    public Circle(Xyz center, Xyz normal, double radius)
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
        return new Circle(Center, Normal, Radius);
    }

    public override Xyz[] GetStrokePoints(double chordHeight)
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

        double step = CircularEntityUtils.GetParametricStep(chordHeight, Radius);
        var points = new List<Xyz>();
        double intervalLength = Abs(endParameter - startParameter);
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

    /// <summary>
    /// Calculates the parameter value corresponding to a given point on the circle.
    /// The parameter is an angle in radians measured counterclockwise from the circle's reference vector
    /// to the vector pointing from the circle's center to the given point. The angle is normalized
    /// to the range [0, 2π).
    /// </summary>
    /// <param name="point">The point on the circle for which to calculate the parameter. Must lie exactly on the circle.</param>
    /// <returns>The parameter value in radians corresponding to the given point.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified point does not lie on the circle or on the circle plane.</exception>
    public override double GetParameterAtPoint(Xyz point)
    {
        // Project point onto the circle plane
        Xyz vectorToPoint = Center.GetVectorTo(point);
        double distanceToPlane = vectorToPoint.DotProduct(Normal);
        
        // Check if point is on the circle plane
        if (!distanceToPlane.IsAlmostEqualTo(0))
            throw new ArgumentException("Point is not on the circle plane", nameof(point));
            
        Xyz projectedVector = vectorToPoint - Normal * distanceToPlane;
    
        // Check if the projected point lies on the circle
        if (!projectedVector.Length.IsAlmostEqualTo(Radius))
            throw new ArgumentException("Point is not on the circle", nameof(point));

        // Normalize the projected vector
        projectedVector = projectedVector.ToUnit();
    
        // Calculate angle relative to reference vector
        double angle = ReferenceVector.GetAngleTo(projectedVector, Normal);
    
        // Ensure angle is in the range [0, 2π)
        if (angle < 0) angle += Tau;
    
        return angle;
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