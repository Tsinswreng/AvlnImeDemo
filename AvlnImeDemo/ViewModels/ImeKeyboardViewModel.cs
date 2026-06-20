using AvlnImeDemo.Ime;

namespace AvlnImeDemo.ViewModels;

public sealed class ImeKeyboardViewModel : ViewModelBase
{
    private readonly ImeServiceBridge _ime = ImeServiceBridge.Instance;

    public void TypeA()
    {
        _ime.CommitText("a");
    }

    public void TypeSpace()
    {
        _ime.CommitText(" ");
    }

    public void Backspace()
    {
        _ime.DeleteSurroundingText(1);
    }

    public void Enter()
    {
        _ime.SendKeyEvent("Enter");
    }

    public void HideKeyboard()
    {
        _ime.HideKeyboard();
    }
}
