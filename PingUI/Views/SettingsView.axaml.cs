using Avalonia.ReactiveUI;
using PingUI.ViewModels;

namespace PingUI.Views;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
	public SettingsView()
	{
		InitializeComponent();
	}
}
