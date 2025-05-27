namespace TGK.Geometry;

public enum IntersectionType
{
    Unknown,

    /// <summary>
    /// The tangent vectors of the two curves are parallels at the intersection point.
    /// </summary>
    Tangent,

    Transverse
}