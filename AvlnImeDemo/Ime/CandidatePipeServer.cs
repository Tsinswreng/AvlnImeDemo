namespace AvlnImeDemo.Ime;

using AvlnImeDemo.Views;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Receives best-effort candidate-window state from TSF text-service instances.</summary>
internal interface ICandidatePipeServer {
    /// <summary>Starts accepting UI messages until application shutdown.</summary>
    Task Run(CandidateWindow Window, CancellationToken Ct);
}

/// <summary>Named-pipe implementation used solely by the Avalonia desktop process.</summary>
internal sealed class CandidatePipeServer : ICandidatePipeServer {
    /// <summary>Stable pipe name shared with the C# TSF service.</summary>
    internal const string PipeName = "AvlnImeDemo.CandidateWindow.v1";
    /// <summary>請求候選窗結束其 Avalonia 生命週期的控制訊息。</summary>
    internal const string ShutdownMessage = "EXIT";
    /// <summary>Starts accepting UI messages until application shutdown.</summary>
    public Task Run(CandidateWindow Window, CancellationToken Ct) {
        return CandidatePipeServerImpl.Run(Window, Ct);
    }
}
