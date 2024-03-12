using System;
using System.Runtime.InteropServices;

namespace PingUI.Interop;

/// <summary>
/// Contains information about the placement of a window on the screen.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WindowPlacement
{
	/// <summary>
	/// Initializes a new <see cref="WindowPlacement" /> structure.
	/// </summary>
	public WindowPlacement()
	{
		Length = Marshal.SizeOf<WindowPlacement>();
	}

	/// <summary>
	/// The length of the structure, in bytes.
	/// </summary>
	public readonly int Length;

	/// <summary>
	/// The flags that control the position of the minimized window and the method by which the window is restored.
	/// </summary>
	public WindowPlacementFlags Flags;

	/// <summary>
	/// The current show state of the window. It can be any of the values that can be specified in the nCmdShow parameter for the ShowWindow function.
	/// </summary>
	public ShowCmd ShowCmd;

	/// <summary>
	/// The coordinates of the window's upper-left corner when the window is minimized.
	/// </summary>
	public Point MinPosition;

	/// <summary>
	/// The coordinates of the window's upper-left corner when the window is maximized.
	/// </summary>
	public Point MaxPosition;

	/// <summary>
	/// The window's coordinates when the window is in the restored position.
	/// </summary>
	public Rect NormalPosition;

	/// <summary>
	/// An immutable record type to represent a <see cref="WindowPlacement" /> in JSON.
	/// </summary>
	public record WindowPlacementRecord
	{
		/// <summary>
		/// Represents the <see cref="WindowPlacement.Flags" /> field.
		/// </summary>
		public WindowPlacementFlags Flags
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="WindowPlacement.ShowCmd" /> field.
		/// </summary>
		public ShowCmd ShowCmd
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="WindowPlacement.MinPosition" /> field.
		/// </summary>
		public required Point.PointRecord MinPosition
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="WindowPlacement.MaxPosition" /> field.
		/// </summary>
		public required Point.PointRecord MaxPosition
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="WindowPlacement.NormalPosition" /> field.
		/// </summary>
		public required Rect.RectRecord NormalPosition
		{
			init;
			get;
		}

		/// <summary>
		/// Converts a <see cref="WindowPlacementRecord" /> object to a <see cref="WindowPlacement" /> struct.
		/// </summary>
		/// <param name="value">The record to convert.</param>
		public static implicit operator WindowPlacement(WindowPlacementRecord value)
		{
			return new()
			{
				Flags = value.Flags,
				ShowCmd = value.ShowCmd,
				MinPosition = value.MinPosition,
				MaxPosition = value.MaxPosition,
				NormalPosition = value.NormalPosition,
			};
		}

		/// <summary>
		/// Converts a <see cref="WindowPlacement" /> struct to a <see cref="WindowPlacementRecord" /> object.
		/// </summary>
		/// <param name="value">The struct to convert.</param>
		public static implicit operator WindowPlacementRecord(WindowPlacement value)
		{
			return new()
			{
				Flags = value.Flags,
				ShowCmd = value.ShowCmd,
				MinPosition = value.MinPosition,
				MaxPosition = value.MaxPosition,
				NormalPosition = value.NormalPosition,
			};
		}
	}
}

/// <summary>
/// The flags that control the position of the minimized window and the method by which the window is restored.
/// </summary>
[Flags]
public enum WindowPlacementFlags
{
	/// <summary>
	/// Default value.
	/// </summary>
	None = 0,

	/// <summary>
	/// <para>The coordinates of the minimized window may be specified.</para>
	/// <para>This flag must be specified if the coordinates are set in the ptMinPosition member.</para>
	/// </summary>
	SetMinPosition = 1,

	/// <summary>
	/// <para>The restored window will be maximized, regardless of whether it was maximized before it was minimized. This setting is only valid the next time the window is restored. It does not change the default restoration behavior.</para>
	/// <para>This flag is only valid when the <see cref="ShowCmd.ShowMinimized"/> value is specified for the <see cref="WindowPlacement.ShowCmd"/> member.</para>
	/// </summary>
	RestoreToMaximized = 1 << 1,

	/// <summary>
	/// If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
	/// </summary>
	AsyncWindowPlacement = 1 << 2,
}

/// <summary>
/// Controls how the window is to be shown.
/// </summary>
public enum ShowCmd
{
	/// <summary>
	/// Hides the window and activates another window.
	/// </summary>
	Hide = 0,

	/// <summary>
	/// Activates and displays a window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
	/// </summary>
	ShowNormal = 1,

	/// <inheritdoc cref="ShowNormal" />
	Normal = ShowNormal,

	/// <summary>
	/// Activates the window and displays it as a minimized window.
	/// </summary>
	ShowMinimized = 2,

