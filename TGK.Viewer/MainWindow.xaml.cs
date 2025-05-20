using TGK.Viewer.ViewModels;

namespace TGK.Viewer;

public sealed partial class MainWindow : IView
{
    public double ViewportWidth => viewport3Dx!.ActualWidth;

    public double ViewportHeight => viewport3Dx!.ActualHeight;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel(this);
    }

    public void ZoomExtents()
    {
        viewport3Dx!.ZoomExtents();
    }
}