namespace TGK.Geometry.Curves;

public abstract class Curve
{
    public abstract void TranslateBy(in Xyz translateVector);

    public abstract Curve Clone();

    public abstract Xyz GetPointAtParameter(double parameter);

    public abstract double GetParameterAtPoint(Xyz point);

    public abstract double GetDistanceTo(Xyz point);

    public abstract Xyz[] GetStrokePoints(double chordHeight);
}