	/// <summary>
	/// Activates the window and displays it as a maximized window.
	/// </summary>
	ShowMaximized = 3,

	/// <inheritdoc cref="ShowMaximized" />
	Maximize = ShowMaximized,

	/// <summary>
	/// Displays a window in its most recent size and position. This value is similar to <see cref="ShowNormal" />, except that the window is not activated.
	/// </summary>
	ShowNoActivate = 4,

	/// <summary>
	/// Activates the window and displays it in its current size and position.
	/// </summary>
	Show = 5,

	/// <summary>
	/// Minimizes the specified window and activates the next top-level window in the Z order.
	/// </summary>
	Minimize = 6,

	/// <summary>
	/// Displays the window as a minimized window. This value is similar to <see cref="ShowMinimized" />, except the window is not activated.
	/// </summary>
	ShowMinNoActive = 7,

	/// <summary>
	/// Displays the window in its current size and position. This value is similar to <see cref="Show" />, except that the window is not activated.
	/// </summary>
	ShowNA = 8,

	/// <summary>
	/// Activates and displays the window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
	/// </summary>
	Restore = 9,

	/// <summary>
	/// Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.
	/// </summary>
	ShowDefault = 10,

	/// <summary>
	/// Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
	/// </summary>
	ForceMinimize = 11,
}

/// <summary>
/// The <see cref="Point" /> structure defines the x- and y-coordinates of a point.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Point
{
	/// <summary>
	/// Specifies the x-coordinate of the point.
	/// </summary>
	public int X;

	/// <summary>
	/// Specifies the y-coordinate of the point.
	/// </summary>
	public int Y;

	/// <summary>
	/// An immutable record type to represent a <see cref="Point" /> in JSON.
	/// </summary>
	public record PointRecord
	{
		/// <summary>
		/// Represents the <see cref="Point.X" /> field.
		/// </summary>
		public int X
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="Point.Y" /> field.
		/// </summary>
		public int Y
		{
			init;
			get;
		}

		/// <summary>
		/// Converts a <see cref="PointRecord" /> object to a <see cref="Point" /> struct.
		/// </summary>
		/// <param name="value">The record to convert.</param>
		public static implicit operator Point(PointRecord value)
		{
			return new()
			{
				X = value.X,
				Y = value.Y,
			};
		}

		/// <summary>
		/// Converts a <see cref="Point" /> struct to a <see cref="PointRecord" /> object.
		/// </summary>
		/// <param name="value">The struct to convert.</param>
		public static implicit operator PointRecord(Point value)
		{
			return new()
			{
				X = value.X,
				Y = value.Y,
			};
		}
	}
}

/// <summary>
/// The <see cref="Rect" /> structure defines a rectangle by the coordinates of its upper-left and lower-right corners.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
	/// <summary>
	/// Specifies the x-coordinate of the upper-left corner of the rectangle.
	/// </summary>
	public int Left;

	/// <summary>
	/// Specifies the y-coordinate of the upper-left corner of the rectangle.
	/// </summary>
	public int Top;

	/// <summary>
	/// Specifies the x-coordinate of the lower-right corner of the rectangle.
	/// </summary>
	public int Right;

	/// <summary>
	/// Specifies the y-coordinate of the lower-right corner of the rectangle.
	/// </summary>
	public int Bottom;

	/// <summary>
	/// An immutable record type to represent a <see cref="Rect" /> in JSON.
	/// </summary>
	public record RectRecord
	{
		/// <summary>
		/// Represents the <see cref="Rect.Left" /> field.
		/// </summary>
		public int Left
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="Rect.Top" /> field.
		/// </summary>
		public int Top
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="Rect.Right" /> field.
		/// </summary>
		public int Right
		{
			init;
			get;
		}

		/// <summary>
		/// Represents the <see cref="Rect.Bottom" /> field.
		/// </summary>
		public int Bottom
		{
			init;
			get;
		}

		/// <summary>
		/// Converts a <see cref="RectRecord" /> object to a <see cref="Rect" /> struct.
		/// </summary>
		/// <param name="value">The record to convert.</param>
		public static implicit operator Rect(RectRecord value)
		{
			return new()
			{
				Left = value.Left,
				Top = value.Top,
				Right = value.Right,
				Bottom = value.Bottom,
			};
		}

		/// <summary>
		/// Converts a <see cref="Rect" /> struct to a <see cref="RectRecord" /> object.
		/// </summary>
		/// <param name="value">The struct to convert.</param>
		public static implicit operator RectRecord(Rect value)
		{
			return new()
			{
				Left = value.Left,
				Top = value.Top,
				Right = value.Right,
				Bottom = value.Bottom,
			};
		}
	}
}
