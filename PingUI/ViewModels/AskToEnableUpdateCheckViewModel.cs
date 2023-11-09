using System.Reactive;
using DialogHostAvalonia;
using ReactiveUI;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface to ask the user if they wish to check for updates.
/// </summary>
public class AskToEnableUpdateCheckViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="AskToEnableUpdateCheckViewModel" />;
	/// </summary>
	public AskToEnableUpdateCheckViewModel()
	{
		YesDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close(true));
		NoDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close(false));
	}

	/// <summary>
	/// A command to enable update checks.
	/// </summary>
	public ReactiveCommand<Unit, Unit> YesDialogCommand
	{
		get;
	}

	/// <summary>
	/// A command to disable update checks.
	/// </summary>
	public ReactiveCommand<Unit, Unit> NoDialogCommand
	{
		get;
	}
}
