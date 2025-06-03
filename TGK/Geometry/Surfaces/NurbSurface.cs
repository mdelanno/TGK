using TGK.Geometry.Curves;
using static System.Math;

namespace TGK.Geometry.Surfaces;

public sealed class NurbSurface : Surface
{
    readonly int _degreeU;

    readonly int _degreeV;

    readonly Xyz[,] _controlPoints;

    readonly double[,] _weights;

    public NurbSurface(int degreeU, int degreeV, Xyz[,] controlPoints, double[,] weights)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(degreeU, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(degreeV, 1);
        ArgumentNullException.ThrowIfNull(controlPoints);
        ArgumentNullException.ThrowIfNull(weights);
        
        _degreeU = degreeU;
        _degreeV = degreeV;
        _controlPoints = controlPoints;
        _weights = weights;
    }

    public Xyz GetPointAtParameters(double u, double v)
    {
        Xyz evaluatedPoint = Xyz.Zero;
        double denominator = 0;

        for (int i = 0; i <= _degreeU; i++)
        {
            for (int j = 0; j <= _degreeV; j++)
            {
                double bi = Bernstein(i, _degreeU, u);
                double bj = Bernstein(j, _degreeV, v);

                evaluatedPoint += _controlPoints[i, j] * (bi * bj * _weights[i, j]);
                denominator += _weights[i, j] * bi * bj;
            }
        }
        evaluatedPoint /= denominator;
        return evaluatedPoint;
    }
    
    static double Bernstein(int index, int degree, double parameter)
    {
        return Binomial(degree, index) * Pow(parameter, index) * Pow(1 - parameter, degree - index);
    }

    static double Binomial(int n, int k)
    {
        double res = 1;
        for (int i = 1; i <= k; i++)
        {
            res *= (n - (k - i));
            res /= i;
        }
        return res;
    }

    public override IEnumerable<CurveSurfaceIntersectionResult> IntersectWith(Line line, double tolerance = 1e-10)
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
        throw new NotImplementedException();
    }

    internal override Uv[] ProjectCurveToParametricSpace(Curve curve, double chordHeight)
    {
        throw new NotImplementedException();
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        throw new NotImplementedException();
    }
}