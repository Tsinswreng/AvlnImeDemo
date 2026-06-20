using System;

namespace AvlnImeDemo.Ime;

public interface IImeServiceBridge
{
    event Action<string>? CommitTextRequested;
    event Action<int>? DeleteSurroundingTextRequested;
    event Action<string>? KeyEventRequested;
    event Action? HideKeyboardRequested;
}

public sealed class ImeServiceBridge : IImeServiceBridge
{
    public static ImeServiceBridge Instance { get; } = new();

    public event Action<string>? CommitTextRequested;
    public event Action<int>? DeleteSurroundingTextRequested;
    public event Action<string>? KeyEventRequested;
    public event Action? HideKeyboardRequested;

    private ImeServiceBridge()
    {
    }

    public void CommitText(string text)
    {
        CommitTextRequested?.Invoke(text);
    }

    public void DeleteSurroundingText(int beforeLength)
    {
        DeleteSurroundingTextRequested?.Invoke(beforeLength);
    }

    public void SendKeyEvent(string keyName)
    {
        KeyEventRequested?.Invoke(keyName);
    }

    public void HideKeyboard()
    {
        HideKeyboardRequested?.Invoke();
    }
}
