using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AvlnImeDemo.ViewModels;
using AvlnImeDemo.Views;
using AvlnImeDemo.Ime;
using System.Threading;

namespace AvlnImeDemo;

public partial class App : Application
{
    /// <summary>Cancels the candidate pipe listener during application shutdown.</summary>
    private readonly CancellationTokenSource CandidatePipeCancellation = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var CandidateWindow = new CandidateWindow();
            // 此程序是背景候選窗服務，不應由 Avalonia 的主視窗生命週期自動顯示或自動結束。
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.Exit += (_, _) => CandidatePipeCancellation.Cancel();
            CandidateWindow.Closed += (_, _) => desktop.Shutdown();
            desktop.Startup += (_, _) => {
                // 先建立原生視窗句柄再隱藏，確保管道服務常駐但永不顯示空白候選窗。
                CandidateWindow.Show();
                CandidateWindow.Hide();
                _ = new CandidatePipeServer().Run(CandidateWindow, CandidatePipeCancellation.Token);
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
