namespace AvlnImeDemo.Tsf;

using System.Runtime.InteropServices;

internal static partial class TsfCom {
    /// <summary>Queries a strongly typed TSF interface without assuming CLR interface casts perform COM QI.</summary>
    internal static partial T Query<T>(object ComObject) where T : class {
        var InterfaceId = typeof(T).GUID;
        var Unknown = Marshal.GetIUnknownForObject(ComObject);
        try {
            var HResult = Marshal.QueryInterface(Unknown, ref InterfaceId, out var InterfacePointer);
            Marshal.ThrowExceptionForHR(HResult);
            try {
                return (T)Marshal.GetObjectForIUnknown(InterfacePointer);
            }
            finally {
                Marshal.Release(InterfacePointer);
            }
        }
        finally {
            Marshal.Release(Unknown);
        }
    }
}
