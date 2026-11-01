namespace AvlnImeDemo.Tsf;

using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Windows TSF text service for the first desktop proof of concept.
/// It owns a genuine TSF composition; Avalonia only renders a mirrored preedit.
/// </summary>
[ComVisible(true)]
[Guid("C6A66736-6E5E-4B4E-8B3A-6C2D2A9D4E11")]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AvlnImeDemo.Tsf.TextService")]
public sealed partial class TsfTextService : ITfTextInputProcessor, ITfKeyEventSink, ITfCompositionSink {
    /// <summary>Windows virtual-key code for space.</summary>
    private const int VkSpace = 0x20;
    /// <summary>Windows virtual-key code for backspace.</summary>
    private const int VkBackspace = 0x08;
    /// <summary>Windows virtual-key code for Escape.</summary>
    private const int VkEscape = 0x1B;
    /// <summary>TSF client identifier for this service in the current target process thread.</summary>
    private uint ClientId;
    /// <summary>Keyboard manager used solely to register and unregister this service as a key sink.</summary>
    private ITfKeystrokeMgr? KeystrokeManager;
    /// <summary>Current uncommitted text, independent of the target application's composition object.</summary>
    private readonly StringBuilder Preedit = new();
    /// <summary>Live TSF composition; null until the first edit session creates it.</summary>
    private ITfComposition? Composition;
    /// <summary>Latest action to apply during the requested TSF edit session.</summary>
    private TsfCompositionOperation PendingOperation;

    /// <summary>Activates the text service for one target application's input thread.</summary>
    public partial int Activate(ITfThreadMgr ThreadManager, uint ClientId);
    /// <summary>Releases TSF registrations and cancels visual state when the service is disabled.</summary>
    public partial int Deactivate();
    /// <summary>Handles focus loss for the active key sink.</summary>
    public partial int OnSetFocus(bool IsForeground);
    /// <summary>Reports whether a key-down belongs to this POC.</summary>
    public partial int OnTestKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten);
    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnTestKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten);
    /// <summary>Mutates the TSF composition in response to a consumed key-down.</summary>
    public partial int OnKeyDown(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten);
    /// <summary>Leaves key-up events to the target application.</summary>
    public partial int OnKeyUp(ITfContext Context, nint WParam, nint LParam, ref bool IsEaten);
    /// <summary>Does not use TSF preserved keys in this POC.</summary>
    public partial int OnPreservedKey(ITfContext Context, in Guid PreservedKey, ref bool IsEaten);
    /// <summary>Handles a host-initiated composition termination.</summary>
    public partial int OnCompositionTerminated(uint EditCookie, ITfComposition Composition);
    /// <summary>Runs the operation selected by the key callback under TSF's document edit lock.</summary>
    internal partial int RunEditSession(ITfContext Context, uint EditCookie);
}

/// <summary>Mutations that must happen inside a TSF edit session.</summary>
internal enum TsfCompositionOperation {
    /// <summary>Creates or updates preedit text.</summary>
    Update,
    /// <summary>Finishes the composition and retains text in the focused target.</summary>
    Commit,
    /// <summary>Removes the composition text before ending it.</summary>
    Cancel,
}
