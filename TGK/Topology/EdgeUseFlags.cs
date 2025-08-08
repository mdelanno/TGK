namespace TGK.Topology;

[Flags]
public enum EdgeUseFlags
{
    None = 0,

    /// <summary>
    /// Seam at the highest parameter value.
    /// </summary>
    HighSeam = 1,

    /// <summary>
    /// Seam at the lowest parameter value.
    /// </summary>
    LowSeam = 2,
}