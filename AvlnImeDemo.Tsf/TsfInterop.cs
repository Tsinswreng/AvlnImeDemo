// Windows Text Services Framework declarations used by the POC.
// Method order follows the Windows SDK msctf.h vtables exactly.
namespace AvlnImeDemo.Tsf;

using System;
using System.Runtime.InteropServices;

/// <summary>Stable identifiers shared by the text service and its registration tool.</summary>
internal static class TsfIds {
    /// <summary>COM class registered as the Windows text service.</summary>
    public static readonly Guid TextServiceClass = new("C6A66736-6E5E-4B4E-8B3A-6C2D2A9D4E11");

    /// <summary>Language profile shown by Windows input settings.</summary>
    public static readonly Guid LanguageProfile = new("C4E37B53-5BBE-4C29-9C94-2D80E997B031");

    /// <summary>Named pipe owned by the Avalonia candidate-window process.</summary>
    public const string CandidatePipeName = "AvlnImeDemo.CandidateWindow.v1";
}

/// <summary>HRESULT values used for success checks without exception-based COM control flow.</summary>
internal static class HResults {
    /// <summary>Successful COM call.</summary>
    public const int Ok = 0;

    /// <summary>Returns whether the HRESULT represents success.</summary>
    public static bool Succeeded(int Value) {
        return Value >= 0;
    }
}

/// <summary>Flags used when requesting a synchronous read/write TSF edit session.</summary>
internal static class TsfEditSessionFlags {
    /// <summary>TSF performs the edit session synchronously when possible.</summary>
    public const uint Sync = 0x00000001;

    /// <summary>The session needs both read and write access.</summary>
    public const uint ReadWrite = 0x00000006;
}

/// <summary>Native TSF text anchors; only collapse-to-end is required by this POC.</summary>
public enum TfAnchor : uint {
    /// <summary>Keep the range at its end after editing.</summary>
    End = 1,
}

/// <summary>Native TSF selection direction.</summary>
public enum TfActiveSelectionEnd : uint {
    /// <summary>The selection's active side is its end.</summary>
    End = 2,
}

/// <summary>Selection behavior supplied to <see cref="ITfContext.SetSelection"/>.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct TfSelectionStyle {
    /// <summary>Active side of the selection.</summary>
    public TfActiveSelectionEnd ActiveSelectionEnd;

    /// <summary>Whether TSF should treat the range as interim text.</summary>
    [MarshalAs(UnmanagedType.Bool)]
    public bool IsInterimCharacter;
}

/// <summary>One TSF selection range.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct TfSelection {
    /// <summary>The editable selection range.</summary>
    [MarshalAs(UnmanagedType.Interface)]
    public ITfRange? Range;

    /// <summary>Selection behavior flags.</summary>
    public TfSelectionStyle Style;
}

