using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

public sealed class Torus : Surface
{
    public Xyz Center { get; private set; }

    public Xyz AxisDirection { get; }

    public double MajorRadius { get; }

    public double MinorRadius { get; }

    public Torus(in Xyz center, double majorRadius, double minorRadius, in Xyz axisDirection)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(majorRadius);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minorRadius);
        if (axisDirection.IsZero())
            throw new ArgumentException("Axis direction vector cannot be zero.", nameof(axisDirection));

        Center = center;
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
        AxisDirection = axisDirection.ToUnit();
    }

    public Torus(in Xyz center, double majorRadius, double minorRadius) : this(center, majorRadius, minorRadius, Xyz.ZAxis)
    {
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Center += translateVector;
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1E-10)
    {
        throw new NotImplementedException();
    }

    public override Xyz GetNormal(in Xyz point)
    {
        throw new NotImplementedException();
    }

    public override bool PassesThrough(in Xyz point)
    {
        throw new NotImplementedException();
    }

    public override Uv GetParametersAtPoint(in Xyz point)
    {
        // 1. Translater le point pour que le tore soit centré à l'origine.
        Xyz relativePoint = point - Center;

        // 2. Construire une base orthonormée où l'axe du tore est Oz'
        // On choisit un vecteur perpendiculaire à _axisDirection pour x', puis y' = z' ^ x'
        Xyz zAxis = AxisDirection;
        Xyz temp = Abs(zAxis.X) > 0.9 ? Xyz.YAxis : Xyz.XAxis;
        Xyz xAxis = (temp - zAxis * temp.DotProduct(zAxis)).ToUnit();
        Xyz yAxis = zAxis.CrossProduct(xAxis);

        // 3. Changer de repère : exprimer le point dans la base locale du tore
        double x = relativePoint.DotProduct(xAxis);
        double y = relativePoint.DotProduct(yAxis);
        double z = relativePoint.DotProduct(zAxis);

        // 4. Calculer l'angle toroïdal 'u' (autour de l'axe du tore)
        double u = Atan2(y, x);

        // 5. Calculer l'angle poloïdal 'v' (autour du tube)
        double projectedRadius = Sqrt(x * x + y * y);
        double v = Atan2(z, projectedRadius - MajorRadius);

        // 6. Normaliser les angles dans [0, 2π]
        if (u < 0) u += 2 * PI;
        if (v < 0) v += 2 * PI;

        return new Uv(u, v);
    }

    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        throw new NotImplementedException();
    }

    public override Interval2d GetDomain()
    {
        return new Interval2d(0, double.Tau, 0, double.Tau);
    }
}