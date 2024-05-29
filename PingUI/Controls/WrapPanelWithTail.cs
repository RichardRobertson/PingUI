using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Utilities;

namespace PingUI.Controls;

public class WrapPanelWithTail : Panel, INavigableContainer
{
	public static readonly StyledProperty<Control?> TailProperty = AvaloniaProperty.Register<WrapPanelWithTail, Control?>(nameof(Tail));

	public Control? Tail
	{
		get => GetValue(TailProperty);
		set => SetValue(TailProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property == TailProperty)
		{
			if (change.OldValue is Control oldValue)
			{
				LogicalChildren.Remove(oldValue);
				VisualChildren.Remove(oldValue);
			}
			if (change.NewValue is Control newValue)
			{
				LogicalChildren.Add(newValue);
				VisualChildren.Add(newValue);
			}
		}
	}

	IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
	{
		var children = (Tail is null ? Children : Children.Append(Tail)).ToArray();
		var index = from is Control fromControl ? Array.IndexOf(children, fromControl) : -1;
		index = direction switch
		{
			NavigationDirection.First => 0,
			NavigationDirection.Last => children.Length - 1,
			NavigationDirection.Next => index + 1,
			NavigationDirection.Previous => index - 1,
			NavigationDirection.Left => index - 1,
			NavigationDirection.Right => index + 1,
			_ => -1,
		};
		return 0 <= index && index < children.Length ? children[index] : null;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		var children = Tail is null ? Children : Children.Append(Tail);
		var currentLineSize = new Size();
		var panelSize = new Size();
		foreach (var child in children)
		{
			child.Measure(availableSize);
			var sz = child.DesiredSize;
			if (MathUtilities.GreaterThan(currentLineSize.Width + sz.Width, availableSize.Width))
			{
				panelSize = new Size(double.Max(currentLineSize.Width, panelSize.Width), panelSize.Height + currentLineSize.Height);
				currentLineSize = sz;
				if (MathUtilities.GreaterThan(sz.Width, availableSize.Width))
				{
					panelSize = new Size(double.Max(sz.Width, panelSize.Width), panelSize.Height + sz.Height);
					currentLineSize = new Size();
				}
			}
			else
			{
				currentLineSize = new Size(currentLineSize.Width + sz.Width, double.Max(sz.Height, currentLineSize.Height));
			}
		}
		return new Size(double.Max(currentLineSize.Width, panelSize.Width), panelSize.Height + currentLineSize.Height);
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		var children = Tail is null ? Children : Children.Append(Tail);
		var childrenCount = Tail is null ? Children.Count : Children.Count + 1;
		var firstInLine = 0;
		var accumulatedHeight = 0.0;
		var currentLineSize = new Size();
		foreach (var (child, i) in children.Select((child, i) => (child, i)))
		{
			var sz = new Size(child.DesiredSize.Width, child.DesiredSize.Height);
			if (MathUtilities.GreaterThan(currentLineSize.Width + sz.Width, finalSize.Width))
			{
				ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, i, finalSize);
				accumulatedHeight += currentLineSize.Height;
				currentLineSize = sz;
				firstInLine = i;
				if (MathUtilities.GreaterThan(sz.Width, finalSize.Width))
				{
					ArrangeLine(accumulatedHeight, sz.Height, i, i + 1, finalSize);
					accumulatedHeight += sz.Height;
					currentLineSize = new Size();
					firstInLine++;
				}
			}
			else
			{
				currentLineSize = new Size(currentLineSize.Width + sz.Width, double.Max(sz.Height, currentLineSize.Height));
			}
		}
		if (firstInLine < childrenCount)
		{
			ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, childrenCount, finalSize);
		}
		return finalSize;
	}

	private void ArrangeLine(double y, double lineHeight, int start, int end, Size finalSize)
	{
		var children = Tail is null ? Children : Children.Append(Tail);
		var x = 0.0;
		foreach (var child in children.Skip(start).Take(end - start))
		{
			var layoutSlotWidth = child.DesiredSize.Width;
			child.Arrange(new Rect(x, y, child == Tail ? finalSize.Width - x : layoutSlotWidth, lineHeight));
			x += layoutSlotWidth;
		}
	}
}
