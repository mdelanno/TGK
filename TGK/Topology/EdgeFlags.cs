namespace TGK.Topology;

[Flags]
public enum EdgeFlags : byte
{
    None = 0,

    Seam = 1,

    /// <summary>
    /// Degenerate edge which is a single point. Used for poles in spherical faces.
    /// </summary>
    Pole = 2
}