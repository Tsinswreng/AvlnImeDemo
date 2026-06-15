using Avalonia.Controls;
using Avalonia.Interactivity;
using AvlnImeDemo.ViewModels;

namespace AvlnImeDemo.Views;

public partial class ImeKeyboardView : UserControl
{
    public ImeKeyboardView()
    {
        InitializeComponent();
        DataContext = new ImeKeyboardViewModel();
    }

    private ImeKeyboardViewModel Vm => (ImeKeyboardViewModel)DataContext!;

    private void TypeA_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.TypeA();
    }

    private void Space_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.TypeSpace();
    }

    private void Backspace_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.Backspace();
    }

    private void Enter_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.Enter();
    }
}
