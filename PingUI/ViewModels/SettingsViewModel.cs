using PingUI.Extensions;
using PingUI.ServiceModels;
using ReactiveUI;
using Splat;

namespace PingUI.ViewModels;

public class SettingsViewModel : ViewModelBase
{
	private readonly IConfiguration configuration = Locator.Current.GetRequiredService<IConfiguration>();

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
}
