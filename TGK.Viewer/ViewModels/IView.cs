using HelixToolkit.SharpDX.Core;
using SharpDX;

namespace TGK.Viewer.ViewModels;

public interface IView
{
    double ViewportWidth { get; }

    void ModelSpaceZoomExtents();

    IList<HitTestResult>? HitTest(System.Windows.Point position);

    void ExpandParentNodesForSelectedItem(ModelTreeItem modelTreeItem);

    void ParametricSpaceZoomExtents();

    BoundingBox CalculateModelSpaceBoundingBox();
}