using System.Globalization;
using TGK.Geometry;
using static System.Globalization.NumberStyles;
using static System.StringComparison;

namespace TGK.Dxf;

static class DxfReader
{
    const string SECTION_ENTITIES = "ENTITIES";

    public async static Task<List<DxfEntity>> GetPolylineAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var entities = new List<DxfEntity>();

        using (var reader = new StreamReader(stream))
        {
            DxfEntity? dxfEntity = null;
            IPolylineVertex2d? vertex = null;
            bool sectionIsEntities = false;
            while (true)
            {
                string? dxfCodeLine = await reader.ReadLineAsync();
                if (dxfCodeLine == null) break;
                string? valueLine = await reader.ReadLineAsync();
                if (valueLine == null) break;
                int dxfCode = ParseInt(dxfCodeLine);

                switch (dxfCode)
                {
                    case 0 when sectionIsEntities:
                        {
                            string value = valueLine.Trim();
                            if (value.Equals("POLYLINE", OrdinalIgnoreCase))
                            {
                                dxfEntity = new Polyline2d();
                                entities.Add(dxfEntity);
                            }
                            else if (value.Equals("LWPOLYLINE", OrdinalIgnoreCase))
                            {
                                dxfEntity = new LightWeightPolyline();
                                entities.Add(dxfEntity);
                                vertex = new LightWeightPolylineVertex();
                            }
                            else if (value.Equals("LINE", OrdinalIgnoreCase))
                            {
                                dxfEntity = new Line();
                                vertex = null;
                                entities.Add(dxfEntity);
                            }
                            else if (value.Equals("VERTEX", OrdinalIgnoreCase))
                            {
                                if (dxfEntity == null) throw new NullReferenceException($"{nameof(dxfEntity)} is null.");
                                if (vertex != null) ((IPolyline2d)dxfEntity).Vertices.Add(vertex);
                                vertex = dxfEntity is Polyline2d ? new PolylineVertex2d() : new LightWeightPolylineVertex();
                            }
                            else if (value.Equals("SEQEND", OrdinalIgnoreCase))
                            {
                                if (dxfEntity == null) throw new NullReferenceException($"{nameof(dxfEntity)} is null.");
                                if (vertex != null) ((IPolyline2d)dxfEntity).Vertices.Add(vertex);
                            }
                            else if (dxfEntity is IPolyline2d polyline)
                            {
                                if (vertex != null) polyline.Vertices.Add(vertex);
                            }
                            else
                            {
                                dxfEntity = null;
                            }
                            break;
                        }

                    case 2:
                        {
                            string section = valueLine.Trim();
                            sectionIsEntities = section == SECTION_ENTITIES;
                            break;
                        }

                    case 10 when sectionIsEntities && dxfEntity is LightWeightPolyline polyline:
                        {
                            vertex = new LightWeightPolylineVertex();
                            if (vertex == null) throw new NullReferenceException($"{nameof(vertex)} is null.");
                            polyline.Vertices.Add(vertex);
                            double x = ParseDouble(valueLine);
                            vertex.Position = new Uv(x, vertex.Position.V);
                            break;
                        }

                    case 10 when sectionIsEntities && dxfEntity is Line line:
                        {
                            // Start point X
                            double x = ParseDouble(valueLine);
                            line.StartPoint = new Uv(x, line.StartPoint.V);
                            break;
                        }

                    case 11 when sectionIsEntities && dxfEntity is Line line:
                        {
                            // End point X
                            double x = ParseDouble(valueLine);
                            line.EndPoint = new Uv(x, line.EndPoint.V);
                            break;
                        }

                    case 20 when sectionIsEntities && dxfEntity is IPolyline2d && vertex != null:
                        {
                            double y = ParseDouble(valueLine);
                            vertex.Position = new Uv(vertex.Position.U, y);
                            vertex = null;
                            break;
                        }

                    case 20 when sectionIsEntities && dxfEntity is Line line:
                        {
                            // Start point Y
                            double y = ParseDouble(valueLine);
                            line.StartPoint = new Uv(line.StartPoint.U, y);
                            break;
                        }

                    case 21 when sectionIsEntities && dxfEntity is Line line:
                        {
                            // End point Y
                            double y = ParseDouble(valueLine);
                            line.EndPoint = new Uv(line.EndPoint.U, y);
                            break;
                        }

                    case 42 when sectionIsEntities && dxfEntity is IPolyline2d && vertex != null:
                        {
                            vertex.Bulge = ParseDouble(valueLine);
                            break;
                        }

                    case 70 when sectionIsEntities && dxfEntity is IPolyline2d polyline2d:
                        {
                            var flags = (PolylineFlag)ParseInt(valueLine);
                            polyline2d.IsClosed = flags.HasFlag(PolylineFlag.Closed);
                            break;
                        }

                    case 1000 when sectionIsEntities && dxfEntity != null:
                    case 1001 when sectionIsEntities && dxfEntity != null:
                        {
                            dxfEntity.ExtendedData.Add((dxfCode, valueLine));
                            break;
                        }
                }
            }
        }

        return entities;
    }

    static double ParseDouble(string line)
    {
        return double.Parse(line, AllowLeadingWhite | AllowTrailingWhite | Float, CultureInfo.InvariantCulture);
    }

    static int ParseInt(string line)
    {
        return int.Parse(line, AllowLeadingWhite | AllowTrailingWhite | Integer, null);
    }
}