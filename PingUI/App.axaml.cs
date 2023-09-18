using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PingUI.ViewModels;
using PingUI.Views;

namespace PingUI;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow()
			{
				ViewModel = new MainWindowViewModel()
			};
		}
		base.OnFrameworkInitializationCompleted();
	}
}
