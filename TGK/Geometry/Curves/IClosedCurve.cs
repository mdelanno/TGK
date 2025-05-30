namespace TGK.Geometry.Curves;

public interface IClosedCurve
{
    double CalculateArea();

    PointContainment Contains(Xyz point, double tolerance = 1e-10);
}