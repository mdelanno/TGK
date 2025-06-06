namespace TGK.Dxf;

[Flags]
public enum PolylineFlag : byte
{
    None = 0,

    Closed = 1,

    Plinegen = 128
}