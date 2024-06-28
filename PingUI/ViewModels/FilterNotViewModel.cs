using System;
using System.Reactive;
using System.Reactive.Linq;
using PingUI.Models;

namespace PingUI.ViewModels;

public class FilterNotViewModel : FilterViewModelBase
{
	public FilterNotViewModel(FilterSource source)
		: base(source)
	{
		Child = new FilterBuilderViewModel();
		AnyChange = Changed.Select(_ => Unit.Default).Merge(Child.AnyChange);
	}

	public override IObservable<Unit> AnyChange
	{
		get;
	}

	public FilterBuilderViewModel Child
	{
		get;
	}

	public override bool Matches(TargetViewModel targetViewModel)
	{
		return !Child.Matches(targetViewModel);
	}
}
