using TGK.Geometry;
using TGK.Topology;

namespace TGK.CenterOfInscribedCircleServices;

public sealed class Cell
{
    static readonly double _squareRootOfTwo = Math.Sqrt(2);
    
    public Uv Center { get; }

    /// <summary>
    /// Moitié de la taille de la cellule.
    /// </summary>
    public double HalfSize { get; }

    public double Max { get; }

    /// <summary>
    /// Distance du centre de la cellule à la région.
    /// </summary>
    public double Distance { get; }

    public Cell(double x, double y, double halfSize, Face face)
    {
        Center = new Uv(x, y);
        HalfSize = halfSize;
        Distance = PointToPolygonDistance(x, y, face);
        Max = Distance + HalfSize * _squareRootOfTwo;
    }

    static double PointToPolygonDistance(double x, double y, Face face)
    {
        var point = new Xyz(x, y, z: 0);
        double distance = face.GetDistanceToBoundary(point);
        return (face.Contains(point) == PointContainment.Outside ? -1 : 1) * distance;
    }
}