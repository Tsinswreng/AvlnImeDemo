using System;
using Avalonia;

namespace AvlnImeDemo.Desktop;

sealed partial class Program
{
    /// <summary>啟動候選窗，或向既有候選窗發出關閉請求。</summary>
    [STAThread]
    public static partial void Main(string[] Args);

    /// <summary>建立桌面 Avalonia 應用程式組態。</summary>
    public static partial AppBuilder BuildAvaloniaApp();
}
