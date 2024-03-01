using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PingUI.Behaviors;

public class DismissSplitViewOnClick : AvaloniaObject
{
	static DismissSplitViewOnClick()
	{
		SplitViewProperty.Changed.AddClassHandler<Button>(OnSplitViewChanged);
	}

	public static readonly AttachedProperty<SplitView?> SplitViewProperty = AvaloniaProperty.RegisterAttached<DismissSplitViewOnClick, Button, SplitView?>("SplitView");

	public static SplitView? GetSplitView(AvaloniaObject element)
	{
		return element.GetValue(SplitViewProperty);
	}

	public static void SetSplitView(AvaloniaObject element, SplitView? splitView)
	{
		element.SetValue(SplitViewProperty, splitView);
	}

	private static void OnSplitViewChanged(Button button, AvaloniaPropertyChangedEventArgs args)
	{
		if (args.GetNewValue<SplitView?>() is null)
		{
			button.Click -= OnButtonClick;
		}
		else
		{
			button.Click += OnButtonClick;
		}
	}

	private static void OnButtonClick(object? sender, RoutedEventArgs e)
	{
		if (sender is Button button && GetSplitView(button) is { } splitView)
		{
			splitView.IsPaneOpen = false;
		}
	}
}
