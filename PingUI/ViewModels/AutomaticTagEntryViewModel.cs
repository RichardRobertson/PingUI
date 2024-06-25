using System;
using System.Reactive.Linq;
using PingUI.I18N;
using PingUI.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace PingUI.ViewModels;

public class AutomaticTagEntryViewModel : ViewModelBase
{
	private AutomaticTagSource _Source;

	private AutomaticTagType _Type;

	private string? _Content;

	private string? _Tag;

	public AutomaticTagEntryViewModel()
	{
		this.ValidationRule(vm => vm.Content, content => !string.IsNullOrWhiteSpace(content), Strings.AutomaticTagEntryViewModel_ContentBlank);
		this.ValidationRule(vm => vm.Tag, tag => !string.IsNullOrWhiteSpace(tag), Strings.AutomaticTagEntryViewModel_TagBlank);
		this.ValidationRule(
			vm => vm.Content,
			this.WhenAnyValue(vm => vm.Type, vm => vm.Content)
				.Throttle(TimeSpan.FromMilliseconds(500))
				.Select(((AutomaticTagType type, string? content) pair) =>
				{
					if (pair.type == AutomaticTagType.MatchesRegex || pair.type == AutomaticTagType.DoesNotMatchRegex)
					{
						try
						{
							_ = new System.Text.RegularExpressions.Regex(pair.content!);
						}
						catch
						{
							return false;
						}
					}
					return true;
				}),
			Strings.AutomaticTagEntryViewModel_ContentInvalidRegex);
	}

	public AutomaticTagSource Source
	{
		get => _Source;
		set => this.RaiseAndSetIfChanged(ref _Source, value);
	}

	public AutomaticTagType Type
	{
		get => _Type;
		set => this.RaiseAndSetIfChanged(ref _Type, value);
	}

	public string? Content
	{
		get => _Content;
		set => this.RaiseAndSetIfChanged(ref _Content, value);
	}

	public string? Tag
	{
		get => _Tag;
		set => this.RaiseAndSetIfChanged(ref _Tag, value);
	}

	public AutomaticTagEntry GetAutomaticTagEntry()
	{
		return new(Source, Type, Content!, Tag!);
	}

	public void SetAutomaticTagEntry(AutomaticTagEntry automaticTagEntry)
	{
		Source = automaticTagEntry.Source;
		Type = automaticTagEntry.Type;
		Content = automaticTagEntry.Content;
		Tag = automaticTagEntry.Tag;
	}
}
