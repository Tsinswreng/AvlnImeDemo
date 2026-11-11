namespace AvlnImeDemo.Views;

using Avalonia;
using Avalonia.Controls;
using System;

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
    /// <summary>Shows the preedit next to the caret reported by the TSF side.</summary>
    public partial void ShowPreeditAt(string Preedit, int X, int Y) {
        PreeditText.Text = Preedit;
        Opacity = 1;
        if (!IsVisible) {
            Show();
        }
        MoveToCaret(X, Y);
    }

    /// <summary>Moves the surface below the caret and keeps it inside the working area.</summary>
    private void MoveToCaret(int X, int Y) {
        var Width = (int)(Bounds.Width > 0 ? Bounds.Width : 360);
        var Height = (int)(Bounds.Height > 0 ? Bounds.Height : 80);
        var Target = new PixelPoint(X, Y);
        var Screen = Screens.ScreenFromPoint(Target) ?? Screens.Primary;
        if (Screen is not null) {
            var Area = Screen.WorkingArea;
            if (Target.X + Width > Area.Right) {
                Target = new PixelPoint(Math.Max(Area.X, Area.Right - Width), Target.Y);
            }
            if (Target.Y + Height > Area.Bottom) {
                Target = new PixelPoint(Target.X, Math.Max(Area.Y, Y - Height - 24));
            }
        }
        Position = Target;
    }

    /// <summary>Hides the candidate surface when composition ends or is cancelled.</summary>
    public partial void HidePreedit() {
        if (IsVisible) {
            Hide();
        }
    }
}
