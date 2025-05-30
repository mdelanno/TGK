using TGK.Geometry.Surfaces;

namespace TGK.Geometry;

public sealed class CoordinateSystem : Entity3d
{
    public Xyz Origin { get; private set; }

    public Xyz XAxis { get; }

    public Xyz YAxis { get; }

    public Xyz ZAxis { get; }

    public CoordinateSystem(Xyz origin, Xyz xAxis, Xyz yAxis) : this(origin, xAxis, yAxis, xAxis.CrossProduct(yAxis))
    {
    }

    public CoordinateSystem(Xyz origin, Xyz xAxis, Xyz yAxis, Xyz zAxis)
    {
        if (!xAxis.IsUnitLength())
            throw new ArgumentException("XAxis must be a unit vector.", nameof(xAxis));
        if (!yAxis.IsUnitLength())
            throw new ArgumentException("YAxis must be a unit vector.", nameof(yAxis));
        if (!zAxis.IsUnitLength())
            throw new ArgumentException("ZAxis must be a unit vector.", nameof(zAxis));
        if (!xAxis.IsPerpendicularTo(yAxis))
            throw new ArgumentException("XAxis and YAxis must be perpendicular.", nameof(yAxis));
        if (!xAxis.IsPerpendicularTo(zAxis))
            throw new ArgumentException("XAxis and ZAxis must be perpendicular.", nameof(zAxis));
        if (!yAxis.IsPerpendicularTo(zAxis))
            throw new ArgumentException("YAxis and ZAxis must be perpendicular.", nameof(zAxis));

        Origin = origin;
        XAxis = xAxis;
        YAxis = yAxis;
        ZAxis = zAxis;
    }

    public Uv Convert2d(in Xyz point)
    {
        Xyz v = Origin.GetVectorTo(point);
        return new Uv(v.DotProduct(XAxis), v.DotProduct(YAxis));
    }

    public override void TranslateBy(in Xyz translateVector)
    {
        Origin += translateVector;
    }
}