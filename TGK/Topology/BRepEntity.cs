namespace TGK.Topology;

public abstract class BRepEntity
{
    public int Id { get; }

    public object? Tag { get; set; }

    protected BRepEntity(int id)
    {
        Id = id;
    }
}