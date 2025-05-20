using SharpDX;
using TGK.Geometry;
using TGK.Topology;

namespace TGK.Viewer;

static class XyzExtensions
{
    public static Vector3 ToVector3(this Xyz xyz)
    {
        return new Vector3((float)xyz.X, (float)xyz.Y, (float)xyz.Z);
    }
}