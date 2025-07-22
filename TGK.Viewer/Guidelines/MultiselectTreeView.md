I have a variation on SoMoS implementation that uses an attached property declared on a derivation of the base TreeView control to track the selection state of the TreeViewItems. This keeps the selection tracking on the TreeViewItem element itself, and off of the model object being presented by the tree-view.

This is the new TreeView class derivation.

using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;

namespace MultiSelectTreeViewDemo
{
public sealed class MultiSelectTreeView : TreeView
{
#region Fields

        // Used in shift selections
        private TreeViewItem _lastItemSelected;

        #endregion Fields
        #region Dependency Properties

        public static readonly DependencyProperty IsItemSelectedProperty =
            DependencyProperty.RegisterAttached("IsItemSelected", typeof(bool), typeof(MultiSelectTreeView));

        public static void SetIsItemSelected(UIElement element, bool value)
        {
            element.SetValue(IsItemSelectedProperty, value);
        }
        public static bool GetIsItemSelected(UIElement element)
        {
            return (bool)element.GetValue(IsItemSelectedProperty);
        }

        #endregion Dependency Properties
        #region Properties

        private static bool IsCtrlPressed
        {
            get { return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl); }
        }
        private static bool IsShiftPressed
        {
            get { return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift); }
        }
    
        public IList SelectedItems
        {
            get
            {
                var selectedTreeViewItems = GetTreeViewItems(this, true).Where(GetIsItemSelected);
                var selectedModelItems = selectedTreeViewItems.Select(treeViewItem => treeViewItem.Header);

                return selectedModelItems.ToList();
            }
        }

        #endregion Properties
        #region Event Handlers

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // If clicking on a tree branch expander...
            if (e.OriginalSource is Shape || e.OriginalSource is Grid || e.OriginalSource is Border)
                return;

            var item = GetTreeViewItemClicked((FrameworkElement)e.OriginalSource);
            if (item != null) SelectedItemChangedInternal(item);
        }

        #endregion Event Handlers
        #region Utility Methods

        private void SelectedItemChangedInternal(TreeViewItem tvItem)
        {
            // Clear all previous selected item states if ctrl is NOT being held down
            if (!IsCtrlPressed)
            {
                var items = GetTreeViewItems(this, true);
                foreach (var treeViewItem in items)
                    SetIsItemSelected(treeViewItem, false);
            }

            // Is this an item range selection?
            if (IsShiftPressed && _lastItemSelected != null)
            {
                var items = GetTreeViewItemRange(_lastItemSelected, tvItem);
                if (items.Count > 0)
                {
                    foreach (var treeViewItem in items)
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
        }
        private static TreeViewItem GetTreeViewItemClicked(DependencyObject sender)
        {
            while (sender != null && !(sender is TreeViewItem))
                sender = VisualTreeHelper.GetParent(sender);
            return sender as TreeViewItem;
        }
        private static List<TreeViewItem> GetTreeViewItems(ItemsControl parentItem, bool includeCollapsedItems, List<TreeViewItem> itemList = null)
        {
            if (itemList == null)
                itemList = new List<TreeViewItem>();

            for (var index = 0; index < parentItem.Items.Count; index++)
            {
                var tvItem = parentItem.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                if (tvItem == null) continue;

                itemList.Add(tvItem);
                if (includeCollapsedItems || tvItem.IsExpanded)
                    GetTreeViewItems(tvItem, includeCollapsedItems, itemList);
            }
            return itemList;
        }
        private List<TreeViewItem> GetTreeViewItemRange(TreeViewItem start, TreeViewItem end)
        {
            var items = GetTreeViewItems(this, false);

            var startIndex = items.IndexOf(start);
            var endIndex = items.IndexOf(end);
            var rangeStart = startIndex > endIndex || startIndex == -1 ? endIndex : startIndex;
            var rangeCount = startIndex > endIndex ? startIndex - endIndex + 1 : endIndex - startIndex + 1;

            if (startIndex == -1 && endIndex == -1)
                rangeCount = 0;

            else if (startIndex == -1 || endIndex == -1)
                rangeCount = 1;

            return rangeCount > 0 ? items.GetRange(rangeStart, rangeCount) : new List<TreeViewItem>();
        }

        #endregion Utility Methods
    }
}

And here's the XAML. Make note that the salient part is the replacement of the two triggers that use the singular 'IsSelected' property with the new 'IsItemSelected' attached property in the MultiSelectTreeViewItemStyle to achieve the visual state.

Also note I'm not aggregating the new TreeView control into a UserControl.

