using System;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Binding;
using PingUI.Models;
using ReactiveUI;

namespace PingUI.ViewModels;

public class FilterBuilderViewModel : ViewModelBase
{
	private FilterViewModelBase? _SideContent;

	private FilterViewModelBase? _BottomContent;

	private FilterSource _Source;

	public FilterBuilderViewModel()
	{
		this.WhenAnyValue(vm => vm.Source).Subscribe(source =>
		{
			SideContent = null;
			BottomContent = null;
			switch (source)
			{
				case FilterSource.Label:
				case FilterSource.Address:
				case FilterSource.AnyTag:
				case FilterSource.AllTags:
					SideContent = new FilterTextViewModel(source);
					break;
				case FilterSource.And:
				case FilterSource.Or:
					BottomContent = new FilterCombinedViewModel(source);
					break;
				case FilterSource.Not:
					SideContent = new FilterNotViewModel(source);
					break;
			};
		});
		var sideObservable = this.WhenAnyValue(vm => vm.SideContent)
			.Select(content => content?.AnyChange ?? Observable.Empty<Unit>())
			.Switch();
		var bottomObservable = this.WhenAnyValue(vm => vm.BottomContent)
			.Select(content => content?.AnyChange ?? Observable.Empty<Unit>())
			.Switch();
		AnyChange = Changed.Select(_ => Unit.Default)
			.Merge(sideObservable)
			.Merge(bottomObservable);
	}

	public IObservable<Unit> AnyChange
	{
		get;
	}

	public FilterViewModelBase? SideContent
	{
		get => _SideContent;
		private set => this.RaiseAndSetIfChanged(ref _SideContent, value);
	}

	public FilterViewModelBase? BottomContent
	{
		get => _BottomContent;
		private set => this.RaiseAndSetIfChanged(ref _BottomContent, value);
	}

	public FilterSource Source
	{
		get => _Source;
		set => this.RaiseAndSetIfChanged(ref _Source, value);
	}

	public bool Matches(TargetViewModel targetViewModel)
	{
		return Source == FilterSource.Unfiltered || (SideContent?.Matches(targetViewModel) ?? BottomContent?.Matches(targetViewModel) ?? true);
	}
}
