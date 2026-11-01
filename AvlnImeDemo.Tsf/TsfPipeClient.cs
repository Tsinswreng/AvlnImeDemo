namespace AvlnImeDemo.Tsf;

/// <summary>One-way notifications sent from an in-process text service to the Avalonia candidate UI.</summary>
internal static partial class TsfPipeClient {
    /// <summary>Asks the candidate window to display the active preedit.</summary>
    internal static partial void Show(string Preedit);
    /// <summary>Asks the candidate window to hide.</summary>
    internal static partial void Hide();
}
