using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using PingUI.Extensions;
using PingUI.I18N;
using PingUI.Models;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface that displays a <see cref="Models.Target" /> and interacts with the <see cref="IPinger" /> service.
/// </summary>
public sealed class TargetViewModel : ViewModelBase, IActivatableViewModel
{
	/// <summary>
	/// Backing store for copying history to edited targets.
	/// </summary>
	private static readonly Dictionary<Target, ObservableCollection<PingResult>> HistoryTransfer = new();

	/// <summary>
	/// Disposable container that handles the <see cref="IPinger" /> observable.
	/// </summary>
	private readonly SerialDisposable _Pinger;

	/// <summary>
	/// Backing store for <see cref="Transitions" />.
	/// </summary>
	private readonly ObservableCollection<PingResult> _History;

	/// <summary>
	/// Backing store for <see cref="Transitions" />.
	/// </summary>
	private ReadOnlyObservableCollection<PingResultViewModel>? _Transitions;

	/// <summary>
	/// Backing store for <see cref="Successes" />.
	/// </summary>
	private int _Successes;

	/// <summary>
	/// Backing store for <see cref="PingCount" />.
	/// </summary>
	private int _PingCount;

	/// <summary>
	/// Backing store for <see cref="PercentSuccess" />.
	/// </summary>
	private double _PercentSuccess;

	/// <summary>
	/// Backing store for <see cref="IsAlert" />.
	/// </summary>
	private bool _IsAlert;

	/// <summary>
	/// Backing store for <see cref="IsUnknown" />.
	/// </summary>
	private bool _IsUnknown;

	/// <summary>
	/// Backing store for <see cref="IsSuccess" />.
	/// </summary>
	private bool _IsSuccess;

	/// <summary>
	/// Backing store for <see cref="IsFailure" />.
	/// </summary>
	private bool _IsFailure;

	/// <summary>
	/// Initializes a new <see cref="TargetViewModel" />.
	/// </summary>
	/// <param name="target">The target to display.</param>
	public TargetViewModel(Target target)
	{
		Target = target;
		_Pinger = new SerialDisposable();
		lock (HistoryTransfer)
		{
			if (HistoryTransfer.Remove(target, out var history))
			{
				_History = history;
			}
			else
			{
				_History = [new PingResult(IPStatus.Unknown, DateTime.Now)];
			}
		}
		this.WhenActivated(disposables =>
		{
			_Pinger.DisposeWith(disposables);
			_History.ToObservableChangeSet()
				.OnItemAdded(change =>
				{
					switch (change.Status)
					{
						case IPStatus.Unknown:
							IsUnknown = true;
							IsSuccess = false;
							IsFailure = false;
							break;
						case IPStatus.Success:
							IsUnknown = false;
							IsSuccess = true;
							IsFailure = false;
							break;
						default:
							IsUnknown = false;
							IsSuccess = false;
							IsFailure = true;
							IsAlert = true;
							break;
					}
				})
				.Cast(result => new PingResultViewModel(result))
				.Bind(out var transitions)
				.Subscribe()
				.DisposeWith(disposables);
			Transitions = transitions;
		});
		PromptForDeleteInteraction = new Interaction<Target, bool?>();
		PromptForEditInteraction = new Interaction<Target, Target?>();
		ClearAlertCommand = ReactiveCommand.Create(() => { IsAlert = false; }, this.WhenAnyValue(vm => vm.IsAlert));
		DeleteSelfCommand = ReactiveCommand.Create(() =>
		{
			PromptForDeleteInteraction.Handle(Target)
				.Do(result =>
				{
					if (true.Equals(result))
					{
						var configuration = Locator.Current.GetRequiredService<IConfiguration>();
						configuration.Targets.Remove(Target);
						_Pinger.Dispose();
					}
				})
				.Subscribe();
		});
		EditSelfCommand = ReactiveCommand.Create(() =>
		{
			PromptForEditInteraction.Handle(Target)
				.Do(result =>
				{
					if (result is Target target)
					{
						var configuration = Locator.Current.GetRequiredService<IConfiguration>();
						lock (HistoryTransfer)
						{
							HistoryTransfer[target] = _History;
						}
						configuration.Targets.Replace(Target, target);
						_Pinger.Dispose();
					}
				})
				.Subscribe();
		});
		ClearHistoryCommand = ReactiveCommand.Create(() =>
		{
			_History.Clear();
			_History.Add(new PingResult(IPStatus.Unknown, DateTime.Now));
			Successes = 0;
			PingCount = 0;
		});
	}

	/// <summary>
	/// Gets the target being displayed.
	/// </summary>
	public Target Target
	{
		get;
	}

	/// <summary>
	/// Gets the list of ping result changes.
	/// </summary>
	/// <remarks>Not every ping result is logged. Entries are only added when <see cref="PingResult.Status" /> changes.</remarks>
	public ReadOnlyObservableCollection<PingResultViewModel>? Transitions
	{
		get => _Transitions;
		private set => this.RaiseAndSetIfChanged(ref _Transitions, value);
	}

