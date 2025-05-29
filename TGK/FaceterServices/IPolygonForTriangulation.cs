using TGK.Geometry;

namespace TGK.FaceterServices;

interface IPolygonForTriangulation
{
    int VerticesCount { get; }

    Uv GetPosition(int index);

    List<int> GetIndices();
}