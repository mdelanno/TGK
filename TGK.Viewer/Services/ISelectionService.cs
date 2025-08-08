using TGK.Topology;

namespace TGK.Viewer.Services;

public interface ISelectionService
{
    event Action<IEnumerable<BRepEntity>> SelectionChanged;
    
    void NotifySelectionChanged(IEnumerable<BRepEntity> entities);
    
    IEnumerable<BRepEntity> SelectedEntities { get; }
}