using System.Reactive;
using DialogHostAvalonia;
using PingUI.Models;
using ReactiveUI;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface confirming deletion of all <see cref="Target" />s.
/// </summary>
public class DeleteAllTargetsViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="DeleteTargetViewModel" />.
	/// </summary>
	public DeleteAllTargetsViewModel()
	{
		CancelDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
		AcceptDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close(true));
	}

	/// <summary>
	/// A command to cancel the deletion.
	/// </summary>
	public ReactiveCommand<Unit, Unit> CancelDialogCommand
	{
		get;
	}

	/// <summary>
	/// A command to accept the deletion.
	/// </summary>
	public ReactiveCommand<Unit, Unit> AcceptDialogCommand
	{
		get;
	}
}
