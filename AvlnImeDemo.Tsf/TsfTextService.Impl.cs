namespace AvlnImeDemo.Tsf;

using System;
public sealed partial class TsfTextService {
    /// <summary>Activates the text service for one target application's input thread.</summary>
    public partial int Activate(ITfThreadMgr ThreadManager, uint ClientId) {
        this.ClientId = ClientId;
        KeystrokeManager = TsfCom.Query<ITfKeystrokeMgr>(ThreadManager);
        return KeystrokeManager.AdviseKeyEventSink(ClientId, this, true);
    }

    /// <summary>Releases TSF registrations and cancels visual state when the service is disabled.</summary>
    public partial int Deactivate() {
        var HResult = KeystrokeManager?.UnadviseKeyEventSink(ClientId) ?? HResults.Ok;
        KeystrokeManager = null;
        Composition = null;
        Preedit.Clear();
        TsfPipeClient.Hide();
        return HResult;
    }

    /// <summary>Handles focus loss for the active key sink.</summary>
    public partial int OnSetFocus(bool IsForeground) {
        if (!IsForeground) {
            // The target no longer safely owns this preedit, so discard its visual mirror.
            Composition = null;
            Preedit.Clear();
            TsfPipeClient.Hide();
        }
        return HResults.Ok;
    }

    /// <summary>Reports whether a key-down belongs to this POC.</summary>
    public partial int OnTestKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        IsEaten = IsSupportedKey((int)WParam);
        return HResults.Ok;
    }

    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnTestKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        IsEaten = false;
        return HResults.Ok;
    }

    /// <summary>Mutates the TSF composition in response to a consumed key-down.</summary>
    public partial int OnKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        var VirtualKey = (int)WParam;
        if (!TryApplyKey(VirtualKey)) {
            IsEaten = false;
            return HResults.Ok;
        }
        // Target-document mutations occur only under a TSF edit lock.
        var EditSession = new TsfCompositionEditSession(this, Context);
        var RequestResult = Context.RequestEditSession(ClientId, EditSession, TsfEditSessionFlags.Sync | TsfEditSessionFlags.ReadWrite, out var SessionResult);
        if (!HResults.Succeeded(RequestResult) || !HResults.Succeeded(SessionResult)) {
            Preedit.Clear();
            Composition = null;
            TsfPipeClient.Hide();
            IsEaten = false;
            return HResults.Ok;
        }
        IsEaten = true;
        PublishPreedit();
        return HResults.Ok;
    }

    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten) {
        IsEaten = false;
        return HResults.Ok;
    }

    /// <summary>Does not use TSF preserved keys in this POC.</summary>
    public partial int OnPreservedKey(ITfContext Context, in Guid PreservedKey, ref bool IsEaten) {
        IsEaten = false;
        return HResults.Ok;
    }

    /// <summary>Handles a host-initiated composition termination.</summary>
    public partial int OnCompositionTerminated(uint EditCookie, ITfComposition Composition) {
        this.Composition = null;
        Preedit.Clear();
        TsfPipeClient.Hide();
        return HResults.Ok;
    }

    /// <summary>Runs the operation selected by the key callback under TSF's document edit lock.</summary>
    internal partial int RunEditSession(ITfContext Context, uint EditCookie) {
        return PendingOperation switch {
            TsfCompositionOperation.Update => UpdateComposition(Context, EditCookie),
            TsfCompositionOperation.Commit => EndComposition(EditCookie, false),
            TsfCompositionOperation.Cancel => EndComposition(EditCookie, true),
            _ => HResults.Ok,
        };
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
            if (!HResults.Succeeded(SelectionResult) || Fetched != 1 || Selection.Range is null) {
                return SelectionResult;
            }
            var Composer = TsfCom.Query<ITfContextComposition>(Context);
            var StartResult = Composer.StartComposition(EditCookie, Selection.Range, this, out var NewComposition);
            if (!HResults.Succeeded(StartResult)) {
                return StartResult;
            }
            Composition = NewComposition;
        }
        var RangeResult = Composition.GetRange(out var Range);
        if (!HResults.Succeeded(RangeResult)) {
            return RangeResult;
        }
        // SetText replaces exactly the composition range, making backspace deterministic.
        var TextResult = Range.SetText(EditCookie, 0, Preedit.ToString(), Preedit.Length);
        if (!HResults.Succeeded(TextResult)) {
            return TextResult;
        }
        return Range.Collapse(EditCookie, TfAnchor.End);
    }

    /// <summary>Commits or removes the active composition, then releases its COM reference.</summary>
    private int EndComposition(uint EditCookie, bool ClearText) {
        if (Composition is null) {
            return HResults.Ok;
        }
        if (ClearText) {
            var RangeResult = Composition.GetRange(out var Range);
            if (!HResults.Succeeded(RangeResult)) {
                return RangeResult;
            }
            var ClearResult = Range.SetText(EditCookie, 0, string.Empty, 0);
            if (!HResults.Succeeded(ClearResult)) {
                return ClearResult;
            }
        }
        var Result = Composition.EndComposition(EditCookie);
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
