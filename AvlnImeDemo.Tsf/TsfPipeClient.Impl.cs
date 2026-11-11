namespace AvlnImeDemo.Tsf;

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

internal static partial class TsfPipeClient {
    /// <summary>Asks the candidate window to display the active preedit.</summary>
    internal static partial void Show(string Preedit) {
        Send("SHOW|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(Preedit)));
    }

    /// <summary>Asks the candidate window to hide.</summary>
    internal static partial void Hide() {
        Send("HIDE");
    }

    /// <summary>Sends a bounded best-effort notification so a missing UI cannot delay keystrokes.</summary>
    private static void Send(string Message) {
        try {
            TsfLog.Write("Pipe send " + (Message.Length > 32 ? Message[..32] + "..." : Message));
            using var Client = new NamedPipeClientStream(".", TsfIds.CandidatePipeName, PipeDirection.Out);
            Client.Connect(10);
            using var Writer = new StreamWriter(Client, Encoding.UTF8, leaveOpen: false) { AutoFlush = true };
            Writer.WriteLine(Message);
            TsfLog.Write("Pipe sent");
        }
        catch (Exception Exception) {
            // Candidate UI startup and restart must never block the host application's key thread.
            TsfLog.Write("Pipe failed " + Exception.GetType().Name + ": " + Exception.Message);
        }
    }
}
