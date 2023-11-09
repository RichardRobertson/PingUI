using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using PingUI.ServiceModels;
using PingUI.ViewModels;
using PingUI.Views;
using ReactiveUI;

namespace PingUI.Services;

/// <summary>
/// Displays errors in a separate window.
/// </summary>
public class DialogErrorReporter : IErrorReporter
{
	/// <inheritdoc />
	public void ReportError(string context, Exception exception)
	{
#if DEBUG
		Debug.Fail(context, exception.ToString());
#endif
		RxApp.MainThreadScheduler.Schedule(() => new DialogErrorReporterView() { ViewModel = new DialogErrorReporterViewModel(context, exception) }.Show());
	}
}
