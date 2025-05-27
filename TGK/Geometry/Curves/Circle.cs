using static System.Math;

namespace TGK.Geometry.Curves;

public sealed class Circle : Curve
{
    public Xyz Normal { get; }

    public double Radius { get; set; }

    public Xyz Center { get; set; }

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
        if (chordHeight <= 0) throw new ArgumentOutOfRangeException(nameof(chordHeight));

        double step = CircularEntityUtils.GetParametricStep(chordHeight, Radius, out int numberOfSegments);
        var points = new Xyz[numberOfSegments];

        for (int i = 0; i < numberOfSegments; i++) points[i] = GetPointAtParameter(i * step);

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
}