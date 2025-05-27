namespace TGK.Geometry.Curves;

public sealed class Line : Curve
{
    public Xyz Origin { get; set; }

    public Xyz Direction { get; set; }

    public Line(in Xyz origin, in Xyz direction)
    {
        if (!direction.IsUnitLength())
            throw new ArgumentException("Direction must not be a unit vector.", nameof(direction));

        Origin = origin;
        Direction = direction;
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        throw new NotImplementedException();
    }

    public override Line Clone()
    {
        return new Line(Origin, Direction);
    }

    public override Xyz GetPointAtParameter(double parameter)
    {
        return Origin + Direction * parameter;
    }

    public override double GetParameterAtPoint(Xyz point)
    {
        throw new NotImplementedException();
    }

    public bool IsParallelTo(Line other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Direction.IsParallelTo(other.Direction);
    }
}