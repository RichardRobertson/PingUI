using System;
using System.Runtime.InteropServices;

namespace PingUI.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct WindowPlacement
{
	public WindowPlacement()
	{
		Length = Marshal.SizeOf<WindowPlacement>();
	}

	public readonly int Length;

	public WindowPlacementFlags Flags;

	public ShowCmd ShowCmd;

	public Point MinPosition;

	public Point MaxPosition;

	public Rect NormalPosition;

	public record WindowPlacementRecord
	{
		public WindowPlacementFlags Flags
		{
			init;
			get;
		}

		public ShowCmd ShowCmd
		{
			init;
			get;
		}

		public required Point.PointRecord MinPosition
		{
			init;
			get;
		}

		public required Point.PointRecord MaxPosition
		{
			init;
			get;
		}

		public required Rect.RectRecord NormalPosition
		{
			init;
			get;
		}

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

[Flags]
public enum WindowPlacementFlags
{
	None = 0,
	SetMinPosition = 1,
	RestoreToMaximized = 1 << 1,
	AsyncWindowPlacement = 1 << 2,
}

public enum ShowCmd
{
	Hide = 0,
	ShowNormal = 1,
	Normal = ShowNormal,
	ShowMinimized = 2,
	ShowMaximized = 3,
	Maximize = ShowMaximized,
	ShowNoActivate = 4,
	Show = 5,
	Minimize = 6,
	ShowMinNoActive = 7,
	ShowNA = 8,
	Restore = 9,
	ShowDefault = 10,
	ForceMinimize = 11,
}

[StructLayout(LayoutKind.Sequential)]
public struct Point
{
	public int X;

	public int Y;

	public record PointRecord
	{
		public int X
		{
			init;
			get;
		}

		public int Y
		{
			init;
			get;
		}

		public static implicit operator Point(PointRecord value)
		{
			return new()
			{
				X = value.X,
				Y = value.Y,
			};
		}

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

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
	public int Left;

	public int Top;

	public int Right;

	public int Bottom;

	public record RectRecord
	{
		public int Left
		{
			init;
			get;
		}

		public int Top
		{
			init;
			get;
		}

		public int Right
		{
			init;
			get;
		}

		public int Bottom
		{
			init;
			get;
		}

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
