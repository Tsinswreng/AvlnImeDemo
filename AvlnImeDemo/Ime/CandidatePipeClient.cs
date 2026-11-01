namespace AvlnImeDemo.Ime;

using System.Threading;
using System.Threading.Tasks;

/// <summary>向已運行的 Avalonia 候選窗發送生命週期控制訊息。</summary>
public static partial class CandidatePipeClient {
    /// <summary>請求候選窗關閉；候選窗未運行時視為已完成。</summary>
    public static partial Task RequestShutdown(CancellationToken Ct);
}
