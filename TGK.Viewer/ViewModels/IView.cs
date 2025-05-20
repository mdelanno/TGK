namespace TGK.Viewer.ViewModels;

public interface IView
{
    double ViewportHeight { get; }

    double ViewportWidth { get; }

    void ZoomExtents();
}