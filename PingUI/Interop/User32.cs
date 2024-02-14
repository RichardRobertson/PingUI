using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PingUI.Interop;

[SupportedOSPlatform("Windows")]
internal static partial class User32
{
	[LibraryImport(nameof(User32))]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetWindowPlacement(nint hWnd, ref WindowPlacement lpwndpl);

	[LibraryImport(nameof(User32))]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool SetWindowPlacement(nint hWnd, in WindowPlacement lpwndpl);
}
