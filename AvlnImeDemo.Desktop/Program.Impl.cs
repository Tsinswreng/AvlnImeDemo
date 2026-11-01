namespace AvlnImeDemo.Desktop;

using Avalonia;
using AvlnImeDemo.Ime;
using System.Threading;

/// <summary>Avalonia 候選窗程序的啟動與關閉實作。</summary>
sealed partial class Program {
    /// <summary>依命令列啟動候選窗，或轉送關閉請求至既有執行個體。</summary>
    public static partial void Main(string[] Args) {
        if (Args.Length == 1 && Args[0] == "--shutdown") {
            CandidatePipeClient.RequestShutdown(CancellationToken.None).GetAwaiter().GetResult();
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(Args);
    }

    /// <summary>建立桌面 Avalonia 應用程式組態。</summary>
    public static partial AppBuilder BuildAvaloniaApp() {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
    }
}
