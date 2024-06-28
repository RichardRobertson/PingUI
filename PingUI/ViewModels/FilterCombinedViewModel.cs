using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Binding;
using PingUI.Extensions;
using PingUI.Models;
using ReactiveUI;

namespace PingUI.ViewModels;

public class FilterCombinedViewModel : FilterViewModelBase
{
	public FilterCombinedViewModel(FilterSource source)
		: base(source)
	{
		Children = [];
		AnyChange = Changed.Select(_ => Unit.Default)
			.Merge(Children.WhenCollectionChanged<ObservableCollectionExtended<FilterBuilderViewModel>, FilterBuilderViewModel, IObservable<Unit>>(collection => collection.Aggregate(Observable.Empty<Unit>(), (accumulator, viewModel) => accumulator.Merge(viewModel.AnyChange))).Switch());
		AddChildCommand = ReactiveCommand.Create(() => Children.Add(new()));
		RemoveChildCommand = ReactiveCommand.Create<FilterBuilderViewModel>(viewModel => Children.Remove(viewModel));
	}

	public ReactiveCommand<Unit, Unit> AddChildCommand
	{
		get;
	}

	public override IObservable<Unit> AnyChange
	{
		get;
	}

	public ObservableCollectionExtended<FilterBuilderViewModel> Children
	{
		get;
	}

	public ReactiveCommand<FilterBuilderViewModel, Unit> RemoveChildCommand
	{
		get;
	}

	public override bool Matches(TargetViewModel targetViewModel)
	{
		return Source switch
		{
			FilterSource.And => Children.All(child => child.Matches(targetViewModel)),
			FilterSource.Or => Children.Any(child => child.Matches(targetViewModel)),
			_ => true,
		};
	}
}
