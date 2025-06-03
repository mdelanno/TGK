using System.Windows.Input;

namespace TGK.Viewer;

static class TgkViewerCommands
{
    public static ICommand Exit { get; } = new ExitCommand();
}