	/// <summary>
	/// Gets or sets a value enabling pings to be sent to the target.
	/// </summary>
	/// <value><see langword="true" /> if <see cref="Target" /> should be pinged; otherwise <see langword="false" />.</value>
	public bool IsEnabled
	{
		get => _Pinger.Disposable is not null;
		set
		{
			if (_Pinger.Disposable is not null && !value)
			{
				this.RaisePropertyChanging();
				_Pinger.Disposable = null;
				_History.Insert(0, new PingResult(IPStatus.Unknown, DateTime.Now));
				this.RaisePropertyChanged();
			}
			else if (_Pinger.Disposable is null && value)
			{
				this.RaisePropertyChanging();
				_Pinger.Disposable = Locator.Current
					.GetRequiredService<IPinger>()
					.GetObservable(Target)
					.Subscribe(Observer.Create<PingResult>(
						item =>
						{
							if (_History[0].Status != item.Status)
							{
								_History.Insert(0, item);
							}
							if (item.Status == IPStatus.Success)
							{
								Successes++;
							}
							if (item.Status != IPStatus.Unknown)
							{
								PingCount++;
								PercentSuccess = (double)_Successes / _PingCount;
							}
						},
						exception =>
						{
#pragma warning disable CA2011
							// justification: assignment is on an event, not the direct body of the setter
							IsEnabled = false;
#pragma warning restore
							Locator.Current.GetRequiredService<IErrorReporter>().ReportError(string.Format(Strings.TargetViewModel_Error_Ping, Address), exception);
						}));
				this.RaisePropertyChanged();
			}
		}
	}

	/// <summary>
	/// Gets the address of <see cref="Target" />.
	/// </summary>
	public IPAddress Address
	{
		get => Target.Address;
	}

	/// <summary>
	/// Gets the label of <see cref="Target" />.
	/// </summary>
	public string? Label
	{
		get => Target.Label;
	}

	/// <summary>
	/// Gets the cool down of <see cref="Target" />.
	/// </summary>
	public TimeSpan CoolDown
	{
		get => Target.CoolDown;
	}

	/// <summary>
	/// Gets a value indicating if the pinger is currently in a success state.
	/// </summary>
	/// <value><see langword="true" /> if the top <see cref="Transitions" /> status is <see cref="IPStatus.Success" />; otherwise <see langword="false" />.</value>
	public bool IsSuccess
	{
		get => _IsSuccess;
		private set => this.RaiseAndSetIfChanged(ref _IsSuccess, value);
	}

	/// <summary>
	/// Gets a value indicating if the pinger is currently in a failure state.
	/// </summary>
	/// <value><see langword="true" /> if the top <see cref="Transitions" /> status is not <see cref="IPStatus.Success" /> or <see cref="IPStatus.Unknown" />; otherwise <see langword="false" />.</value>
	public bool IsFailure
	{
		get => _IsFailure;
		private set => this.RaiseAndSetIfChanged(ref _IsFailure, value);
	}

	/// <summary>
	/// Gets a value indicating if the pinger is currently in an unknown state.
	/// </summary>
	/// <value><see langword="true" /> if the top <see cref="Transitions" /> status is <see cref="IPStatus.Unknown" />; otherwise <see langword="false" />.</value>
	public bool IsUnknown
	{
		get => _IsUnknown;
		private set => this.RaiseAndSetIfChanged(ref _IsUnknown, value);
	}

	/// <summary>
	/// Gets the number of pings that resulted in <see cref="IPStatus.Success" />.
	/// </summary>
	public int Successes
	{
		get => _Successes;
		private set => this.RaiseAndSetIfChanged(ref _Successes, value);
	}

	/// <summary>
	/// Gets the number of pings that did not result in <see cref="IPStatus.Unknown" />.
	/// </summary>
	public int PingCount
	{
		get => _PingCount;
		private set => this.RaiseAndSetIfChanged(ref _PingCount, value);
	}

	/// <summary>
	/// Gets the percentage of pings that were successful.
	/// </summary>
	/// <value>A number from 0.0 to 1.0 inclusively or <see cref="double.NaN" /> calculated by dividing <see cref="Successes" /> by <see cref="PingCount" />.</value>
	public double PercentSuccess
	{
		get => _PercentSuccess;
		private set => this.RaiseAndSetIfChanged(ref _PercentSuccess, value);
	}

	/// <summary>
	/// Gets a value indicating whether a ping result that was neither <see cref="IPStatus.Success" /> not <see cref="IPStatus.Unknown" /> occurred since the alert was last acknowledged.
	/// </summary>
	public bool IsAlert
	{
		get => _IsAlert;
		private set => this.RaiseAndSetIfChanged(ref _IsAlert, value);
	}

	/// <summary>
	/// A command to clear <see cref="IsAlert" />.
	/// </summary>
	public ReactiveCommand<Unit, Unit> ClearAlertCommand
	{
		get;
	}

	/// <summary>
	/// A command to initiate removal of <see cref="Target" /> from the application.
	/// </summary>
	public ReactiveCommand<Unit, Unit> DeleteSelfCommand
	{
		get;
	}

	/// <summary>
	/// A command to initiate modification of <see cref="Target" />.
	/// </summary>
	public ReactiveCommand<Unit, Unit> EditSelfCommand
	{
		get;
	}

	/// <summary>
	/// A command to clear <see cref="Transitions" />.
	/// </summary>
	public ReactiveCommand<Unit, Unit> ClearHistoryCommand
	{
		get;
	}

	/// <summary>
	/// An interaction with the intent of confirming deletion of this target.
	/// </summary>
	public Interaction<Target, bool?> PromptForDeleteInteraction
	{
		get;
	}

	/// <summary>
	/// An interaction with the intent of editing this target.
	/// </summary>
	public Interaction<Target, Target?> PromptForEditInteraction
	{
		get;
	}

	/// <inheritdoc />
	public ViewModelActivator Activator
	{
		get;
	} = new();
}
