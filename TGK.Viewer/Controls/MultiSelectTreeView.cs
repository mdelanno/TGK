using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TGK.Viewer.ViewModels;

namespace TGK.Viewer.Controls;

public sealed class MultiSelectTreeView : TreeView
{
    // Used in shift selections
    TreeViewItem? _lastItemSelected;

    bool _synchronisationInProgress;

    public static readonly DependencyProperty IsItemSelectedProperty =
        DependencyProperty.RegisterAttached("IsItemSelected", typeof(bool), typeof(MultiSelectTreeView),
            new PropertyMetadata(defaultValue: false, OnIsItemSelectedChanged));

    public static void SetIsItemSelected(UIElement element, bool value)
    {
        element.SetValue(IsItemSelectedProperty, value);
    }

    public static bool GetIsItemSelected(UIElement element)
    {
        return (bool)element.GetValue(IsItemSelectedProperty);
    }

    static bool IsCtrlPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

    static bool IsShiftPressed => Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(MultiSelectTreeView),
            new PropertyMetadata(null!, OnSelectedItemsChanged));

    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public IList GetSelectedItems()
    {
        IEnumerable<TreeViewItem> selectedTreeViewItems = GetTreeViewItems(this, true).Where(GetIsItemSelected);
        IEnumerable<object> selectedModelItems = selectedTreeViewItems
            .Select(treeViewItem => treeViewItem.Header)
            .Where(header => header != null)!;
        return selectedModelItems.ToList();
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        // If clicking on a tree branch expander...
        if (e.OriginalSource is Shape or Grid or Border)
            return;

        TreeViewItem? item = GetTreeViewItemClicked((FrameworkElement?)e.OriginalSource);
        if (item != null) SelectedItemChangedInternal(item);
    }

    void SelectedItemChangedInternal(TreeViewItem tvItem)
    {
        // Clear all previous selected item states if ctrl is NOT being held down
        if (!IsCtrlPressed)
        {
            List<TreeViewItem> items = GetTreeViewItems(this, true);
            foreach (TreeViewItem treeViewItem in items)
                SetIsItemSelected(treeViewItem, false);
        }

        // Is this an item range selection?
        if (IsShiftPressed && _lastItemSelected != null)
        {
            List<TreeViewItem> items = GetTreeViewItemRange(_lastItemSelected, tvItem);
            if (items.Count > 0)
            {
                foreach (TreeViewItem treeViewItem in items)
                    SetIsItemSelected(treeViewItem, true);

                _lastItemSelected = items.Last();
            }
        }
        // Otherwise, individual selection
        else
        {
            SetIsItemSelected(tvItem, true);
            _lastItemSelected = tvItem;
        }

        // Update the bound SelectedItems collection
        UpdateSelectedItemsCollection();
    }

    static TreeViewItem? GetTreeViewItemClicked(DependencyObject? sender)
    {
        while (sender != null && sender is not TreeViewItem)
            sender = VisualTreeHelper.GetParent(sender);
        return sender as TreeViewItem;
    }

    static List<TreeViewItem> GetTreeViewItems(ItemsControl parentItem, bool includeCollapsedItems, List<TreeViewItem>? itemList = null)
    {
        itemList ??= [];

        for (int index = 0; index < parentItem.Items.Count; index++)
        {
            var tvItem = parentItem.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
            if (tvItem == null) continue;

            itemList.Add(tvItem);
            if (includeCollapsedItems || tvItem.IsExpanded)
                GetTreeViewItems(tvItem, includeCollapsedItems, itemList);
        }
        return itemList;
    }

    List<TreeViewItem> GetTreeViewItemRange(TreeViewItem start, TreeViewItem end)
    {
        List<TreeViewItem> items = GetTreeViewItems(this, false);

        int startIndex = items.IndexOf(start);
        int endIndex = items.IndexOf(end);
        int rangeStart = startIndex > endIndex || startIndex == -1 ? endIndex : startIndex;
        int rangeCount = startIndex > endIndex ? startIndex - endIndex + 1 : endIndex - startIndex + 1;

        if (startIndex == -1 && endIndex == -1)
            rangeCount = 0;
        else if (startIndex == -1 || endIndex == -1)
            rangeCount = 1;

        return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : new List<TreeViewItem>();
    }

    void UpdateSelectedItemsCollection()
    {
        if (SelectedItems == null) return;

        IList selectedItems = GetSelectedItems();
        
        // Remove items that are no longer selected (iterate backwards to safely remove)
        for (int i = SelectedItems.Count - 1; i >= 0; i--)
        {
            object? item = SelectedItems[i];
            if (item != null && !selectedItems.Contains(item))
            {
                SelectedItems.RemoveAt(i);
            }
        }
        
        // Add newly selected items
        foreach (object? item in selectedItems)
        {
            if (item != null && !SelectedItems.Contains(item))
            {
                SelectedItems.Add(item);
            }
        }
    }

    static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiSelectTreeView treeView)
        {
            treeView.SyncTreeViewSelection();
        }
    }

    void SyncTreeViewSelection()
    {
        List<TreeViewItem> allItems = GetTreeViewItems(this, true);
        
        // Create a set of data items that should be selected for faster lookup
        var itemsToSelect = new HashSet<object>();
        if (SelectedItems != null)
        {
            foreach (object item in SelectedItems)
            {
                if (item != null)
                    itemsToSelect.Add(item);
            }
        }

        // Update selection state only when it differs from current state
        // Use SetValue directly to bypass the event handler
        foreach (TreeViewItem treeViewItem in allItems)
        {
            bool shouldBeSelected = treeViewItem.Header != null && itemsToSelect.Contains(treeViewItem.Header);
            bool isCurrentlySelected = GetIsItemSelected(treeViewItem);
            
            if (shouldBeSelected != isCurrentlySelected)
            {
                treeViewItem.SetValue(IsItemSelectedProperty, shouldBeSelected);
            }
        }
        
        // Expand parent nodes for selected items after all selections are set
        foreach (TreeViewItem treeViewItem in allItems)
        {
            if (GetIsItemSelected(treeViewItem) && treeViewItem.Header != null)
            {
                ExpandParentNodesForSelectedItem(treeViewItem.Header);
            }
        }
    }

    public void ExpandParentNodesForSelectedItem(object selectedItem)
    {
        if (selectedItem is not ModelTreeItem modelTreeItem)
            return;

        // Find the complete path from root to target item
        var pathFromRoot = new List<ModelTreeItem>();
        if (FindPathFromRoot(modelTreeItem, pathFromRoot))
        {
            // Expand each level step by step, starting from the root
            ExpandPathStepByStep(pathFromRoot);
        }
    }

    bool FindPathFromRoot(ModelTreeItem targetItem, List<ModelTreeItem> pathFromRoot)
    {
        // Search through all root items
        foreach (object rootObject in Items)
        {
            if (rootObject is ModelTreeItem rootItem)
            {
                pathFromRoot.Clear();
                pathFromRoot.Add(rootItem);
                
                if (rootItem == targetItem)
                {
                    return true; // Target is a root item
                }
                
                if (FindPathRecursively(rootItem, targetItem, pathFromRoot))
                {
                    return true; // Found the path
                }
            }
        }
        return false;
    }

    static bool FindPathRecursively(ModelTreeItem currentItem, ModelTreeItem targetItem, List<ModelTreeItem> pathFromRoot)
    {
        foreach (ModelTreeItem child in currentItem.Children)
        {
            pathFromRoot.Add(child);
            
            if (child == targetItem)
            {
                return true; // Found the target
            }
            
            if (FindPathRecursively(child, targetItem, pathFromRoot))
            {
                return true; // Found in deeper level
            }
            
            pathFromRoot.RemoveAt(pathFromRoot.Count - 1); // Backtrack
        }
        return false;
    }

    void ExpandPathStepByStep(List<ModelTreeItem> pathFromRoot)
    {
        ItemsControl currentContainer = this;
        
        // Expand each level except the last one (the target item itself doesn't need to be expanded)
        // We need to disable some event handlers that will change the current selection
        _synchronisationInProgress = true;
        try
        {
            for (int i = 0; i < pathFromRoot.Count - 1; i++)
            {
                ModelTreeItem currentDataItem = pathFromRoot[i];
            
                // Find the container for this data item
                DependencyObject? container = currentContainer.ItemContainerGenerator.ContainerFromItem(currentDataItem);
            
                if (container is TreeViewItem treeViewItem)
                {
                    // Expand this level
                    treeViewItem.IsExpanded = true;
                
                    // Force the container generator to create child containers. This will trigger a change on the IsItemSelected attached property,
                    // so we need to set _synchronisationInProgress to true to not change the current selection
                    treeViewItem.UpdateLayout();
                
                    // Move to the next level
                    currentContainer = treeViewItem;
                }
                else
                {
                    // Container not found, can't continue
                    break;
                }
            }
        }
        finally
        {
            _synchronisationInProgress = false;
        }
    }

    static void OnIsItemSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeViewItem treeViewItem)
        {
            // Find the parent MultiSelectTreeView
            MultiSelectTreeView? treeView = FindParentTreeView(treeViewItem);
            if (treeView is { _synchronisationInProgress: false })
            {
                // Update the SelectedItems collection when IsItemSelected changes
                treeView.UpdateSelectedItemsCollection();
            }
        }
    }

    static MultiSelectTreeView? FindParentTreeView(DependencyObject child)
    {
        DependencyObject? parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is MultiSelectTreeView treeView)
                return treeView;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}