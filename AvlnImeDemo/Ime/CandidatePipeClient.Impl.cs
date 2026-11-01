namespace AvlnImeDemo.Ime;

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>候選窗關閉訊息的命名管道實作。</summary>
public static partial class CandidatePipeClient {
    /// <summary>連線至候選窗並要求其結束；未啟動的候選窗不應視為錯誤。</summary>
    public static async partial Task RequestShutdown(CancellationToken Ct) {
        try {
            await using var Client = new NamedPipeClientStream(".", CandidatePipeServer.PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
            await Client.ConnectAsync(500, Ct);
            await using var Writer = new StreamWriter(Client, Encoding.UTF8, leaveOpen: true);
            await Writer.WriteLineAsync(CandidatePipeServer.ShutdownMessage);
            await Writer.FlushAsync(Ct);
        }
        catch (IOException) {
            // 沒有候選窗或候選窗正在結束時，停止請求自然完成。
        }
        catch (TimeoutException) {
            // 命名管道在短暫等待內沒有服務端，表示沒有需要停止的候選窗。
        }
    }
}
