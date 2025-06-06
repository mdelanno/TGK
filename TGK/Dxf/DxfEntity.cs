namespace TGK.Dxf;

public abstract class DxfEntity
{
    public const short BY_LAYER = 256;

    public string LayerName { get; set; } = "0";

    public string Name { get; }

    public short ColorIndex { get; set; } = BY_LAYER;

    public List<(int, string)> ExtendedData { get; } = [];

    protected DxfEntity(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        Name = name;
    }

    public void WriteCommonGroupCodes(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(0, Name);
        dxfWriter.WritePair(8, LayerName);
        dxfWriter.WritePair(62, ColorIndex);
    }

    public void WriteExtendedData(DxfWriter dxfWriter)
    {
        foreach ((int groupCode, string value) in ExtendedData) dxfWriter.WritePair(groupCode, value);
    }

    public abstract void Write(DxfWriter dxfWriter);
}