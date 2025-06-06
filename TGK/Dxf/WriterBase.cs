using System.Globalization;

namespace TGK.Dxf;

public abstract class WriterBase
{
    protected readonly TextWriter _textWriter;

    protected WriterBase(TextWriter textWriter)
    {
        ArgumentNullException.ThrowIfNull(textWriter);
        if (!Equals(textWriter.FormatProvider, CultureInfo.InvariantCulture))
            throw new ArgumentException("The text writer must use the invariant culture.", nameof(textWriter));

        _textWriter = textWriter;
    }

    public abstract void Write();
}