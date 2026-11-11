namespace AvlnImeDemo.Tsf;

internal sealed partial class TsfCompositionEditSession {
    /// <summary>Creates an edit-session callback for one service operation.</summary>
    internal partial TsfCompositionEditSession(TsfTextService Service, ITfContext Context) {
        this.Service = Service;
        this.Context = Context;
    }

    /// <summary>Executes the requested operation under the supplied TSF edit cookie.</summary>
    public partial int DoEditSession(uint EditCookie) {
        try {
            TsfLog.Write("DoEditSession enter cookie=" + EditCookie);
            return Service.RunEditSession(Context, EditCookie);
        }
        catch (System.Exception Exception) {
            TsfLog.Write("DoEditSession EX " + Exception);
            return unchecked((int)0x80004005);
        }
    }
}
