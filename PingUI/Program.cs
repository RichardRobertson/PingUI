using Avalonia;
using Avalonia.ReactiveUI;
using PingUI.Extensions;
using PingUI.I18N;
using PingUI.ServiceModels;
using PingUI.Services;
using ReactiveUI;
using Splat;
using System;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PingUI;

public static class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		var errorReporter = new DialogErrorReporter();
		Locator.CurrentMutable.RegisterConstant<IErrorReporter>(errorReporter);
		RxApp.DefaultExceptionHandler = Observer.Create<Exception>(exception => errorReporter.ReportError(Strings.Program_Error_Global, exception));
		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			errorReporter.ReportError(Strings.Program_Error_Global, e.Exception);
			e.SetObserved();
		};
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			errorReporter.ReportError(Strings.Program_Error_Global, e.ExceptionObject as Exception ?? new RuntimeWrappedException(e.ExceptionObject));
		Locator.CurrentMutable.RegisterConstant<IPinger>(new NetworkPinger());
		Locator.CurrentMutable.RegisterLazySingleton<IConfiguration>(LocalOrAppDataJsonConfiguration.LoadOrDefault);
		try
		{
			BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
		}
		catch (Exception exception)
		{
			errorReporter.ReportError(Strings.Program_Error_Global, exception);
		}
		Locator.Current.GetRequiredService<IConfiguration>().Save();
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();
	}
}