<Window
x:Class="MultiSelectTreeViewDemo.MainWindow"
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
xmlns:local="clr-namespace:MultiSelectTreeViewDemo"
Title="MultiSelect TreeView Demo" Height="350" Width="525">

    <Window.Resources>
        <local:DemoViewModel x:Key="ViewModel"/>
        <Style x:Key="TreeViewItemFocusVisual">
            <Setter Property="Control.Template">
                <Setter.Value>
                    <ControlTemplate>
                        <Rectangle/>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Checked.Fill" Color="#FF595959"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Checked.Stroke" Color="#FF262626"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Stroke" Color="#FF1BBBFA"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Fill" Color="Transparent"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Checked.Stroke" Color="#FF262626"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.MouseOver.Checked.Fill" Color="#FF595959"/>
        <PathGeometry x:Key="TreeArrow" Figures="M0,0 L0,6 L6,0 z"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Fill" Color="Transparent"/>
        <SolidColorBrush x:Key="TreeViewItem.TreeArrow.Static.Stroke" Color="#FF989898"/>
        <Style x:Key="ExpandCollapseToggleStyle" TargetType="{x:Type ToggleButton}">
            <Setter Property="Focusable" Value="False"/>
            <Setter Property="Width" Value="16"/>
            <Setter Property="Height" Value="16"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type ToggleButton}">
                        <Border Background="Transparent" Height="16" Padding="5,5,5,5" Width="16">
                            <Path x:Name="ExpandPath" Data="{StaticResource TreeArrow}" Fill="{StaticResource TreeViewItem.TreeArrow.Static.Fill}" Stroke="{StaticResource TreeViewItem.TreeArrow.Static.Stroke}">
                                <Path.RenderTransform>
                                    <RotateTransform Angle="135" CenterY="3" CenterX="3"/>
                                </Path.RenderTransform>
                            </Path>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsChecked" Value="True">
                                <Setter Property="RenderTransform" TargetName="ExpandPath">
                                    <Setter.Value>
                                        <RotateTransform Angle="180" CenterY="3" CenterX="3"/>
                                    </Setter.Value>
                                </Setter>
                                <Setter Property="Fill" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.Static.Checked.Fill}"/>
                                <Setter Property="Stroke" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.Static.Checked.Stroke}"/>
                            </Trigger>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Stroke" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.MouseOver.Stroke}"/>
                                <Setter Property="Fill" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.MouseOver.Fill}"/>
                            </Trigger>
                            <MultiTrigger>
                                <MultiTrigger.Conditions>
                                    <Condition Property="IsMouseOver" Value="True"/>
                                    <Condition Property="IsChecked" Value="True"/>
                                </MultiTrigger.Conditions>
                                <Setter Property="Stroke" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.MouseOver.Checked.Stroke}"/>
                                <Setter Property="Fill" TargetName="ExpandPath" Value="{StaticResource TreeViewItem.TreeArrow.MouseOver.Checked.Fill}"/>
                            </MultiTrigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
        <Style x:Key="MultiSelectTreeViewItemStyle" TargetType="{x:Type TreeViewItem}">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="HorizontalContentAlignment" Value="{Binding HorizontalContentAlignment, RelativeSource={RelativeSource AncestorType={x:Type ItemsControl}}}"/>
            <Setter Property="VerticalContentAlignment" Value="{Binding VerticalContentAlignment, RelativeSource={RelativeSource AncestorType={x:Type ItemsControl}}}"/>
            <Setter Property="Padding" Value="1,0,0,0"/>
            <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}"/>
            <Setter Property="FocusVisualStyle" Value="{StaticResource TreeViewItemFocusVisual}"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="{x:Type TreeViewItem}">
                        <Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition MinWidth="19" Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition/>
                            </Grid.RowDefinitions>
                            <ToggleButton 
                                x:Name="Expander" 
                                ClickMode="Press" 
                                IsChecked="{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}}" 
                                Style="{StaticResource ExpandCollapseToggleStyle}"/>
                            <Border 
                                x:Name="Bd" 
                                BorderBrush="{TemplateBinding BorderBrush}" 
                                BorderThickness="{TemplateBinding BorderThickness}" 
                                Background="{TemplateBinding Background}" 
                                Grid.Column="1" 
                                Padding="{TemplateBinding Padding}" 
                                SnapsToDevicePixels="true">
                                <ContentPresenter 
                                    x:Name="PART_Header" 
                                    ContentSource="Header" 
                                    HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" 
                                    SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}"/>
                            </Border>
                            <ItemsPresenter 
                                x:Name="ItemsHost" 
                                Grid.ColumnSpan="2" 
                                Grid.Column="1" 
                                Grid.Row="1"/>
                        </Grid>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsExpanded" Value="false">
                                <Setter Property="Visibility" TargetName="ItemsHost" Value="Collapsed"/>
                            </Trigger>
                            <Trigger Property="HasItems" Value="false">
                                <Setter Property="Visibility" TargetName="Expander" Value="Hidden"/>
                            </Trigger>
                            <!--Trigger Property="IsSelected" Value="true"-->
                            <Trigger Property="local:MultiSelectTreeView.IsItemSelected" Value="true">
                                <Setter Property="Background" TargetName="Bd" Value="{DynamicResource {x:Static SystemColors.HighlightBrushKey}}"/>
                                <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.HighlightTextBrushKey}}"/>
                            </Trigger>
                            <MultiTrigger>
                                <MultiTrigger.Conditions>
                                    <!--Condition Property="IsSelected" Value="true"/-->
                                    <Condition Property="local:MultiSelectTreeView.IsItemSelected" Value="true"/>
                                    <Condition Property="IsSelectionActive" Value="false"/>
                                </MultiTrigger.Conditions>
                                <Setter Property="Background" TargetName="Bd" Value="{DynamicResource {x:Static SystemColors.InactiveSelectionHighlightBrushKey}}"/>
                                <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.InactiveSelectionHighlightTextBrushKey}}"/>
                            </MultiTrigger>
                            <Trigger Property="IsEnabled" Value="false">
                                <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.GrayTextBrushKey}}"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="VirtualizingPanel.IsVirtualizing" Value="true">
                    <Setter Property="ItemsPanel">
                        <Setter.Value>
                            <ItemsPanelTemplate>
                                <VirtualizingStackPanel/>
                            </ItemsPanelTemplate>
                        </Setter.Value>
                    </Setter>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Grid
        Background="WhiteSmoke"
        DataContext="{DynamicResource ViewModel}">
        <Grid.RowDefinitions>
            <RowDefinition/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <local:MultiSelectTreeView
            x:Name="multiSelectTreeView"
            ItemContainerStyle="{StaticResource MultiSelectTreeViewItemStyle}"
            ItemsSource="{Binding FoodGroups}">
            <local:MultiSelectTreeView.ItemTemplate>
                <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                    <Grid>
                        <TextBlock FontSize="14" Text="{Binding Name}"/>
                    </Grid>
                </HierarchicalDataTemplate>
            </local:MultiSelectTreeView.ItemTemplate>
        </local:MultiSelectTreeView>
        <Button 
            Grid.Row="1" 
            Margin="0,10" 
            Padding="20,2"
            HorizontalAlignment="Center"
            Content="Get Selections" 
            Click="GetSelectionsButton_OnClick"/>
    </Grid>
