namespace AvlnImeDemo.Views;

using Avalonia.Controls;

public sealed partial class CandidateWindow {
    /// <summary>Shows the supplied preedit without moving keyboard focus away from the target application.</summary>
    public partial void ShowPreedit(string Preedit) {
        PreeditText.Text = Preedit;
        // 隱藏服務啟動時刻意使用透明視窗，收到真實 TSF preedit 後才使其可見。
        Opacity = 1;
        if (!IsVisible) {
            Show();
        }
    }
    /// <summary>Hides the candidate surface when composition ends or is cancelled.</summary>
    public partial void HidePreedit() {
        if (IsVisible) {
            Hide();
        }
    }
}
