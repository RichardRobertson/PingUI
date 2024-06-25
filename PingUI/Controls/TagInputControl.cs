using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using PingUI.Models;
using PingUI.ViewModels;
using ReactiveUI;

namespace PingUI.Controls;

[TemplatePart(PART_TextBox, typeof(AutoCompleteBox))]
[TemplatePart(PART_ItemsPresenter, typeof(ItemsPresenter))]
public class TagInputControl : ItemsControl
{
	public const string PART_TextBox = "PART_TextBox";

	public const string PART_ItemsPresenter = "PART_ItemsPresenter";

	public static readonly DirectProperty<TagInputControl, ReactiveCommand<TargetTagViewModel, Unit>> DeleteItemCommandProperty = AvaloniaProperty.RegisterDirect<TagInputControl, ReactiveCommand<TargetTagViewModel, Unit>>(nameof(DeleteItemCommand), tic => tic.DeleteItemCommand);

	private AutoCompleteBox? _TextBox;

	private ItemsPresenter? _ItemsPresenter;

	public TagInputControl()
	{
		DeleteItemCommand = ReactiveCommand.Create<TargetTagViewModel>(tag =>
		{
			(ItemsSource as IList<TargetTagViewModel>)?.Remove(tag);
			_TextBox?.Focus();
		});
		RaisePropertyChanged(DeleteItemCommandProperty, null!, DeleteItemCommand);
	}

	public ReactiveCommand<TargetTagViewModel, Unit> DeleteItemCommand
	{
		get;
	}

	protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
	{
		base.OnApplyTemplate(e);
		if (_TextBox is not null)
		{
			_TextBox.KeyUp -= OnTextBoxKeyUp;
			_TextBox.SelectionChanged -= OnTextBoxSelectionChanged;
			_TextBox.TextFilter = null;
			_TextBox.AsyncPopulator = null;
		}
		_TextBox = e.NameScope.Find<AutoCompleteBox>(PART_TextBox);
		RegisterTextBoxEvents();
		if (_ItemsPresenter is not null)
		{
			_ItemsPresenter.LayoutUpdated -= OnItemsPresenterLayoutUpdated;
		}
		_ItemsPresenter = e.NameScope.Find<ItemsPresenter>(PART_ItemsPresenter);
		if (_ItemsPresenter is not null)
		{
			_ItemsPresenter.LayoutUpdated += OnItemsPresenterLayoutUpdated;
		}
	}

	private void OnTextBoxKeyUp(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			AddTag();
			e.Handled = true;
		}
	}

	private void OnItemsPresenterLayoutUpdated(object? sender, EventArgs e)
	{
		if (_TextBox is null)
		{
			_TextBox = this.Find<AutoCompleteBox>(PART_TextBox)
				?? this.GetVisualDescendants()
					.OfType<AutoCompleteBox>()
					.FirstOrDefault(tb => tb.Name == PART_TextBox);
			RegisterTextBoxEvents();
		}
	}

	private void RegisterTextBoxEvents()
	{
		if (_TextBox is not null)
		{
			_TextBox.KeyUp += OnTextBoxKeyUp;
			_TextBox.SelectionChanged += OnTextBoxSelectionChanged;
			_TextBox.TextFilter = FilterSuggestions;
			_TextBox.AsyncPopulator = PopulateSuggestionsAsync;
		}
	}

	private Task<IEnumerable<object>> PopulateSuggestionsAsync(string? searchText, CancellationToken cancellationToken)
	{
		return Task.Run<IEnumerable<object>>(
			() => FuzzierSharp.Process.ExtractTop(searchText, TargetTag.LatestAllTags)
				.Select(top => top.Value),
			cancellationToken);
	}

	private bool FilterSuggestions(string? search, string? item)
	{
		return true;
	}

	private void AddTag()
	{
		if (_TextBox is not null && !string.IsNullOrWhiteSpace(_TextBox.Text))
		{
			(ItemsSource as IList<TargetTagViewModel>)?.Add(new TargetTagViewModel(new TargetTag(_TextBox.Text, false)));
			_TextBox.Text = string.Empty;
		}
	}

	private void OnTextBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count > 0 && sender is AutoCompleteBox textBox && textBox.Text != e.AddedItems[0] as string)
		{
			AddTag();
			textBox.SelectedItem = null;
			textBox.Text = string.Empty;
		}
	}
}
