namespace AvlnImeDemo.Tsf;

/// <summary>Minimal COM QueryInterface helper for TSF objects supplied by Windows.</summary>
internal static partial class TsfCom {
    /// <summary>Queries a strongly typed TSF interface without assuming CLR interface casts perform COM QI.</summary>
    internal static partial T Query<T>(object ComObject) where T : class;
}
