namespace AvlnImeDemo.Tsf;

using System;

public sealed partial class TsfTextService {
    /// <summary>任何例外都不得跨越 COM 邊界回到原生端，否則宿主行程會直接崩潰。</summary>
    private const int EFail = unchecked((int)0x80004005);

    /// <summary>Activates the text service for one target application's input thread.</summary>
    public partial int Activate(ITfThreadMgr ThreadManager, uint ClientId) {
        try {
            TsfLog.Write("Activate enter clientId=" + ClientId);
            this.ClientId = ClientId;
            KeystrokeManager = TsfCom.Query<ITfKeystrokeMgr>(ThreadManager);
            TsfLog.Write("Activate queried ITfKeystrokeMgr");
            var Result = KeystrokeManager.AdviseKeyEventSink(ClientId, this, true);
            TsfLog.Write("Activate AdviseKeyEventSink hr=0x" + Result.ToString("X8"));
            return Result;
        }
        catch (Exception Exception) {
            TsfLog.Write("Activate EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Releases TSF registrations and cancels visual state when the service is disabled.</summary>
    public partial int Deactivate() {
        try {
            TsfLog.Write("Deactivate enter");
            var HResult = KeystrokeManager?.UnadviseKeyEventSink(ClientId) ?? HResults.Ok;
            KeystrokeManager = null;
            Composition = null;
            Preedit.Clear();
            TsfPipeClient.Hide();
            TsfLog.Write("Deactivate done hr=0x" + HResult.ToString("X8"));
            return HResult;
        }
        catch (Exception Exception) {
            TsfLog.Write("Deactivate EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Handles focus loss for the active key sink.</summary>
    public partial int OnSetFocus(bool IsForeground) {
        try {
            TsfLog.Write("OnSetFocus foreground=" + IsForeground);
            if (!IsForeground) {
                // The target no longer safely owns this preedit, so discard its visual mirror.
                Composition = null;
                Preedit.Clear();
                TsfPipeClient.Hide();
            }
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnSetFocus EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Reports whether a key-down belongs to this POC.</summary>
    public partial int OnTestKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        try {
            var VirtualKey = (int)WParam;
            var Supported = IsSupportedKey(VirtualKey);
            if (Supported) {
                TsfLog.Write("OnTestKeyDown vk=0x" + VirtualKey.ToString("X2") + " eat=true");
            }
            IsEaten = Supported;
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnTestKeyDown EX " + Exception);
            IsEaten = false;
            return EFail;
        }
    }

    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnTestKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        try {
            IsEaten = false;
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnTestKeyUp EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Mutates the TSF composition in response to a consumed key-down.</summary>
    public partial int OnKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        try {
            var VirtualKey = (int)WParam;
            TsfLog.Write("OnKeyDown vk=0x" + VirtualKey.ToString("X2"));
            if (!TryApplyKey(VirtualKey)) {
                IsEaten = false;
                TsfLog.Write("OnKeyDown not-applied");
                return HResults.Ok;
            }
            TsfLog.Write("OnKeyDown op=" + PendingOperation + " preedit=[" + Preedit + "]");
            // Target-document mutations occur only under a TSF edit lock.
            var EditSession = new TsfCompositionEditSession(this, Context);
            var RequestResult = Context.RequestEditSession(ClientId, EditSession, TsfEditSessionFlags.Sync | TsfEditSessionFlags.ReadWrite, out var SessionResult);
            TsfLog.Write("OnKeyDown RequestEditSession hr=0x" + RequestResult.ToString("X8") + " sessionHr=0x" + SessionResult.ToString("X8"));
            if (!HResults.Succeeded(RequestResult) || !HResults.Succeeded(SessionResult)) {
                Preedit.Clear();
                Composition = null;
                TsfPipeClient.Hide();
                IsEaten = false;
                return HResults.Ok;
            }
            IsEaten = true;
            PublishPreedit();
            TsfLog.Write("OnKeyDown done eaten=true");
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnKeyDown EX " + Exception);
            IsEaten = false;
            return EFail;
        }
    }

    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        try {
            IsEaten = false;
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnKeyUp EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Does not use TSF preserved keys in this POC.</summary>
    public partial int OnPreservedKey(ITfContext Context, in Guid PreservedKey, ref bool IsEaten) {
        try {
            IsEaten = false;
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnPreservedKey EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Handles a host-initiated composition termination.</summary>
    public partial int OnCompositionTerminated(uint EditCookie, ITfComposition Composition) {
        try {
            TsfLog.Write("OnCompositionTerminated cookie=" + EditCookie);
            this.Composition = null;
            Preedit.Clear();
            TsfPipeClient.Hide();
            return HResults.Ok;
        }
        catch (Exception Exception) {
            TsfLog.Write("OnCompositionTerminated EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Runs the operation selected by the key callback under TSF's document edit lock.</summary>
    internal partial int RunEditSession(ITfContext Context, uint EditCookie) {
        try {
            TsfLog.Write("RunEditSession enter cookie=" + EditCookie + " op=" + PendingOperation);
            var Result = PendingOperation switch {
                TsfCompositionOperation.Update => UpdateComposition(Context, EditCookie),
                TsfCompositionOperation.Commit => EndComposition(EditCookie, false),
                TsfCompositionOperation.Cancel => EndComposition(EditCookie, true),
                _ => HResults.Ok,
            };
            TsfLog.Write("RunEditSession done hr=0x" + Result.ToString("X8"));
            return Result;
        }
        catch (Exception Exception) {
            TsfLog.Write("RunEditSession EX " + Exception);
            return EFail;
        }
    }

    /// <summary>Updates local input state and decides which edit-session operation is needed.</summary>
    private bool TryApplyKey(int VirtualKey) {
        if (VirtualKey == VkSpace) {
            if (Preedit.Length == 0) {
                return false;
            }
            PendingOperation = TsfCompositionOperation.Commit;
            return true;
        }
        if (VirtualKey == VkEscape) {
            if (Preedit.Length == 0) {
                return false;
            }
            PendingOperation = TsfCompositionOperation.Cancel;
            return true;
        }
        if (VirtualKey == VkBackspace) {
            if (Preedit.Length == 0) {
                return false;
            }
            Preedit.Remove(Preedit.Length - 1, 1);
            PendingOperation = TsfCompositionOperation.Update;
            return true;
        }
        var Text = TryGetPrintableText(VirtualKey);
        if (Text is null) {
            return false;
        }
        Preedit.Append(Text);
        PendingOperation = TsfCompositionOperation.Update;
        return true;
    }

    /// <summary>Creates the first composition or replaces text in the existing one.</summary>
    private int UpdateComposition(ITfContext Context, uint EditCookie) {
        if (Composition is null) {
            var SelectionResult = Context.GetSelection(EditCookie, 0, 1, out var Selection, out var Fetched);
            TsfLog.Write("UpdateComposition GetSelection hr=0x" + SelectionResult.ToString("X8") + " fetched=" + Fetched);
            if (!HResults.Succeeded(SelectionResult) || Fetched != 1 || Selection.Range is null) {
                return SelectionResult;
            }
            var Composer = TsfCom.Query<ITfContextComposition>(Context);
            var StartResult = Composer.StartComposition(EditCookie, Selection.Range, this, out var NewComposition);
            TsfLog.Write("UpdateComposition StartComposition hr=0x" + StartResult.ToString("X8"));
            if (!HResults.Succeeded(StartResult)) {
                return StartResult;
            }
            Composition = NewComposition;
        }
        var RangeResult = Composition.GetRange(EditCookie, out var Range);
        TsfLog.Write("UpdateComposition GetRange hr=0x" + RangeResult.ToString("X8"));
        if (!HResults.Succeeded(RangeResult)) {
            return RangeResult;
        }
        // SetText replaces exactly the composition range, making backspace deterministic.
        var TextResult = Range.SetText(EditCookie, 0, Preedit.ToString(), Preedit.Length);
        TsfLog.Write("UpdateComposition SetText hr=0x" + TextResult.ToString("X8"));
        if (!HResults.Succeeded(TextResult)) {
            return TextResult;
        }
        var CollapseResult = Range.Collapse(EditCookie, TfAnchor.End);
        TsfLog.Write("UpdateComposition Collapse hr=0x" + CollapseResult.ToString("X8"));
        return CollapseResult;
    }

    /// <summary>Commits or removes the active composition, then releases its COM reference.</summary>
    private int EndComposition(uint EditCookie, bool ClearText) {
        if (Composition is null) {
            return HResults.Ok;
        }
        if (ClearText) {
            var RangeResult = Composition.GetRange(EditCookie, out var Range);
            TsfLog.Write("EndComposition GetRange hr=0x" + RangeResult.ToString("X8"));
            if (!HResults.Succeeded(RangeResult)) {
                return RangeResult;
            }
            var ClearResult = Range.SetText(EditCookie, 0, string.Empty, 0);
            TsfLog.Write("EndComposition SetText hr=0x" + ClearResult.ToString("X8"));
            if (!HResults.Succeeded(ClearResult)) {
                return ClearResult;
            }
        }
        var Result = Composition.EndComposition(EditCookie);
        TsfLog.Write("EndComposition EndComposition hr=0x" + Result.ToString("X8"));
        if (HResults.Succeeded(Result)) {
            Composition = null;
            Preedit.Clear();
        }
        return Result;
    }

    /// <summary>Mirrors the successful TSF composition in the independent Avalonia candidate window.</summary>
    private void PublishPreedit() {
        if (Preedit.Length == 0) {
            TsfPipeClient.Hide();
            return;
        }
        TsfPipeClient.Show(Preedit.ToString());
    }

    /// <summary>Restricts the POC to alphanumeric virtual keys and three editing keys.</summary>
    private static bool IsSupportedKey(int VirtualKey) {
        return VirtualKey is >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A or VkSpace or VkBackspace or VkEscape;
    }

    /// <summary>Maps the deliberately limited first POC keyboard surface to preedit text.</summary>
    private static string? TryGetPrintableText(int VirtualKey) {
        if (VirtualKey is >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A) {
            return ((char)VirtualKey).ToString().ToLowerInvariant();
        }
        return null;
    }
}
