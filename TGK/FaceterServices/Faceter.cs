using System.Diagnostics;
using TGK.Topology;

namespace TGK.FaceterServices;

public static class Faceter
{
    public static Mesh FacetSolid(Solid solid, double chordHeight)
    {
        ArgumentNullException.ThrowIfNull(solid);
        ArgumentOutOfRangeException.ThrowIfNegative(chordHeight);

        var mesh = new Mesh();
        var adapter = new NodeListAdapter();
        foreach (Face face in solid.Faces)
        {
            List<Node> nodes = face.ProjectBoundaryToParameterSpace(mesh, chordHeight);
            adapter.Set(nodes);
            int[] triangleIndices = TriangulationUtils.EarClipping(adapter);
            mesh.TriangleIndices.Add(face, triangleIndices);
        }

        Debug.Assert(mesh.Positions.Count == mesh.Normals.Count, "Mesh positions and normals count should match.");
        return mesh;
    }
}