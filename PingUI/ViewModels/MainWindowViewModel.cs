using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DialogHostAvalonia;
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

	/// <summary>
	/// Backing store for <see cref="Targets" />.
	/// </summary>
	private ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>? _Targets;

	/// <summary>
	/// Initializes a new <see cref="MainWindowViewModel" />.
	/// </summary>
	public MainWindowViewModel()
	{
		configuration = Locator.Current.GetRequiredService<IConfiguration>();
		this.WhenActivated(disposables =>
		{
			Targets = new ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>(
				configuration.Targets,
				t => new TargetViewModel(t),
				(t, tvm) => tvm.Target = t,
				tvm => tvm.Target);
			Targets.DisposeWith(disposables);
			Disposable.Create(() => Targets = null).DisposeWith(disposables);
			Observable.FromAsync(CheckForUpdatesAsync).Subscribe().DisposeWith(disposables);
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
		ShowSettingsCommand = ReactiveCommand.Create(() =>
		{
			DialogHost.Show(new SettingsViewModel());
		});
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
	public ReadOnlyObservableMappedCollection<Target, ObservableCollection<Target>, TargetViewModel>? Targets
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
