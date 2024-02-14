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

	public ReactiveCommand<Unit, Unit> DismissDialogCommand
	{
		get;
	}
}
