using System.Windows;
using System.Windows.Input;

namespace TGK.Viewer;

sealed class ExitCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        Application.Current!.Shutdown();
    }
}