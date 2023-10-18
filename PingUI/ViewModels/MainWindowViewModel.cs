using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using DialogHostAvalonia;
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
/// Represents the main interface that shows the list of targets and enables creation of new targets.
/// </summary>
public class MainWindowViewModel : ViewModelBase, IActivatableViewModel
{
	/// <summary>
	/// Reference to the current application configuration.
	/// </summary>
	private readonly IConfiguration configuration;

	/// <summary>
	/// Backing store for <see cref="Targets" />.
	/// </summary>
	private ReadOnlyObservableCollection<TargetViewModel>? _Targets;

	/// <summary>
	/// Initializes a new <see cref="MainWindowViewModel" />.
	/// </summary>
	public MainWindowViewModel()
	{
		configuration = Locator.Current.GetRequiredService<IConfiguration>();
		this.WhenActivated(disposables =>
		{
			configuration.Targets
				.ToObservableChangeSet()
				.Cast(target => new TargetViewModel(target))
				.Bind(out var targets)
				.Subscribe()
				.DisposeWith(disposables);
			Targets = targets;
			Disposable.Create(() => Targets = null).DisposeWith(disposables);
		});
		AddTargetCommand = ReactiveCommand.Create(() =>
		{
			DialogHost.Show(new EditTargetViewModel(null))
				.ToObservable()
				.Do(result =>
				{
					if (result is Target target)
					{
						if (configuration.Targets.Contains(target))
						{
							Locator.Current.GetRequiredService<IErrorReporter>().ReportError(string.Empty, new InvalidOperationException(string.Format(Strings.Shared_Error_DuplicateTarget, target)));
							return;
						}
						configuration.Targets.Add(target);
					}
				})
				.Subscribe();
		});
	}

	/// <summary>
	/// Gets the list of <see cref="Target" /> view models.
	/// </summary>
	public ReadOnlyObservableCollection<TargetViewModel>? Targets
	{
		get => _Targets;
		private set => this.RaiseAndSetIfChanged(ref _Targets, value);
	}

	/// <summary>
	/// A command to add a new target.
	/// </summary>
	public ReactiveCommand<Unit, Unit> AddTargetCommand
	{
		get;
	}

	/// <summary>
	/// Indicates whether the main window should display on top of other windows at all times.
	/// </summary>
	public bool Topmost
	{
		get => configuration.Topmost;
		set
		{
			if (configuration.Topmost != value)
			{
				this.RaisePropertyChanging();
				configuration.Topmost = value;
				this.RaisePropertyChanged();
			}
		}
	}

	/// <inheritdoc />
	public ViewModelActivator Activator
	{
		get;
	} = new();
}
