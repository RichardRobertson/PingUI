using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DialogHostAvalonia;
using DynamicData;
using DynamicData.Binding;
using PingUI.Collections;
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
	/// The GitHub API path to check for updates.
	/// </summary>
	public const string GitHubApiReleasesUri = "https://api.github.com/repos/RichardRobertson/PingUI/releases/latest";

	/// <summary>
	/// Reference to the current application configuration.
	/// </summary>
	private readonly IConfiguration configuration;

	private readonly ObservableAsPropertyHelper<bool> _IsFiltered;

	/// <summary>
	/// Backing store for <see cref="Targets" />.
	/// </summary>
	private ReadOnlyObservableFilteredCollection<ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>, TargetViewModel>? _Targets;

	/// <summary>
	/// Backing store for <see cref="ViewStyle" />.
	/// </summary>
	private MainViewStyle _ViewStyle;

	/// <summary>
	/// Backing store for <see cref="IsDetailsView" />.
	/// </summary>
	private bool _IsDetailsView;

	/// <summary>
	/// Backing store for <see cref="IsCondensedView" />.
	/// </summary>
	private bool _IsCondensedView;

	/// <summary>
	/// Initializes a new <see cref="MainWindowViewModel" />.
	/// </summary>
	public MainWindowViewModel()
	{
		configuration = Locator.Current.GetRequiredService<IConfiguration>();
		FilterBuilder = new FilterBuilderViewModel();
		_IsFiltered = FilterBuilder.AnyChange.Select(_ => FilterBuilder.Source != FilterSource.Unfiltered).ToProperty(this, vm => vm.IsFiltered);
		this.WhenActivated(disposables =>
		{
			Targets = new ReadOnlyObservableFilteredCollection<ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>, TargetViewModel>(
				new ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>(
					configuration.Targets,
					t => new TargetViewModel(t),
					(t, tvm) => tvm.Target = t,
					tvm => tvm.Target),
				FilterBuilder.AnyChange.Select<Unit, Func<TargetViewModel, bool>>(_ => FilterBuilder.Matches));
			Targets.DisposeWith(disposables);
			Disposable.Create(() => Targets = null).DisposeWith(disposables);
			Observable.FromAsync(CheckForUpdatesAsync).Subscribe().DisposeWith(disposables);
			this.WhenValueChanged(vm => vm.ViewStyle)
				.Subscribe(viewStyle =>
				{
					switch (viewStyle)
					{
						case MainViewStyle.Details:
							IsCondensedView = false;
							IsDetailsView = true;
							break;
						case MainViewStyle.Condensed:
							IsDetailsView = false;
							IsCondensedView = true;
							break;
						default:
							throw new InvalidEnumArgumentException(nameof(ViewStyle), (int)viewStyle, typeof(MainViewStyle));
					}
				})
				.DisposeWith(disposables);
		});
		AddTargetCommand = ReactiveCommand.Create(() =>
		{
			DialogHost.Show(new EditTargetViewModel(null))
				.ToObservable()
				.ObserveOn(RxApp.MainThreadScheduler)
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
		DeleteAllTargetsCommand = ReactiveCommand.Create(
			() =>
			{
				DialogHost.Show(new DeleteAllTargetsViewModel())
					.ToObservable()
					.ObserveOn(RxApp.MainThreadScheduler)
					.Do(result =>
					{
						if (result as bool? == true && _Targets is { } targets)
						{
							foreach (var target in targets)
							{
								target.IsEnabled = false;
							}
							configuration.Targets.Clear();
						}
					})
					.Subscribe();
			},
			configuration.Targets.WhenAnyValue(targets => targets.Count).Select(count => count != 0));
		ShowSettingsCommand = ReactiveCommand.Create(() =>
		{
			DialogHost.Show(new SettingsViewModel());
		});
		ShowEditAutomaticTagsCommand = ReactiveCommand.Create(() =>
		{
			DialogHost.Show(new EditAutomaticTagsDialogViewModel());
		});
		StartPingingAllCommand = ReactiveCommand.Create(() =>
		{
			if (_Targets is { } targets)
			{
				foreach (var target in targets)
				{
					target.IsEnabled = true;
				}
			}
		});
		StopPingingAllCommand = ReactiveCommand.Create(() =>
		{
			if (_Targets is { } targets)
			{
				foreach (var target in targets)
				{
					target.IsEnabled = false;
				}
			}
		});
		StartPingingAllFilteredCommand = ReactiveCommand.Create(
			() =>
			{
				if (_Targets is { } targets)
				{
					foreach (var target in targets)
					{
						if (FilterBuilder.Matches(target))
						{
							target.IsEnabled = true;
						}
					}
				}
			},
			this.WhenValueChanged(vm => vm.IsFiltered));
		StopPingingAllFilteredCommand = ReactiveCommand.Create(
			() =>
			{
				if (_Targets is { } targets)
				{
					foreach (var target in targets)
					{
						if (FilterBuilder.Matches(target))
						{
							target.IsEnabled = false;
						}
					}
				}
			},
			this.WhenValueChanged(vm => vm.IsFiltered));
		_ViewStyle = MainViewStyle.Details;
		SetDetailsViewCommand = ReactiveCommand.Create(() => { ViewStyle = MainViewStyle.Details; }, this.WhenValueChanged(vm => vm.IsDetailsView).Select(isDetailsView => !isDetailsView));
		SetCondensedViewCommand = ReactiveCommand.Create(() => { ViewStyle = MainViewStyle.Condensed; }, this.WhenValueChanged(vm => vm.IsCondensedView).Select(isCondensedView => !isCondensedView));
	}

	/// <summary>
	/// Check for updates from a new GitHub release.
	/// </summary>
	/// <returns>The task object representing the asynchronous operation.</returns>
	private async Task CheckForUpdatesAsync()
	{
		configuration.CheckOnlineForUpdates ??= await DialogHost.Show(new AskToEnableUpdateCheckViewModel()).ConfigureAwait(false) as bool?;
		if (configuration.CheckOnlineForUpdates != true)
		{
			return;
		}
		try
		{
			using var client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"github.com-RichardRobertson-PingUI/{typeof(MainWindowViewModel).Assembly.GetName().Version}");
			var responseJson = await client.GetFromJsonAsync(GitHubApiReleasesUri, JsonContext.Instance.GitHubRelease).ConfigureAwait(false);
			if (responseJson?.AllSet == true && Version.TryParse(responseJson.TagName[1..], out var version)
#if !DEBUG
				&& typeof(MainWindowViewModel).Assembly.GetName().Version < version
#endif
			)
			{
				RxApp.MainThreadScheduler.Schedule(() =>
				{
					DialogHost.Show(new UpdateNotificationViewModel(responseJson.HtmlUrl, responseJson.Body))
						.ToObservable()
						.Subscribe();
				});
			}
		}
		catch (Exception exception)
		{
			Locator.Current.GetRequiredService<IErrorReporter>().ReportError(Strings.MainWindow_Error_CheckForUpdatesAsync, exception);
		}
	}

	/// <summary>
	/// Gets the list of <see cref="Target" /> view models.
	/// </summary>
	public ReadOnlyObservableFilteredCollection<ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>, TargetViewModel>? Targets
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
	/// A command to show the settings dialog.
	/// </summary>
	public ReactiveCommand<Unit, Unit> ShowSettingsCommand
	{
		get;
	}

	/// <summary>
	/// A command to show the automatic tags editor dialog.
	/// </summary>
	public ReactiveCommand<Unit, Unit> ShowEditAutomaticTagsCommand
	{
		get;
	}

	/// <summary>
	/// A command to start pinging all targets.
	/// </summary>
	public ReactiveCommand<Unit, Unit> StartPingingAllCommand
	{
		get;
	}

	/// <summary>
	/// A command to stop pinging all targets.
	/// </summary>
	public ReactiveCommand<Unit, Unit> StopPingingAllCommand
	{
		get;
	}

	/// <summary>
	/// A command to start pinging all targets that match the current filter.
	/// </summary>
	public ReactiveCommand<Unit, Unit> StartPingingAllFilteredCommand
	{
		get;
	}

	/// <summary>
	/// A command to stop pinging all targets that match the current filter.
	/// </summary>
	public ReactiveCommand<Unit, Unit> StopPingingAllFilteredCommand
	{
		get;
	}

	/// <summary>
	/// Change the view to details.
	/// </summary>
	public ReactiveCommand<Unit, Unit> SetDetailsViewCommand
	{
		get;
	}

	/// <summary>
	/// Change the view to icons.
	/// </summary>
	public ReactiveCommand<Unit, Unit> SetCondensedViewCommand
	{
		get;
	}

	/// <summary>
	/// Delete all targets at once.
	/// </summary>
	public ReactiveCommand<Unit, Unit> DeleteAllTargetsCommand
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

	/// <summary>
	/// Indicate how to display the list of targets.
	/// </summary>
	public MainViewStyle ViewStyle
	{
		get => _ViewStyle;
		set => this.RaiseAndSetIfChanged(ref _ViewStyle, value);
	}

	/// <summary>
	/// Indicates when the target view style should be details.
	/// </summary>
	public bool IsDetailsView
	{
		get => _IsDetailsView;
		private set => this.RaiseAndSetIfChanged(ref _IsDetailsView, value);
	}

	/// <summary>
	/// Indicates when the target view style should be icons.
	/// </summary>
	public bool IsCondensedView
	{
		get => _IsCondensedView;
		private set => this.RaiseAndSetIfChanged(ref _IsCondensedView, value);
	}

	public FilterBuilderViewModel FilterBuilder
	{
		get;
	}

	public bool IsFiltered => _IsFiltered.Value;

	/// <inheritdoc />
	public ViewModelActivator Activator
	{
		get;
	} = new();
}
