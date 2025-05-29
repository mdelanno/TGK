using TGK.Geometry;
using static System.Math;

namespace TGK.FaceterServices;

static class TriangleUtils
{
    /// <summary>
    /// Returns true if the point is inside the triangle. If the point is on the edge of the triangle or outside, returns false.
    ///
    /// The triangle's vertices don't need to be sorted in any particular order.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="vertex0"></param>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static bool IsInside(Uv point, Uv vertex0, Uv vertex1, Uv vertex2, double tolerance = 1e-10)
    {
        if (point.IsAlmostEqualTo(vertex0, tolerance)) return false;
        if (point.IsAlmostEqualTo(vertex1, tolerance)) return false;
        if (point.IsAlmostEqualTo(vertex2, tolerance)) return false;

        // For each edge, we check whether the opposite vertex is on the same side as the point.
        return SameSide(vertex0, vertex1, vertex2, point, tolerance) && SameSide(vertex1, vertex2, vertex0, point, tolerance) && SameSide(vertex2, vertex0, vertex1, point, tolerance);
    }

    /// <summary>
    /// Returns true if the point and the opposite vertex is on the same side of the edge.
    /// </summary>
    /// <param name="edgeStartVertex"></param>
    /// <param name="edgeEndVertex"></param>
    /// <param name="oppositeVertex"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    static bool SameSide(Uv edgeStartVertex, Uv edgeEndVertex, Uv oppositeVertex, Uv point, double tolerance)
    {
        if (tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));

        Uv side = edgeEndVertex - edgeStartVertex;
        Uv v = point - edgeStartVertex;
        Uv otherSide = oppositeVertex - edgeEndVertex;

        double pointDeterminant = side.U * v.V - side.V * v.U;
        if (Abs(pointDeterminant) < tolerance) return false;

        double oppositeVertexDeterminant = side.U * otherSide.V - side.U * otherSide.V;

        return Sign(pointDeterminant) == Sign(oppositeVertexDeterminant);
    }
}