</Window>

And here's a cheesy view-model to drive it (for demo purposes).

using System.Collections.ObjectModel;

namespace MultiSelectTreeViewDemo
{
public sealed class DemoViewModel
{
public ObservableCollection<FoodItem> FoodGroups { get; set; }

        public DemoViewModel()
        {
            var redMeat = new FoodItem { Name = "Reds" };
            redMeat.Add(new FoodItem { Name = "Beef" });
            redMeat.Add(new FoodItem { Name = "Buffalo" });
            redMeat.Add(new FoodItem { Name = "Lamb" });
        
            var whiteMeat = new FoodItem { Name = "Whites" };
            whiteMeat.Add(new FoodItem { Name = "Chicken" });
            whiteMeat.Add(new FoodItem { Name = "Duck" });
            whiteMeat.Add(new FoodItem { Name = "Pork" });
            var meats = new FoodItem { Name = "Meats", Children = { redMeat, whiteMeat } };

            var veggies = new FoodItem { Name = "Vegetables" };
            veggies.Add(new FoodItem { Name = "Potato" });
            veggies.Add(new FoodItem { Name = "Corn" });
            veggies.Add(new FoodItem { Name = "Spinach" });

            var fruits = new FoodItem { Name = "Fruits" };
            fruits.Add(new FoodItem { Name = "Apple" });
            fruits.Add(new FoodItem { Name = "Orange" });
            fruits.Add(new FoodItem { Name = "Pear" });

            FoodGroups = new ObservableCollection<FoodItem> { meats, veggies, fruits };
        }
    }
    public sealed class FoodItem
    {
        public string Name { get; set; }
        public ObservableCollection<FoodItem> Children { get; set; }

        public FoodItem()
        {
            Children = new ObservableCollection<FoodItem>();
        }
        public void Add(FoodItem item)
        {
            Children.Add(item);
        }
    }
}

And here's the button click-handler on the MainWindow code-behind that shows the selections in a MessageBox.

    private void GetSelectionsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var selectedMesg = "";
        var selectedItems = multiSelectTreeView.SelectedItems;

        if (selectedItems.Count > 0)
        {
            selectedMesg = selectedItems.Cast<FoodItem>()
                .Where(modelItem => modelItem != null)
                .Aggregate(selectedMesg, (current, modelItem) => current + modelItem.Name + Environment.NewLine);
        }
        else
            selectedMesg = "No selected items!";

        MessageBox.Show(selectedMesg, "MultiSelect TreeView Demo", MessageBoxButton.OK);
    }

Hope this helps.