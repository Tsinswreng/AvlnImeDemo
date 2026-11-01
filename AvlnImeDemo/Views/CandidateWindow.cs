namespace AvlnImeDemo.Views;

using Avalonia.Controls;

/// <summary>Non-activating Avalonia surface that mirrors a TSF composition from another process.</summary>
public sealed partial class CandidateWindow : Window {
    /// <summary>Creates the candidate surface and resolves its compiled XAML controls.</summary>
    public CandidateWindow() {
        InitializeComponent();
    }
    /// <summary>Shows the supplied preedit without moving keyboard focus away from the target application.</summary>
    public partial void ShowPreedit(string Preedit);
    /// <summary>Hides the candidate surface when composition ends or is cancelled.</summary>
    public partial void HidePreedit();
}
