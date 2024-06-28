using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using PingUI.I18N;
using PingUI.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace PingUI.ViewModels;

public class FilterTextViewModel : FilterViewModelBase
{
	private static readonly Regex DummyRegex = new(string.Empty);

	private readonly ISubject<Unit> DummyRegexUpdated;

	private string _Content = string.Empty;

	private Regex _Regex;

	private MatchType _Type;

	public FilterTextViewModel(FilterSource source)
		: base(source)
	{
		_Regex = DummyRegex;
		DummyRegexUpdated = new Subject<Unit>();
		this.ValidationRule(vm => vm.Content, content => !string.IsNullOrWhiteSpace(content), Strings.AutomaticTagEntryViewModel_ContentBlank);
		this.ValidationRule(
			vm => vm.Content,
			this.WhenAnyValue(vm => vm.Type, vm => vm.Content)
				.Throttle(TimeSpan.FromMilliseconds(500))
				.Select(((MatchType type, string content) pair) =>
				{
					_Regex = DummyRegex;
					if (pair.type == MatchType.MatchesRegex || pair.type == MatchType.DoesNotMatchRegex)
					{
						try
						{
							_Regex = new Regex(pair.content!);
						}
						catch
						{
							DummyRegexUpdated.OnNext(Unit.Default);
							return false;
						}
					}
					DummyRegexUpdated.OnNext(Unit.Default);
					return true;
				}),
			Strings.AutomaticTagEntryViewModel_ContentInvalidRegex);
		AnyChange = Changed.Select(_ => Unit.Default).Merge(DummyRegexUpdated.ObserveOn(RxApp.MainThreadScheduler));
	}

	public override IObservable<Unit> AnyChange
	{
		get;
	}

	public string Content
	{
		get => _Content;
		set => this.RaiseAndSetIfChanged(ref _Content, value);
	}

	public MatchType Type
	{
		get => _Type;
		set => this.RaiseAndSetIfChanged(ref _Type, value);
	}

	public override bool Matches(TargetViewModel targetViewModel)
	{
		if (HasErrors)
		{
			return true;
		}
		return Source switch
		{
			FilterSource.Label => MatchesText(targetViewModel.Label ?? string.Empty),
			FilterSource.Address => MatchesText(targetViewModel.Address.ToString()),
			FilterSource.AnyTag => targetViewModel.Tags.Any(tag => MatchesText(tag.Text)),
			FilterSource.AllTags => targetViewModel.Tags.All(tag => MatchesText(tag.Text)),
			_ => true,
		};

		bool MatchesText(string source)
		{
			return Type switch
			{
				MatchType.Contains => source.Contains(Content!),
				MatchType.IsExactly => source.Equals(Content),
				MatchType.StartsWith => source.StartsWith(Content!),
				MatchType.EndsWith => source.EndsWith(Content!),
				MatchType.MatchesRegex => _Regex.IsMatch(source),
				MatchType.DoesNotContain => !source.Contains(Content!),
				MatchType.DoesNotMatchExactly => !source.Equals(Content),
				MatchType.DoesNotStartWith => !source.StartsWith(Content!),
				MatchType.DoesNotEndWith => !source.EndsWith(Content!),
				MatchType.DoesNotMatchRegex => !_Regex.IsMatch(source),
				_ => throw new UnreachableException(),
			};
		}
	}
}
