using System.Reactive;
using DialogHostAvalonia;
using PingUI.Extensions;
using PingUI.Interop;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface to display user settings.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
	/// <summary>
	/// Convenient reference to configuration.
	/// </summary>
	private readonly IConfiguration configuration = Locator.Current.GetRequiredService<IConfiguration>();

	/// <summary>
	/// Initializes a new <see cref="SettingsViewModel" />.
	/// </summary>
	public SettingsViewModel()
	{
		DismissDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
	}

	/// <summary>
	/// Indicates whether the app should check for updates on startup.
	/// </summary>
	public bool CheckOnlineForUpdates
	{
		get => configuration.CheckOnlineForUpdates ?? false;
		set
		{
			if (value != configuration.CheckOnlineForUpdates)
			{
				this.RaisePropertyChanging();
				configuration.CheckOnlineForUpdates = value;
				this.RaisePropertyChanged();
			}
		}
	}

	/// <summary>
	/// Indicates whether the app should remember and restore the window location on startup.
	/// </summary>
	public bool RememberWindowLocation
	{
		get => configuration.WindowBounds is not null;
		set
		{
			if (value != (configuration.WindowBounds is not null))
			{
				this.RaisePropertyChanging();
				configuration.WindowBounds = value ? new WindowPlacement() : null;
				this.RaisePropertyChanged();
			}
		}
	}

	/// <summary>
	/// A command to close the settings dialog.
	/// </summary>
	public ReactiveCommand<Unit, Unit> DismissDialogCommand
	{
		get;
	}
}
