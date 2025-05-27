using static System.Math;

namespace TGK.Geometry;

static class CircularEntityUtils
{
    /// <summary>
    /// Returns a step in radians.
    /// </summary>
    /// <param name="chordHeight"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static double GetParametricStep(double chordHeight, double radius)
    {
        return GetParametricStep(chordHeight, radius, out int _);
    }

    /// <summary>
    /// Returns a step in radians and the number of segments needed to cover the entire circle.
    /// </summary>
    /// <param name="chordHeight"></param>
    /// <param name="radius"></param>
    /// <param name="numberOfSegments"></param>
    /// <returns></returns>
    public static double GetParametricStep(double chordHeight, double radius, out int numberOfSegments)
    {
        double sectorMaxAngle = Acos((radius - chordHeight) / radius) * 2.0;
        numberOfSegments = Max((int)Ceiling(2 * PI / sectorMaxAngle), 6);
        return 2 * PI / numberOfSegments;
    }
}