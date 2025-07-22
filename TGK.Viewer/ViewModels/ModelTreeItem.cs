using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TGK.Topology;

namespace TGK.Viewer.ViewModels;

public sealed class ModelTreeItem : ObservableObject
{
    string _displayName = string.Empty;

    bool _isSelected;

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public ObservableCollection<ModelTreeItem> Children { get; } = [];

    public BRepEntity? Data { get; set; }

    public ModelTreeItem(string displayName, BRepEntity? data = null)
    {
        DisplayName = displayName;
        Data = data;
    }
}