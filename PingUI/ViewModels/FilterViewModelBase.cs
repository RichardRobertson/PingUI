using System;
using System.Reactive;
using PingUI.Models;

namespace PingUI.ViewModels;

public abstract class FilterViewModelBase(FilterSource source) : ViewModelBase
{
	public abstract IObservable<Unit> AnyChange
	{
		get;
	}

	public FilterSource Source
	{
		get;
	} = source;

	public abstract bool Matches(TargetViewModel targetViewModel);
}
