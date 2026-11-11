namespace AvlnImeDemo.Ime;

using Avalonia.Threading;
using AvlnImeDemo.Views;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Implementation details for the candidate-window named-pipe receiver.</summary>
internal static class CandidatePipeServerImpl {
    /// <summary>Accepts one message per short-lived pipe connection until cancellation.</summary>
    internal static async Task Run(CandidateWindow Window, CancellationToken Ct) {
        while (!Ct.IsCancellationRequested) {
            using var Server = new NamedPipeServerStream(CandidatePipeServer.PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try {
                await Server.WaitForConnectionAsync(Ct);
                using var Reader = new StreamReader(Server, Encoding.UTF8, leaveOpen: true);
                var Message = await Reader.ReadLineAsync(Ct);
                Dispatch(Window, Message);
            }
            catch (OperationCanceledException) {
                return;
            }
            catch (IOException) {
                // A client can close while writing; the next pipe instance remains available.
            }
        }
    }
    /// <summary>Translates the fixed pipe protocol into UI-thread work.</summary>
    private static void Dispatch(CandidateWindow Window, string? Message) {
        if (Message == CandidatePipeServer.ShutdownMessage) {
            Dispatcher.UIThread.Post(Window.Close);
            return;
        }
        if (Message is null || Message == "HIDE") {
            Dispatcher.UIThread.Post(Window.HidePreedit);
            return;
        }
        const string ShowPrefix = "SHOW|";
        if (!Message.StartsWith(ShowPrefix, StringComparison.Ordinal)) {
            return;
        }
        try {
            var Payload = Message[ShowPrefix.Length..];
            // 新格式：SHOW|x|y|base64；舊格式：SHOW|base64。
            var Parts = Payload.Split('|');
            if (Parts.Length == 3 && int.TryParse(Parts[0], out var X) && int.TryParse(Parts[1], out var Y)) {
                var Preedit = Encoding.UTF8.GetString(Convert.FromBase64String(Parts[2]));
                Dispatcher.UIThread.Post(() => Window.ShowPreeditAt(Preedit, X, Y));
                return;
            }
            var Legacy = Encoding.UTF8.GetString(Convert.FromBase64String(Payload));
            Dispatcher.UIThread.Post(() => Window.ShowPreedit(Legacy));
        }
        catch (FormatException) {
            // Ignore malformed input instead of allowing another process to terminate the UI loop.
        }
    }
}
