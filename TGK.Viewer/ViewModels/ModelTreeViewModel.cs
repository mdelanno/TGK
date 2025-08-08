using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TGK.Geometry.Curves;
using TGK.Topology;
using TGK.Viewer.Services;

namespace TGK.Viewer.ViewModels;

public sealed class ModelTreeViewModel : ObservableObject
{
    readonly ISelectionService _selectionService;

    public ObservableCollection<ModelTreeItem> Items { get; } = [];

    public ObservableCollection<ModelTreeItem> SelectedItems { get; } = [];

    public RelayCommand ClearSelectionCommand { get; }

    public ModelTreeViewModel(ISelectionService selectionService)
    {
        ArgumentNullException.ThrowIfNull(selectionService);

        _selectionService = selectionService;
        ClearSelectionCommand = new RelayCommand(ClearAllSelections);

        SelectedItems.CollectionChanged += OnSelectedItemsChanged;
    }

    public void UpdateTree(Solid? solid)
    {
        // Sauvegarder les entités sélectionnées
        var selectedEntities = SelectedItems.Select(item => item.Data).OfType<BRepEntity>().ToHashSet();

        Items.Clear();

        if (solid == null)
        {
            SelectedItems.Clear();
            return;
        }

        var newSelectedItems = new List<ModelTreeItem>();

        var rootItem = new ModelTreeItem("Model") { IsExpanded = true };
        Items.Add(rootItem);

        if (solid.Vertices.Count > 0)
        {
            var verticesItem = new ModelTreeItem($"Vertices ({solid.Vertices.Count})") { IsExpanded = true };
            rootItem.Children.Add(verticesItem);

            foreach (Vertex vertex in solid.Vertices)
            {
                var vertexItem = new ModelTreeItem($"Vertex {vertex.Id}: {vertex.Position}", vertex);
                verticesItem.Children.Add(vertexItem);

                if (selectedEntities.Contains(vertex))
                {
                    newSelectedItems.Add(vertexItem);
                    vertexItem.IsSelected = true;
                }
            }
        }

        if (solid.Edges.Count > 0)
        {
            var edgesItem = new ModelTreeItem($"Edges ({solid.Edges.Count})") { IsExpanded = true };
            rootItem.Children.Add(edgesItem);

            foreach (Edge edge in solid.Edges)
            {
                string edgeDescription;
                switch (edge.Curve)
                {
                    case null:
                        edgeDescription = edge.IsPole ? $"Pole: {edge.StartVertex.Position}" : $"Line: {edge.StartVertex.Position} → {edge.EndVertex.Position}";
                        break;

                    case Circle circle:
                        edgeDescription = $"Circle: Center={circle.Center}, Radius={circle.Radius:F2}";
                        break;

                    default:
                        edgeDescription = edge.Curve.GetType().Name;
                        break;
                }
                if (edge.IsSeam)
                    edgeDescription += " (seam)";
                var edgeItem = new ModelTreeItem($"Edge {edge.Id}: {edgeDescription}", edge);
                edgesItem.Children.Add(edgeItem);

                if (selectedEntities.Contains(edge))
                {
                    newSelectedItems.Add(edgeItem);
                    edgeItem.IsSelected = true;
                }
            }
        }

        if (solid.Faces.Count > 0)
        {
            var facesItem = new ModelTreeItem($"Faces ({solid.Faces.Count})");
            rootItem.Children.Add(facesItem);

            foreach (Face face in solid.Faces)
            {
                string surfaceDescription = face.Surface.GetType().Name;
                var faceItem = new ModelTreeItem($"Face {face.Id}: {surfaceDescription}", face);
                facesItem.Children.Add(faceItem);

                if (selectedEntities.Contains(face))
                {
                    newSelectedItems.Add(faceItem);
                    faceItem.IsSelected = true;
                }
            }
        }

        SynchronizeSelectedItems(newSelectedItems);
    }

    public void SelectEntity(BRepEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        ClearAllSelections();

        ModelTreeItem? itemToSelect = FindItem(entity);
        if (itemToSelect != null)
        {
            SelectedItems.Add(itemToSelect);
            itemToSelect.IsSelected = true;
        }
    }

    public ModelTreeItem? FindItem(BRepEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        foreach (ModelTreeItem rootItem in Items)
        {
            ModelTreeItem? found = FindItemRecursively(rootItem, entity);
            if (found != null) return found;
        }
        return null;
    }

    void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var selectedEntities = SelectedItems.Select(item => item.Data).OfType<BRepEntity>();
        _selectionService.NotifySelectionChanged(selectedEntities);
    }

    void SynchronizeSelectedItems(List<ModelTreeItem> newSelectedItems)
    {
        for (int i = SelectedItems.Count - 1; i >= 0; i--)
        {
            if (!newSelectedItems.Contains(SelectedItems[i]))
                SelectedItems.RemoveAt(i);
        }

        foreach (ModelTreeItem newItem in newSelectedItems)
        {
            if (!SelectedItems.Contains(newItem))
                SelectedItems.Add(newItem);
        }
    }

    void ClearAllSelections()
    {
        foreach (ModelTreeItem item in Items)
            ClearSelectionRecursively(item);
        
        SelectedItems.Clear();
    }

    static ModelTreeItem? FindItemRecursively(ModelTreeItem item, BRepEntity entity)
    {
        if (item.Data == entity) return item;

        foreach (ModelTreeItem child in item.Children)
        {
            ModelTreeItem? found = FindItemRecursively(child, entity);
            if (found != null) return found;
        }

        return null;
    }

    static void ClearSelectionRecursively(ModelTreeItem item)
    {
        item.IsSelected = false;
        foreach (ModelTreeItem child in item.Children)
            ClearSelectionRecursively(child);
    }
}