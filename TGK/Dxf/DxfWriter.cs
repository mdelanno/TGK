namespace TGK.Dxf;

public sealed class DxfWriter : WriterBase
{
    public List<DxfEntity> Entities { get; } = [];

    public DxfWriter(TextWriter textWriter) : base(textWriter)
    {
    }

    public override void Write()
    {
        _textWriter.WriteLine("  0");
        _textWriter.WriteLine("SECTION");

        _textWriter.WriteLine("  2");
        _textWriter.WriteLine("ENTITIES");

        foreach (DxfEntity entity in Entities)
        {
            if (entity is LightWeightPolyline)
                throw new InvalidOperationException("Light weight polylines are not supported for writing.");
            entity.WriteCommonGroupCodes(this);
            entity.Write(this);
            entity.WriteExtendedData(this);
        }

        _textWriter.WriteLine("  0");
        _textWriter.WriteLine("ENDSEC");

        _textWriter.WriteLine("  0");
        _textWriter.WriteLine("EOF");
    }

    public void WritePair(int dxfCode, object value)
    {
        _textWriter.WriteLine($"  {dxfCode}");
        _textWriter.WriteLine(value);
    }
}