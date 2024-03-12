using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PingUI.Interop;

/// <summary>
/// Contains P/Invoke entries for user32.dll on Windows.
/// </summary>
[SupportedOSPlatform("Windows")]
internal static partial class User32
{
	/// <summary>
	/// Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
	/// </summary>
	/// <param name="hWnd">A handle to the window.</param>
	/// <param name="lpwndpl">A reference to the <see cref="WindowPlacement"/> structure that receives the show state and position information.</param>
	/// <returns><see langword="true" /> if the function succeeds; otherwise <see langword="false" />.</returns>
	[LibraryImport(nameof(User32))]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool GetWindowPlacement(nint hWnd, ref WindowPlacement lpwndpl);

	/// <summary>
	/// Sets the show state and the restored, minimized, and maximized positions of the specified window.
	/// </summary>
	/// <param name="hWnd">A handle to the window.</param>
	/// <param name="lpwndpl">A reference to a <see cref="WindowPlacement" /> structure that specifies the new show state and window positions.</param>
	/// <returns><see langword="true" /> if the function succeeds; otherwise <see langword="false" />.</returns>
	[LibraryImport(nameof(User32))]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool SetWindowPlacement(nint hWnd, in WindowPlacement lpwndpl);
}
