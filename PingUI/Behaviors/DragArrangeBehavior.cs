using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PingUI.Extensions;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.Behaviors;

public class DragArrangeBehavior : Behavior
{
	public static readonly StyledProperty<double> ScrollEdgeDistanceProperty = AvaloniaProperty.Register<DragArrangeBehavior, double>(nameof(ScrollEdgeDistance), 50.0);

	public static readonly StyledProperty<double> HighSpeedScrollMultiplierProperty = AvaloniaProperty.Register<DragArrangeBehavior, double>(nameof(HighSpeedScrollMultiplier), 4.0);

	private bool _enableDrag;

	private SerialDisposable _dragStarted = new();

	private Point _start;

	private int _draggedIndex;

	private int _targetIndex;

	private ItemsControl? _itemsControl;

	private Control? _draggedContainer;

	private bool _captured;

	private double _relativeToScrollViewer;

	public double ScrollEdgeDistance
	{
		get => GetValue(ScrollEdgeDistanceProperty);
		set => SetValue(ScrollEdgeDistanceProperty, value);
	}

	public double HighSpeedScrollMultiplier
	{
		get => GetValue(HighSpeedScrollMultiplierProperty);
		set => SetValue(HighSpeedScrollMultiplierProperty, value);
	}

	protected override void OnAttached()
	{
		if (AssociatedObject is Control associatedObject)
		{
			associatedObject.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, RoutingStrategies.Tunnel);
			associatedObject.AddHandler(InputElement.PointerPressedEvent, PointerPressed, RoutingStrategies.Tunnel);
			associatedObject.AddHandler(InputElement.PointerMovedEvent, PointerMoved, RoutingStrategies.Tunnel);
			associatedObject.AddHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost, RoutingStrategies.Tunnel);
		}
	}

	protected override void OnDetaching()
	{
		if (AssociatedObject is Control associatedObject)
		{
			associatedObject.RemoveHandler(InputElement.PointerReleasedEvent, PointerReleased);
			associatedObject.RemoveHandler(InputElement.PointerPressedEvent, PointerPressed);
			associatedObject.RemoveHandler(InputElement.PointerMovedEvent, PointerMoved);
			associatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, PointerCaptureLost);
		}
	}

	private void PointerPressed(object? sender, PointerPressedEventArgs e)
	{
		var properties = e.GetCurrentPoint(AssociatedObject as Control).Properties;
		if (properties.IsLeftButtonPressed && AssociatedObject is Control { Parent: ItemsControl itemsControl })
		{
			_enableDrag = true;
			_dragStarted.Disposable = null;
			_start = e.GetPosition(itemsControl);
			_draggedIndex = -1;
			_targetIndex = -1;
			_itemsControl = itemsControl;
			_draggedContainer = AssociatedObject as Control;
			if (_draggedContainer is not null)
			{
				SetDraggingPseudoClasses(_draggedContainer, true);
			}
			AddTransforms(_itemsControl);
			_captured = true;
		}
	}

	private void PointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (_captured)
		{
			if (e.InitialPressMouseButton == MouseButton.Left)
			{
				Released();
			}
			_captured = false;
		}
	}

	private void PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
	{
		Released();
		_captured = false;
	}

	private void Released()
	{
		if (!_enableDrag)
		{
			return;
		}
		RemoveTransforms(_itemsControl);
		if (_itemsControl is not null)
		{
			foreach (var control in _itemsControl.GetRealizedContainers())
			{
				SetDraggingPseudoClasses(control, true);
			}
		}
		if (_dragStarted.Disposable is not null && _draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
		{
			Locator.Current.GetRequiredService<IConfiguration>().Targets.Move(_draggedIndex, _targetIndex);
		}
		if (_itemsControl is not null)
		{
			foreach (var control in _itemsControl.GetRealizedContainers())
			{
				SetDraggingPseudoClasses(control, false);
			}
		}
		if (_draggedContainer is not null)
		{
			SetDraggingPseudoClasses(_draggedContainer, false);
		}
		_draggedIndex = -1;
		_targetIndex = -1;
		_enableDrag = false;
		_dragStarted.Disposable = null;
		_itemsControl = null;
		_draggedContainer = null;
	}

	private static void AddTransforms(ItemsControl? itemsControl)
	{
		if (itemsControl?.Items is null)
		{
			return;
		}
		var i = 0;
		foreach (var _ in itemsControl.Items)
		{
			var container = itemsControl.ContainerFromIndex(i);
			if (container is not null)
			{
				SetTranslateTransform(container, 0, 0);
			}
			i++;
		}
	}

	private static void RemoveTransforms(ItemsControl? itemsControl)
	{
		if (itemsControl?.Items is null)
		{
			return;
		}
		var i = 0;
		foreach (var _ in itemsControl.Items)
		{
			var container = itemsControl.ContainerFromIndex(i);
			if (container is not null)
			{
				SetTranslateTransform(container, 0, 0);
			}
			i++;
		}
	}

	private void PointerMoved(object? sender, PointerEventArgs e)
	{
		var properties = e.GetCurrentPoint(AssociatedObject as Control).Properties;
		if (_captured && properties.IsLeftButtonPressed)
		{
			if (_itemsControl?.Items is null || _draggedContainer?.RenderTransform is null || !_enableDrag)
			{
				return;
			}
			var scrollViewer = _itemsControl.FindAncestorOfType<ScrollViewer>();
			if (scrollViewer is not null)
			{
				_relativeToScrollViewer = e.GetPosition(scrollViewer).Y;
			}
			var position = e.GetPosition(_itemsControl);
			var delta = position.Y - _start.Y;
			if (_dragStarted.Disposable is null)
			{
				var diff = _start - position;
				if (Math.Abs(diff.Y) > 3.0)
				{
					if (scrollViewer is not null)
					{
						_dragStarted.Disposable = Observable.Interval(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
							.Subscribe(_ =>
							{
								if (_draggedContainer is null)
								{
									return;
								}
								if (_relativeToScrollViewer < 0.0)
								{
									scrollViewer.Offset -= new Vector(0, scrollViewer.SmallChange.Height * HighSpeedScrollMultiplier);
								}
								else if (_relativeToScrollViewer < ScrollEdgeDistance)
								{
									scrollViewer.Offset -= new Vector(0, scrollViewer.SmallChange.Height);
								}
								else if (_relativeToScrollViewer > scrollViewer.Viewport.Height)
								{
									scrollViewer.Offset += new Vector(0, scrollViewer.SmallChange.Height * HighSpeedScrollMultiplier);
								}
								else if (_relativeToScrollViewer > scrollViewer.Viewport.Height - ScrollEdgeDistance)
								{
									scrollViewer.Offset += new Vector(0, scrollViewer.SmallChange.Height);
								}
							});
					}
					else
					{
						_dragStarted.Disposable = Disposable.Empty;
					}
				}
				else
				{
					return;
				}
			}
			SetTranslateTransform(_draggedContainer, 0, delta);
			// TODO scroll the view if possible
			// something like checking position of cursor relative to top or bottom 10 pixels of ScrollViewer and tick up or down continuously until moved out
			// maybe accelerate scrolling the closer to the bottom?
			_draggedIndex = _itemsControl.IndexFromContainer(_draggedContainer);
			_targetIndex = -1;
			var draggedBounds = _draggedContainer.Bounds;
			var draggedStart = draggedBounds.Y;
			var draggedDeltaStart = draggedBounds.Y + delta;
			var draggedDeltaEnd = draggedBounds.Y + delta + draggedBounds.Height;
			var i = 0;
			foreach (var _ in _itemsControl.Items)
			{
				var targetContainer = _itemsControl.ContainerFromIndex(i);
				if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
				{
					i++;
					continue;
				}
				var targetBounds = targetContainer.Bounds;
				var targetStart = targetBounds.Y;
				var targetMid = targetBounds.Y + (targetBounds.Height / 2.0);
				var targetIndex = _itemsControl.IndexFromContainer(targetContainer);
				if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
				{
					SetTranslateTransform(targetContainer, 0, -draggedBounds.Height);
					_targetIndex = _targetIndex == -1 ? targetIndex : targetIndex > _targetIndex ? targetIndex : _targetIndex;
				}
				else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
				{
					SetTranslateTransform(targetContainer, 0, draggedBounds.Height);
					_targetIndex = _targetIndex == -1 ? targetIndex : targetIndex < _targetIndex ? targetIndex : _targetIndex;
				}
				else
				{
					SetTranslateTransform(targetContainer, 0, 0);
				}
				i++;
			}
		}
	}

	private static void SetDraggingPseudoClasses(Control control, bool isDragging)
	{
		if (isDragging)
		{
			((IPseudoClasses)control.Classes).Add(":dragging");
		}
		else
		{
			((IPseudoClasses)control.Classes).Remove(":dragging");
		}
	}

	private static void SetTranslateTransform(Control control, double x, double y)
	{
		var transformBuilder = new TransformOperations.Builder(1);
		transformBuilder.AppendTranslate(x, y);
		control.RenderTransform = transformBuilder.Build();
	}
}
