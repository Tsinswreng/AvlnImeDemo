namespace AvlnImeDemo.Tsf;

using System;
using System.IO;
using System.Text;

/// <summary>
/// 最小化的檔案日誌。TSF 被載入到宿主行程（notepad、explorer）後無法用主控台觀察，
/// 因此所有 COM 入口與編輯工作階段都寫入此檔，用來定位崩潰前最後執行的步驟。
/// 日誌路徑：%LOCALAPPDATA%\NganImePoc\tsf.log，初始化失敗時靜默停用。
/// </summary>
internal static class TsfLog {
    /// <summary>多執行緒寫入時的互斥鎖；TSF 回呼可能來自不同執行緒。</summary>
    private static readonly object Gate = new();

    /// <summary>日誌檔路徑；null 表示無法建立日誌目錄。</summary>
    private static readonly string? FilePath = ResolvePath();

    /// <summary>建立日誌檔並寫入行程啟動記錄。</summary>
    static TsfLog() {
        Write("TsfLog start pid=" + Environment.ProcessId);
    }

    /// <summary>寫入一行帶時間戳與行程識別碼的日誌；任何失敗都不影響輸入法。</summary>
    internal static void Write(string Message) {
        try {
            if (FilePath is null) {
                return;
            }
            var Line = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + Environment.ProcessId + "] " + Message + Environment.NewLine;
            lock (Gate) {
                File.AppendAllText(FilePath, Line, Encoding.UTF8);
            }
        }
        catch {
            // 日誌是除錯輔助，寫入失敗不得影響按鍵處理。
        }
    }

    /// <summary>解析日誌路徑，優先使用目前使用者的 LocalAppData。</summary>
    private static string? ResolvePath() {
        try {
            var DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NganImePoc");
            Directory.CreateDirectory(DirectoryPath);
            return Path.Combine(DirectoryPath, "tsf.log");
        }
        catch {
            return null;
        }
    }
}
