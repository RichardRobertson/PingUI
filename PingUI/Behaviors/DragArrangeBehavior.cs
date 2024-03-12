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

/// <summary>
/// Behavior to allow dragging items in an <see cref="ItemsControl" /> to reorder them.
/// </summary>
public class DragArrangeBehavior : Behavior
{
	/// <summary>
	/// Defines the <see cref="ScrollEdgeDistance" /> property.
	/// </summary>
	public static readonly StyledProperty<double> ScrollEdgeDistanceProperty = AvaloniaProperty.Register<DragArrangeBehavior, double>(nameof(ScrollEdgeDistance), 50.0);

	/// <summary>
	/// Defines the <see cref="HighSpeedScrollMultiplier" /> property.
	/// </summary>
	public static readonly StyledProperty<double> HighSpeedScrollMultiplierProperty = AvaloniaProperty.Register<DragArrangeBehavior, double>(nameof(HighSpeedScrollMultiplier), 4.0);

	/// <summary>
	/// Indicates that a drag may occur.
	/// </summary>
	private bool _enableDrag;

	/// <summary>
	/// Indicates if a drag is currently running by optionally containing a disposable that handles scrolling.
	/// </summary>
	private readonly SerialDisposable _dragStarted = new();

	/// <summary>
	/// Indicates the point that a drag started at.
	/// </summary>
	private Point _start;

	/// <summary>
	/// Indicates the index of the item being dragged.
	/// </summary>
	private int _draggedIndex;

	/// <summary>
	/// Indicates the index that the dragged item will land on if released.
	/// </summary>
	private int _targetIndex;

	/// <summary>
	/// <see cref="ItemsControl" /> that is the parent of the dragged item.
	/// </summary>
	private ItemsControl? _itemsControl;

	/// <summary>
	/// The dragged item's container.
	/// </summary>
	private Control? _draggedContainer;

	/// <summary>
	/// Indicates if the pointer is captured.
	/// </summary>
	private bool _captured;

	/// <summary>
	/// Indicates the position of the pointer relative to a parent <see cref="ScrollViewer" />.
	/// </summary>
	private double _relativeToScrollViewer;

	/// <summary>
	/// Gets or sets a value indicating how close in pixels the pointer must be to the top or bottom of a parent <see cref="ScrollViewer" /> to begin scrolling.
	/// </summary>
	public double ScrollEdgeDistance
	{
		get => GetValue(ScrollEdgeDistanceProperty);
		set => SetValue(ScrollEdgeDistanceProperty, value);
	}

	/// <summary>
	/// Gets or sets a value indicating the speed multiplier when the pointer exceeds the top or bottom bounds of a parent <see cref="ScrollViewer" /> when scrolling.
	/// </summary>
	public double HighSpeedScrollMultiplier
	{
		get => GetValue(HighSpeedScrollMultiplierProperty);
		set => SetValue(HighSpeedScrollMultiplierProperty, value);
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Occurs when the pointer is pressed over the control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The details of the event.</param>
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
			ResetTransforms(_itemsControl);
			_captured = true;
		}
	}

	/// <summary>
	/// Occurs when the pointer is released over the control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The details of the event.</param>
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

	/// <summary>
	/// Occurs when the control or its child control loses the pointer capture for any
	/// reason, event will not be triggered for a parent control if capture was transferred
	/// to another child of that parent control
	/// </summary>
	/// <param name="sender">The source of hte event.</param>
	/// <param name="e">The details of the event.</param>
	private void PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
	{
		Released();
		_captured = false;
	}

	/// <summary>
	/// Move the dragged item if necessary and clean up.
	/// </summary>
	private void Released()
	{
		if (!_enableDrag)
		{
			return;
		}
		ResetTransforms(_itemsControl);
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

	/// <summary>
	/// Resets <see cref="TransformOperations" /> instances on all items in an <see cref="ItemsControl" />.
	/// </summary>
	/// <param name="itemsControl">The <see cref="ItemsControl" /> whose contained items to transform.</param>
	private static void ResetTransforms(ItemsControl? itemsControl)
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

	/// <summary>
	/// Occurs when the pointer moves over the control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The details of the event.</param>
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

	/// <summary>
	/// Toggle the <c>:dragging</c> pseudo class on or off for a given control.
	/// </summary>
	/// <param name="control">The <see cref="Control" /> to modify.</param>
	/// <param name="isDragging"><see langword="true" /> to add <c>:dragging</c> to the control; <see langword="false" /> to remove <c>:dragging</c> from the control.</param>
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

	/// <summary>
	/// Sets a <see cref="TransformOperations" /> on a <see cref="Control" /> for a translate transform.
	/// </summary>
	/// <param name="control">The <see cref="Control" /> to modify.</param>
	/// <param name="x">The horizontal offset.</param>
	/// <param name="y">The vertical offset.</param>
	private static void SetTranslateTransform(Control control, double x, double y)
	{
		var transformBuilder = new TransformOperations.Builder(1);
		transformBuilder.AppendTranslate(x, y);
		control.RenderTransform = transformBuilder.Build();
	}
}
