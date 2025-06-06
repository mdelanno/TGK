using TGK.Geometry;
using static TGK.Dxf.HorizontalJustification;
using static TGK.Dxf.VerticalJustification;

namespace TGK.Dxf;

sealed class DbText : DxfEntity
{
    public string Text { get; }

    public double Height { get; init; } = 1.0;

    public Uv FirstAlignmentPoint { get; init; } = Uv.Zero;

    public Uv SecondAlignmentPoint { get; init; } = Uv.Zero;

    public HorizontalJustification HorizontalJustification { get; init; } = Left;

    public VerticalJustification VerticalJustification { get; init; } = BaseLine;

    public DbText(string text) : base("TEXT")
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(text));

        Text = text;
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(10, FirstAlignmentPoint.U);
        dxfWriter.WritePair(20, FirstAlignmentPoint.V);
        dxfWriter.WritePair(30, 0.0);
        if (HorizontalJustification != Left || VerticalJustification != BaseLine)
        {
            dxfWriter.WritePair(11, SecondAlignmentPoint.U);
            dxfWriter.WritePair(21, SecondAlignmentPoint.V);
            dxfWriter.WritePair(31, 0.0);
        }
        dxfWriter.WritePair(40, Height);
        dxfWriter.WritePair(1, Text);
        if (HorizontalJustification != Left)
            dxfWriter.WritePair(72, (int)HorizontalJustification);
        if (VerticalJustification != BaseLine)
            dxfWriter.WritePair(73, (int)VerticalJustification);
    }
}