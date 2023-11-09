using System.Diagnostics;
using System.Reactive;
using DialogHostAvalonia;
using ReactiveUI;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface to display updates.
/// </summary>
public class UpdateNotificationViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="UpdateNotificationViewModel" />.
	/// </summary>
	/// <param name="releaseUri">The Uri to open in the browser if the user wishes to download the update.</param>
	/// <param name="releaseBodyMarkdown">The Markdown text from the changelog body.</param>
	public UpdateNotificationViewModel(string releaseUri, string releaseBodyMarkdown)
	{
		ReleaseUri = releaseUri;
		ReleaseBodyMarkdown = releaseBodyMarkdown;
		OpenReleaseWebpageCommand = ReactiveCommand.Create(
			() =>
			{
				new Process()
				{
					StartInfo = new ProcessStartInfo()
					{
						FileName = releaseUri,
						UseShellExecute = true,
					},
				}
					.Start();
				DialogHost.GetDialogSession(null)?.Close();
			});
		DismissDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
	}

	/// <summary>
	/// Gets the Uri to open in the browser.
	/// </summary>
	public string ReleaseUri
	{
		get;
	}

	/// <summary>
	/// Gets the Markdown text from the changelog.
	/// </summary>
	public string ReleaseBodyMarkdown
	{
		get;
	}

	/// <summary>
	/// A command to open <see cref="ReleaseUri" /> in the user's browser.
	/// </summary>
	public ReactiveCommand<Unit, Unit> OpenReleaseWebpageCommand
	{
		get;
	}

	/// <summary>
	/// A command to cancel the dialog.
	/// </summary>
	public ReactiveCommand<Unit, Unit> DismissDialogCommand
	{
		get;
	}
}
