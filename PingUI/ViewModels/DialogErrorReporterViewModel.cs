using System;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface for displaying an error.
/// </summary>
public class DialogErrorReporterViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="DialogErrorReporterViewModel" />.
	/// </summary>
	/// <param name="context">A human readable string indicating what was happening when the error occurred.</param>
	/// <param name="exception">The error that occurred.</param>
	/// <exception cref="ArgumentNullException"><paramref name="context" /> or <paramref name="exception" /> is <see langword="null" />.</exception>
	public DialogErrorReporterViewModel(string context, Exception exception)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(exception);
		Context = context;
		Exception = exception;
	}

	/// <summary>
	/// Gets a human readable string indicating what was happening when the error occurred.
	/// </summary>
	public string Context
	{
		get;
	}

	/// <summary>
	/// Gets the error that occurred.
	/// </summary>
	public Exception Exception
	{
		get;
	}
}
