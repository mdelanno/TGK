using TGK.Geometry;
using static TGK.Dxf.HorizontalJustification;
using static TGK.Dxf.VerticalJustification;

namespace TGK.Dxf;

sealed class DbText : DxfEntity
{
    public string Text { get; }

    public double Height { get; init; } = 1.0;

    /// <summary>
    /// First alignment point (in OCS). Ignored if group 72 and/or 73 are nonzero.
    /// </summary>
    public Xyz FirstAlignmentPoint { get; init; } = Xyz.Zero;

    /// <summary>
    /// Second alignment point (in OCS). Optional, must be specified if the text is not left-aligned or baseline-aligned.
    /// </summary>
    public Xyz SecondAlignmentPoint { get; init; } = Xyz.Zero;

    public Xyz NormalVector { get; init; } = Xyz.ZAxis;

    public HorizontalJustification HorizontalJustification { get; init; } = Left;

    public VerticalJustification VerticalJustification { get; init; } = BaseLine;

    public DbText(string text) : base("TEXT")
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(text));

        Text = text;
    }

    public override void Write(DxfWriter dxfWriter)
    {
        dxfWriter.WritePair(10, FirstAlignmentPoint.X);
        dxfWriter.WritePair(20, FirstAlignmentPoint.Y);
        dxfWriter.WritePair(30, FirstAlignmentPoint.Z);
        if (HorizontalJustification != Left || VerticalJustification != BaseLine)
        {
            dxfWriter.WritePair(11, SecondAlignmentPoint.X);
            dxfWriter.WritePair(21, SecondAlignmentPoint.Y);
            dxfWriter.WritePair(31, SecondAlignmentPoint.Z);
        }
        dxfWriter.WritePair(40, Height);
        dxfWriter.WritePair(1, Text);
        if (HorizontalJustification != Left)
            dxfWriter.WritePair(72, (int)HorizontalJustification);
        if (VerticalJustification != BaseLine)
            dxfWriter.WritePair(73, (int)VerticalJustification);
        dxfWriter.WritePair(210, NormalVector.X);
        dxfWriter.WritePair(220, NormalVector.Y);
        dxfWriter.WritePair(230, NormalVector.Z);
    }
}