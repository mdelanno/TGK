namespace TGK.Geometry;

public sealed class CoordinateSystem
{
    public Xyz Origin { get; }

    public Xyz XAxis { get; }

    public Xyz YAxis { get; }

    public CoordinateSystem(Xyz origin, Xyz xAxis, Xyz yAxis)
    {
        if (!xAxis.IsUnitLength())
            throw new ArgumentException("XAxis must be a unit vector.", nameof(xAxis));
        if (!yAxis.IsUnitLength())
            throw new ArgumentException("YAxis must be a unit vector.", nameof(yAxis));
        if (!xAxis.IsPerpendicularTo(yAxis))
            throw new ArgumentException("XAxis and YAxis must be perpendicular.", nameof(yAxis));

        Origin = origin;
        XAxis = xAxis;
        YAxis = yAxis;
    }

    public Xyz GetZAxis() => XAxis.CrossProduct(YAxis);

    public Uv Convert2d(in Xyz point)
    {
        Xyz v = Origin.GetVectorTo(point);
        return new Uv(v.DotProduct(XAxis), v.DotProduct(YAxis));
    }
}