/// <summary>Opaque TSF thread manager passed to a text service activation callback.</summary>
[ComImport]
[Guid("AA80E801-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfThreadMgr {
}

/// <summary>TSF context; declarations include every method before the members used by the POC.</summary>
[ComImport]
[Guid("AA80E7FD-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfContext {
    /// <summary>Schedules an edit session under the target application's TSF lock.</summary>
    int RequestEditSession(uint ClientId, ITfEditSession EditSession, uint Flags, out int SessionResult);

    /// <summary>Reports whether this client owns the write lock.</summary>
    int InWriteSession(uint ClientId, [MarshalAs(UnmanagedType.Bool)] out bool IsWriteSession);

    /// <summary>Retrieves the current insertion selection.</summary>
    int GetSelection(uint EditCookie, uint Index, uint Count, out TfSelection Selection, out uint Fetched);

    /// <summary>Changes the target selection after a composition edit.</summary>
    int SetSelection(uint EditCookie, uint Count, in TfSelection Selection);
}

/// <summary>TSF key-event callback implemented by the text service.</summary>
[Guid("AA80E7F5-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfKeyEventSink {
    /// <summary>Notifies foreground focus changes.</summary>
    int OnSetFocus([MarshalAs(UnmanagedType.Bool)] bool IsForeground);

    /// <summary>Tests whether a key-down will be consumed.</summary>
    int OnTestKeyDown(ITfContext Context, nint WParam, nint LParam, [MarshalAs(UnmanagedType.Bool)] ref bool IsEaten);

    /// <summary>Tests whether a key-up will be consumed.</summary>
    int OnTestKeyUp(ITfContext Context, nint WParam, nint LParam, [MarshalAs(UnmanagedType.Bool)] ref bool IsEaten);

    /// <summary>Processes an owned key-down.</summary>
    int OnKeyDown(ITfContext Context, nint WParam, nint LParam, [MarshalAs(UnmanagedType.Bool)] ref bool IsEaten);

    /// <summary>Processes an owned key-up.</summary>
    int OnKeyUp(ITfContext Context, nint WParam, nint LParam, [MarshalAs(UnmanagedType.Bool)] ref bool IsEaten);

    /// <summary>Processes a registered preserved key.</summary>
    int OnPreservedKey(ITfContext Context, in Guid PreservedKey, [MarshalAs(UnmanagedType.Bool)] ref bool IsEaten);
}

/// <summary>TSF text-service lifecycle contract.</summary>
[Guid("AA80E7F7-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfTextInputProcessor {
    /// <summary>Activates the text service for a target process thread.</summary>
    int Activate(ITfThreadMgr ThreadManager, uint ClientId);

    /// <summary>Deactivates the text service.</summary>
    int Deactivate();
}

/// <summary>Keyboard manager queried from the thread manager COM object.</summary>
[ComImport]
[Guid("AA80E7F0-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfKeystrokeMgr {
    /// <summary>Registers a foreground key-event sink.</summary>
    int AdviseKeyEventSink(uint ClientId, ITfKeyEventSink Sink, [MarshalAs(UnmanagedType.Bool)] bool IsForeground);

    /// <summary>Removes the key-event sink for this service.</summary>
    int UnadviseKeyEventSink(uint ClientId);
}

/// <summary>Callback run by TSF only while it owns the appropriate document edit lock.</summary>
[Guid("AA80E803-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfEditSession {
    /// <summary>Applies one requested composition operation.</summary>
    int DoEditSession(uint EditCookie);
}

/// <summary>Composition range with every preceding vtable member retained before Collapse.</summary>
[ComImport]
[Guid("AA80E7FF-2021-11D2-93E0-0060B067B86E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfRange {
    /// <summary>Reads text.</summary>
    int GetText(uint EditCookie, uint Flags, nint Buffer, uint MaxCharacters, out uint CharactersRead);
    /// <summary>Replaces the composition contents.</summary>
    int SetText(uint EditCookie, uint Flags, [MarshalAs(UnmanagedType.LPWStr)] string Text, int CharacterCount);
    /// <summary>Gets formatted text.</summary>
    int GetFormattedText(uint EditCookie, out nint DataObject);
    /// <summary>Gets an embedded object.</summary>
    int GetEmbedded(uint EditCookie, in Guid Service, in Guid Interface, out nint Unknown);
    /// <summary>Inserts an embedded object.</summary>
    int InsertEmbedded(uint EditCookie, uint Flags, nint DataObject);
    /// <summary>Moves the range start.</summary>
    int ShiftStart(uint EditCookie, int RequestedCharacters, out int ShiftedCharacters, nint HaltCondition);
    /// <summary>Moves the range end.</summary>
    int ShiftEnd(uint EditCookie, int RequestedCharacters, out int ShiftedCharacters, nint HaltCondition);
    /// <summary>Aligns the range start with another range.</summary>
    int ShiftStartToRange(uint EditCookie, ITfRange Range, TfAnchor Position);
    /// <summary>Aligns the range end with another range.</summary>
    int ShiftEndToRange(uint EditCookie, ITfRange Range, TfAnchor Position);
    /// <summary>Moves range start through a TSF region.</summary>
    int ShiftStartRegion(uint EditCookie, uint Direction, [MarshalAs(UnmanagedType.Bool)] out bool NoRegion);
    /// <summary>Moves range end through a TSF region.</summary>
    int ShiftEndRegion(uint EditCookie, uint Direction, [MarshalAs(UnmanagedType.Bool)] out bool NoRegion);
    /// <summary>Checks whether the range is empty.</summary>
    int IsEmpty(uint EditCookie, [MarshalAs(UnmanagedType.Bool)] out bool IsEmpty);
    /// <summary>Places both range anchors at one side of the composition.</summary>
    int Collapse(uint EditCookie, TfAnchor Position);
}

/// <summary>Owns a TSF composition after it has been created in an edit session.</summary>
[ComImport]
[Guid("20168D64-5A8F-4A5A-B7BD-CFA29F4D0FD9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfComposition {
    /// <summary>Gets the live editable composition range. The edit cookie is part of the SDK signature.</summary>
    int GetRange(uint EditCookie, out ITfRange Range);
    /// <summary>Moves composition start.</summary>
    int ShiftStart(uint EditCookie, ITfRange Range);
    /// <summary>Moves composition end.</summary>
    int ShiftEnd(uint EditCookie, ITfRange Range);
    /// <summary>Ends the composition and leaves its current text committed in the target.</summary>
    int EndComposition(uint EditCookie);
}

/// <summary>Receives termination notifications when an application cancels the composition itself.</summary>
[Guid("A781718C-579A-4B15-A280-32B8577ACC5E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfCompositionSink {
    /// <summary>Clears service state after composition termination.</summary>
    int OnCompositionTerminated(uint EditCookie, ITfComposition Composition);
}

/// <summary>Creates a composition from the target application's current selection.</summary>
[ComImport]
[Guid("D40C8AAE-AC92-4FC7-9A11-0EE0E23AA39B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface ITfContextComposition {
    /// <summary>Begins a composition over the supplied selection range.</summary>
    int StartComposition(uint EditCookie, ITfRange Range, ITfCompositionSink Sink, out ITfComposition Composition);
}
