# Windows 输入法 POC

这是一个用 C# 编写的真实 Windows TSF 输入法概念验证。TSF 服务被加载到聚焦的目标程序中，负责持有 `ITfComposition`；Avalonia 桌面程序只显示不抢焦点的候选窗。

## 启动

项目根目录内只需要执行这一条命令：

```powershell
dotnet script .\RunWindowsIme.csx
```

脚本会自动构建 TSF 服务、Avalonia 候选窗和注册工具。Windows 会显示一次 UAC 提示，仅用于写入系统级 COM、TSF profile 和键盘分类；随后脚本以当前登录用户身份调用 Windows 的 `InstallLayoutOrTip`，把 `Avln IME POC` 加入简体中文输入法列表。

候选窗服务会在后台启动，但初始完全隐藏，不抢焦点也不会显示空白框。只有在 Windows 实际加载本输入法、TSF 收到按键并创建 preedit 后，候选窗才会显示。

脚本完成后，用 `Win+Space` 或任务栏输入法切换器选择 `Avln IME POC`，再在记事本测试。没有切换到它之前，按键仍由原输入法处理，这是正常行为。

卸载 POC：

```powershell
dotnet script .\RunWindowsIme.csx --unregister
```

脚本先以当前用户身份移除输入法列表项，再请求 UAC 提升权限以移除系统注册，并自动关闭正在运行的候选窗。

## 当前行为

- 按 `A-Z`、`0-9`：创建或更新组合文本，候选窗显示小写 preedit。
- 按 `Backspace`：删除一个 preedit 字符。
- 按 `Escape`：取消组合，不上屏。
- 按 `Space`：结束组合，把当前文本上屏到外部程序的聚焦输入框。

## POC 边界

当前只支持 x64 目标程序；尚未接 Rime、候选选择、真实键盘布局转换和候选窗跟随光标。这些是后续功能，不影响本次验证“外部程序输入、组合和上屏”的链路。
