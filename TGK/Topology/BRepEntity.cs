namespace TGK.Geometry;

public abstract class BRepEntity
{
    public int Id { get; }

    protected BRepEntity(int id)
    {
        Id = id;
    }
}