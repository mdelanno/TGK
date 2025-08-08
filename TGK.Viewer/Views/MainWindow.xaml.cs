using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using System.Windows.Input;
using TGK.Viewer.Services;
using TGK.Viewer.ViewModels;
using Point = System.Windows.Point;

namespace TGK.Viewer.Views;

public sealed partial class MainWindow : IView
{
    public double ViewportWidth => modelSpaceViewport!.ActualWidth;

    public double ViewportHeight => modelSpaceViewport!.ActualHeight;

    public MainWindow()
    {
        InitializeComponent();

        var selectionService = new SelectionService();
        var viewModel = new MainViewModel(this, selectionService);
        DataContext = viewModel;
    }

    public void ModelSpaceZoomExtents()
    {
        modelSpaceViewport!.ZoomExtents();
    }

    public void ParametricSpaceZoomExtents()
    {
        parametricSpaceViewport!.ZoomExtents();
    }

    public IList<HitTestResult>? HitTest(Point position)
    {
        return modelSpaceViewport!.FindHits(position);
    }

    public void ExpandParentNodesForSelectedItem(ModelTreeItem modelTreeItem)
    {
        ArgumentNullException.ThrowIfNull(modelTreeItem);

        modelTreeView?.ExpandParentNodesForSelectedItem(modelTreeItem);
    }

    void Viewport3Dx_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Only handle left clicks for selection
        if (e.ChangedButton == MouseButton.Left)
        {
            Point position = e.GetPosition(modelSpaceViewport!);

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectEntityCommand.Execute(position);
            }
        }
    }

    public BoundingBox CalculateModelSpaceBoundingBox() => modelSpaceViewport!.FindBounds();
}