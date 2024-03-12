using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;

namespace PingUI.Behaviors;

public class DismissSplitViewOnClickBehavior : Behavior
{
	protected override void OnAttached()
	{
		if (AssociatedObject is Button button)
		{
			button.AddHandler(Button.ClickEvent, OnButtonClick);
		}
	}

	protected override void OnDetaching()
	{
		if (AssociatedObject is Button button)
		{
			button.RemoveHandler(Button.ClickEvent, OnButtonClick);
		}
	}

	private void OnButtonClick(object? sender, RoutedEventArgs e)
	{
		if (sender is ILogical element && element.FindLogicalAncestorOfType<SplitView>() is { } splitView)
		{
			splitView.IsPaneOpen = false;
		}
	}
}
