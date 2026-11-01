namespace AvlnImeDemo.Tsf;

using System.Runtime.InteropServices;

/// <summary>Short-lived callback that applies one operation while the target document is locked.</summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed partial class TsfCompositionEditSession : ITfEditSession {
    /// <summary>Text service containing the operation and composition state.</summary>
    private readonly TsfTextService Service;
    /// <summary>Context whose edit lock caused this callback.</summary>
    private readonly ITfContext Context;
    /// <summary>Creates an edit-session callback for one service operation.</summary>
    internal partial TsfCompositionEditSession(TsfTextService Service, ITfContext Context);
    /// <summary>Executes the requested operation under the supplied TSF edit cookie.</summary>
    public partial int DoEditSession(uint EditCookie);
}
