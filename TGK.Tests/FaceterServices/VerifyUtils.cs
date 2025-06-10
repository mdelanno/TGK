using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TGK.Dxf;
using TGK.FaceterServices;
using VerifyNUnit;

namespace TGK.Tests.FaceterServices;

static class VerifyUtils
{
    public static Task VerifyTriangleIndices(NodeListAdapter adapter, int[] triangleIndices)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        ArgumentNullException.ThrowIfNull(triangleIndices);
        if (triangleIndices.Length % 3 != 0)
            throw new ArgumentException("Triangle indices must be a multiple of 3.", nameof(triangleIndices));
        
        var writer = new StringWriter(CultureInfo.InvariantCulture);
        var dxfWriter = new DxfWriter(writer);
        for (int i = 0; i < triangleIndices.Length; i += 3)
        {
            var polyline = new Polyline2d(3)
            {
                IsClosed = true
            };
            dxfWriter.Entities.Add(polyline);
            polyline.Vertices.Add(new PolylineVertex2d { Position = adapter.GetPosition(triangleIndices[i]) });
            polyline.Vertices.Add(new PolylineVertex2d { Position = adapter.GetPosition(triangleIndices[i + 1]) });
            polyline.Vertices.Add(new PolylineVertex2d { Position = adapter.GetPosition(triangleIndices[i + 2]) });
        }
        dxfWriter.Write();
        string dxf = writer.ToString();
        return Verifier.Verify(dxf, extension: "dxf");
    }
}