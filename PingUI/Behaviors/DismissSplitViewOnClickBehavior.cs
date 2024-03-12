using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;

namespace PingUI.Behaviors;

/// <summary>
/// Behavior to close a parent <see cref="SplitView" /> when a button is clicked.
/// </summary>
public class DismissSplitViewOnClickBehavior : Behavior
{
	/// <inheritdoc />
	protected override void OnAttached()
	{
		if (AssociatedObject is Button button)
		{
			button.AddHandler(Button.ClickEvent, OnButtonClick);
		}
	}

	/// <inheritdoc />
	protected override void OnDetaching()
	{
		if (AssociatedObject is Button button)
		{
			button.RemoveHandler(Button.ClickEvent, OnButtonClick);
		}
	}

	/// <summary>
	/// Raised when a user clicks the button.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The details of the event.</param>
	private void OnButtonClick(object? sender, RoutedEventArgs e)
	{
		if (sender is ILogical element && element.FindLogicalAncestorOfType<SplitView>() is { } splitView)
		{
			splitView.IsPaneOpen = false;
		}
	}
}
