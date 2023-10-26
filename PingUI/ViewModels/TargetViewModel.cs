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
public sealed class TargetViewModel : ViewModelBase, IDisposable
{
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
	/// Backing store for <see cref="Target" />.
	/// </summary>
	private Target _Target;

	/// <summary>
	/// Signal for <see cref="Dispose(bool)" />.
	/// </summary>
	private bool disposedValue;

	/// <summary>
	/// Storage for <see cref="Dispose(bool)" />.
	/// </summary>
	private readonly CompositeDisposable _Disposables;

	/// <summary>
	/// Backing store for <see cref="Address" />.
	/// </summary>
	private readonly ObservableAsPropertyHelper<IPAddress> _Address;

	/// <summary>
	/// Backing store for <see cref="Label" />.
	/// </summary>
	private readonly ObservableAsPropertyHelper<string?> _Label;

	/// <summary>
	/// Backing store for <see cref="CoolDown" />.
	/// </summary>
	private readonly ObservableAsPropertyHelper<TimeSpan> _CoolDown;

	/// <summary>
	/// Initializes a new <see cref="TargetViewModel" />.
	/// </summary>
	/// <param name="target">The target to display.</param>
	public TargetViewModel(Target target)
	{
		_Target = target;
		_Address = this.WhenAnyValue(vm => vm.Target)
			.Select(target => target.Address)
			.ToProperty(this, vm => vm.Address);
		_Label = this.WhenAnyValue(vm => vm.Target)
			.Select(target => target.Label)
			.ToProperty(this, vm => vm.Label);
		_CoolDown = this.WhenAnyValue(vm => vm.Target)
			.Select(target => target.CoolDown)
			.ToProperty(this, vm => vm.CoolDown);
		_Pinger = new SerialDisposable();
		_History = [new PingResult(IPStatus.Unknown, DateTime.Now)];
		_Disposables = [];
		this.WhenAnyValue(vm => vm.Target)
			.Buffer(2, 1)
			.Do(values =>
			{
				IsEnabled = false;
				if (!EqualityComparer<IPAddress>.Default.Equals(values[0]?.Address, values[1]?.Address))
				{
					_History.Clear();
					_History.Add(new PingResult(IPStatus.Unknown, DateTime.Now));
				}
			})
			.Subscribe()
			.DisposeWith(_Disposables);
		_Pinger.DisposeWith(_Disposables);
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
			.DisposeWith(_Disposables);
		Transitions = transitions;
		PromptForDeleteInteraction = new Interaction<Target, bool?>();
		PromptForEditInteraction = new Interaction<Target, Target?>();
		ClearAlertCommand = ReactiveCommand.Create(() => { IsAlert = false; }, this.WhenAnyValue(vm => vm.IsAlert));
		DeleteSelfCommand = ReactiveCommand.Create(() =>
		{
			PromptForDeleteInteraction.Handle(Target)
				.ObserveOn(RxApp.MainThreadScheduler)
				.Do(result =>
				{
					if (result == true)
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
				.ObserveOn(RxApp.MainThreadScheduler)
				.Do(result =>
				{
					if (result is Target target && target != Target)
					{
						var configuration = Locator.Current.GetRequiredService<IConfiguration>();
						if (configuration.Targets.Contains(target))
						{
							Locator.Current.GetRequiredService<IErrorReporter>().ReportError(string.Empty, new InvalidOperationException(string.Format(Strings.Shared_Error_DuplicateTarget, target)));
							return;
						}
						configuration.Targets.Replace(Target, target);
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
		var configuration = Locator.Current.GetRequiredService<IConfiguration>();
		MoveUpCommand = ReactiveCommand.Create(
			() =>
			{
				var configuration = Locator.Current.GetRequiredService<IConfiguration>();
				var index = configuration.Targets.IndexOf(Target);
				configuration.Targets.Move(index, index - 1);
			},
			configuration.Targets
				.ToObservableChangeSet()
				.Select(_ => configuration.Targets.IndexOf(Target) != 0));
		MoveDownCommand = ReactiveCommand.Create(
			() =>
			{
				var configuration = Locator.Current.GetRequiredService<IConfiguration>();
				var index = configuration.Targets.IndexOf(Target);
				configuration.Targets.Move(index, index + 1);
			},
			configuration.Targets
				.ToObservableChangeSet()
				.Select(_ => configuration.Targets.IndexOf(Target) != configuration.Targets.Count - 1));
	}

	/// <summary>
	/// Gets the target being displayed.
	/// </summary>
	public Target Target
	{
		get => _Target;
		set => this.RaiseAndSetIfChanged(ref _Target, value);
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
					.ObserveOn(RxApp.MainThreadScheduler)
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
		get => _Address.Value;
	}

	/// <summary>
	/// Gets the label of <see cref="Target" />.
	/// </summary>
	public string? Label
	{
		get => _Label.Value;
	}

	/// <summary>
	/// Gets the cool down of <see cref="Target" />.
	/// </summary>
	public TimeSpan CoolDown
	{
		get => _CoolDown.Value;
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
	/// A command to move this target up in the list.
	/// </summary>
	public ReactiveCommand<Unit, Unit> MoveUpCommand
	{
		get;
	}

	/// <summary>
	/// A command to move this target down in the list.
	/// </summary>
	public ReactiveCommand<Unit, Unit> MoveDownCommand
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

	/// <summary>
	/// Implementation for <see cref="Dispose()" />.
	/// </summary>
	/// <param name="disposing"><see langword="true" /> if called from <see cref="Dispose()" />; <see langword="false" /> if called from finalizer.</param>
	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_Disposables.Dispose();
			}
			disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
