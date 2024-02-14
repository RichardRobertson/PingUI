using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using PingUI.Extensions;
using PingUI.Interop;
using PingUI.ServiceModels;
using PingUI.ViewModels;
using Splat;

namespace PingUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
	public MainWindow()
	{
		InitializeComponent();
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		var configuration = Locator.Current.GetRequiredService<IConfiguration>();
		if (configuration.WindowBounds is WindowPlacement.WindowPlacementRecord placement)
		{
			if (OperatingSystem.IsWindows() && TryGetPlatformHandle()?.Handle is nint handle)
			{
				User32.SetWindowPlacement(handle, placement);
			}
			else
			{
				Width = placement.NormalPosition.Right - placement.NormalPosition.Left;
				Height = placement.NormalPosition.Bottom - placement.NormalPosition.Top;
				Position = new PixelPoint(placement.NormalPosition.Left, placement.NormalPosition.Top);
			}
		}
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		base.OnClosing(e);
		var configuration = Locator.Current.GetRequiredService<IConfiguration>();
		if (configuration.WindowBounds is not null)
		{
			if (OperatingSystem.IsWindows() && TryGetPlatformHandle()?.Handle is nint handle)
			{
				var placement = new WindowPlacement();
				User32.GetWindowPlacement(handle, ref placement);
				configuration.WindowBounds = placement;
			}
			else
			{
				configuration.WindowBounds = new WindowPlacement.WindowPlacementRecord()
				{
					Flags = WindowPlacementFlags.None,
					ShowCmd = ShowCmd.ShowNormal,
					MinPosition = new Interop.Point.PointRecord(),
					MaxPosition = new Interop.Point.PointRecord(),
					NormalPosition = new Interop.Rect.RectRecord()
					{
						Left = Position.X,
						Top = Position.Y,
						Right = Position.X + (int)Width,
						Bottom = Position.Y + (int)Height,
					},
				};
			}
		}
	}

	private void CloseWindowButton_Click(object? sender, RoutedEventArgs e)
	{
		Close();
	}
}
