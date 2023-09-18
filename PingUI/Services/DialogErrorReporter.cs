using System;
using PingUI.ServiceModels;
using PingUI.ViewModels;
using PingUI.Views;

namespace PingUI.Services;

/// <summary>
/// Displays errors in a separate window.
/// </summary>
public class DialogErrorReporter : IErrorReporter
{
	/// <inheritdoc />
	public void ReportError(string context, Exception exception)
	{
		new DialogErrorReporterView() { ViewModel = new DialogErrorReporterViewModel(context, exception) }.Show();
	}
}
