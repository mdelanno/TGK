using TGK.Topology;

namespace TGK.Viewer.Services;

public sealed class SelectionService : ISelectionService
{
    public event Action<IEnumerable<BRepEntity>>? SelectionChanged;
    
    public IEnumerable<BRepEntity> SelectedEntities { get; private set; } = [];

    public void NotifySelectionChanged(IEnumerable<BRepEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        
        SelectedEntities = entities.ToList();
        SelectionChanged?.Invoke(SelectedEntities);
    }
}