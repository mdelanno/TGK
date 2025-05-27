using TGK.Geometry.Curves;

namespace TGK.Geometry;

public sealed class PointOnCurve
{
    Xyz? _point;

    double _parameter = double.NaN;

    public Curve Curve { get; }

    public Xyz GetPoint()
    {
        if (_point is not null)
            return _point.Value;
        _point = Curve.GetPointAtParameter(_parameter);
        return _point.Value;
    }

    public double GetParameter()
    {
        if (!double.IsNaN(_parameter))
            return _parameter;
        _parameter = Curve.GetParameterAtPoint(_point!.Value);
        return _parameter;
    }

    public PointOnCurve(Curve curve, Xyz point)
    {
        ArgumentNullException.ThrowIfNull(curve);

        Curve = curve;
        _point = point;
    }

    public PointOnCurve(Curve curve, double parameter)
    {
        ArgumentNullException.ThrowIfNull(curve);

        Curve = curve;
        _parameter = parameter;
    }
}