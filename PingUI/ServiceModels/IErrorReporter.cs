using System;

namespace PingUI.ServiceModels;

/// <summary>
/// Represents an error reporting system.
/// </summary>
public interface IErrorReporter
{
	/// <summary>
	/// Report an error.
	/// </summary>
	/// <param name="context">A human readable string describing what was happening when the error occurred.</param>
	/// <param name="exception">The error to display.</param>
	void ReportError(string context, Exception exception);
}
