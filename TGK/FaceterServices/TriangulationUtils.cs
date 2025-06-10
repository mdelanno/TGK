using TGK.Geometry;

namespace TGK.FaceterServices;

static class TriangulationUtils
{
    public static int[] EarClipping(IPolygonForTriangulation polygon, double tolerance = 1e-10)
    {
        int numberOfVertices = polygon.VerticesCount;
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (numberOfVertices < 3) throw new ArgumentException("Polygon must have at least 3 vertices.", nameof(polygon));

        List<int> indices = polygon.GetIndices();

        switch (numberOfVertices)
        {
            case 3:
                return [indices[0], indices[1], indices[2]];

            case 4:
                return [indices[0], indices[1], indices[2], indices[2], indices[3], indices[0]];
        }

        int numberOfTriangles = numberOfVertices - 2;
        int[] triangleIndices = new int[numberOfTriangles * 3];
        int triangleIndex = 0;

        DirectionOfRotation direction = DirectionOfRotation.Unknown;
        while (true)
        {
            int start;
            int end;
            int increment;
            switch (direction)
            {
                case DirectionOfRotation.Unknown:
                case DirectionOfRotation.Clockwise:
                    direction = DirectionOfRotation.CounterClockwise;
                    start = 0;
                    end = indices.Count - 1;
                    increment = 1;
                    break;

                case DirectionOfRotation.CounterClockwise:
                    direction = DirectionOfRotation.Clockwise;
                    start = indices.Count - 1;
                    end = 0;
                    increment = -1;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            for (int i = start; i != end; i += increment)
            {
                int previousVertexIndex = indices[(i + indices.Count - 1) % indices.Count];
                Uv previous = polygon.GetPosition(previousVertexIndex);

                int currentVertexIndex = indices[i];
                Uv vertex = polygon.GetPosition(currentVertexIndex);

                int nextVertexIndex = indices[(i + 1) % indices.Count];
                Uv next = polygon.GetPosition(nextVertexIndex);

                bool? vertexIsConvex = vertex.TurnLeft(previous, next, tolerance);
                switch (vertexIsConvex)
                {
                    case true:
                        {
                            // The vertex is convex. We will remove it if it is an ear.

                            // Test if there is no other vertex in the triangle.
                            foreach (int vertexIndex in indices)
                            {
                                if (vertexIndex == previousVertexIndex || vertexIndex == currentVertexIndex || vertexIndex == nextVertexIndex) continue;
                                Uv other = polygon.GetPosition(vertexIndex);
                                if (TriangleUtils.IsInside(other, previous, vertex, next, tolerance)) goto nextVertex;
                            }

                            indices.Remove(currentVertexIndex);

                            triangleIndices[triangleIndex++] = previousVertexIndex;
                            triangleIndices[triangleIndex++] = currentVertexIndex;
                            triangleIndices[triangleIndex++] = nextVertexIndex;

                            goto restart;
                        }

                    case null:
                        {
                            // The vertex is on a straight line. We don't remove it because we want to keep all the vertices in the polygon.
                            // We try on the next vertex.
                            break;
                        }
                }

                nextVertex: ;
            }
            throw new InvalidOperationException("Can not triangulate the polygon.");

            restart: ;
            if (indices.Count == 3)
            {
                triangleIndices[triangleIndex++] = indices[0];
                triangleIndices[triangleIndex++] = indices[1];
                triangleIndices[triangleIndex] = indices[2];
                return triangleIndices;
            }
        }
    }
}