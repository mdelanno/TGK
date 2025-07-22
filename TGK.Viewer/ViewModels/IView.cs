using HelixToolkit.SharpDX.Core;

namespace TGK.Viewer.ViewModels;

public interface IView
{
    double ViewportHeight { get; }

    double ViewportWidth { get; }

    void ZoomExtents();

    IList<HitTestResult>? HitTest(System.Windows.Point position);

    void ExpandParentNodesForSelectedItem(ModelTreeItem modelTreeItem);
}