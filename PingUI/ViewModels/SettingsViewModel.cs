using System.Reactive;
using DialogHostAvalonia;
using PingUI.Extensions;
using PingUI.Interop;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
	private readonly IConfiguration configuration = Locator.Current.GetRequiredService<IConfiguration>();

	public SettingsViewModel()
	{
		DismissDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
	}

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

	public ReactiveCommand<Unit, Unit> DismissDialogCommand
	{
		get;
	}
}
