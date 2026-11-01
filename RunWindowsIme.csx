// Windows TSF 輸入法概念驗證的一鍵啟動入口。
// 所有外部命令統一交給 Tsinswreng.CsSh，避免在腳本內手動管理進程。

#r "nuget: Tsinswreng.CsSh, 0.1.0-alpha"

using System;
using System.Threading;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

// 此版本的 dotnet-script 不支援頂層 using 宣告；腳本結束即釋放此短生命週期物件。
var CtSource = new CancellationTokenSource();
var Ct = CtSource.Token;

// 路徑只從腳本所在目錄推導，因此可從任意工作目錄執行。
var Root = CsxDir();
Cd(Root);

var TsfProject = Root / "AvlnImeDemo.Tsf/AvlnImeDemo.Tsf.csproj";
var DesktopProject = Root / "AvlnImeDemo.Desktop/AvlnImeDemo.Desktop.csproj";
var RegistrationProject = Root / "AvlnImeDemo.Tsf.Registration/AvlnImeDemo.Tsf.Registration.csproj";
var TsfComHost = Root / "AvlnImeDemo.Tsf/bin/Debug/net10.0-windows/AvlnImeDemo.Tsf.comhost.dll";
var DesktopAssembly = Root / "AvlnImeDemo.Desktop/bin/Debug/net10.0/AvlnImeDemo.Desktop.dll";
var RegistrationAssembly = Root / "AvlnImeDemo.Tsf.Registration/bin/Debug/net10.0-windows/AvlnImeDemo.Tsf.Registration.dll";

if (Args.Count == 1 && Args[0] == "--unregister") {
    // 目前使用者清單必須以未提升身分修改；系統層資料則在 UAC 中移除。
    await Exe("dotnet", ["build", RegistrationProject, "-c", "Debug"], Ct);
    await Exe("dotnet", [RegistrationAssembly, "--uninstall-user"], Ct);
    var UnregisterCommand = "$Process = Start-Process -FilePath 'dotnet' -ArgumentList @('" + RegistrationAssembly.ToString().Replace("'", "''") + "', '--unregister-system') -Verb RunAs -Wait -PassThru; exit $Process.ExitCode";
	var UnregisterCommandBytes = System.Text.Encoding.Unicode.GetBytes(UnregisterCommand);
	var UnregisterCommandBase64 = Convert.ToBase64String(UnregisterCommandBytes);
	var UnregisterExit = await TryExe("powershell", ["-NoProfile", "-EncodedCommand", UnregisterCommandBase64], Ct);
	if (UnregisterExit.IsSuccess) {
	await TryExe("dotnet", [DesktopAssembly, "--shutdown"], Ct);
		Console.WriteLine("已撤銷 Avln IME POC 的系統註冊，並已請求候選窗關閉。");
	}
	else {
		Console.WriteLine("反註冊失敗；請保留上方的具體錯誤資訊。");
		Environment.ExitCode = 1;
	}
	return;
}

if (Args.Count != 0) {
	Console.WriteLine("用法：dotnet script .\\RunWindowsIme.csx");
	Console.WriteLine("      dotnet script .\\RunWindowsIme.csx --unregister");
	Environment.ExitCode = 2;
	return;
}

// 先建置全部元件，避免以過期的 comhost 或候選窗啟動輸入法。
await Exe("dotnet", ["build", TsfProject, "-c", "Debug"], Ct);
await Exe("dotnet", ["build", DesktopProject, "-c", "Debug"], Ct);
await Exe("dotnet", ["build", RegistrationProject, "-c", "Debug"], Ct);

// 只有系統輸入法註冊需要提升權限；建置與候選窗仍以目前使用者身分執行。
var RegisterCommand = "$Process = Start-Process -FilePath 'dotnet' -ArgumentList @('" + RegistrationAssembly.ToString().Replace("'", "''") + "', '--register', '" + TsfComHost.ToString().Replace("'", "''") + "') -Verb RunAs -Wait -PassThru; exit $Process.ExitCode";
var RegisterCommandBytes = System.Text.Encoding.Unicode.GetBytes(RegisterCommand);
var RegisterCommandBase64 = Convert.ToBase64String(RegisterCommandBytes);
var RegistrationExit = await TryExe("powershell", ["-NoProfile", "-EncodedCommand", RegisterCommandBase64], Ct);
if (!RegistrationExit.IsSuccess) {
	Console.WriteLine("輸入法註冊失敗；候選窗未啟動。Windows 的 UAC 視窗若被取消也會得到此結果。");
	Environment.ExitCode = 1;
    return;
}

// InstallLayoutOrTip 必須在實際登入者身分執行，不能隨 UAC 提升到管理員帳戶。
await Exe("dotnet", [RegistrationAssembly, "--install-user"], Ct);

// 背景候選窗服務在啟動時保持隱藏；只有 TSF 真正送出 preedit 後才會顯示。
var CandidateWindow = Cmd("dotnet", [DesktopAssembly], Ct);
_ = CandidateWindow.Done;

Console.WriteLine("Avln IME POC 已安裝到目前使用者的輸入法列表。請切換到它後在記事本測試；候選窗只會在首次輸入 preedit 時出現。");
