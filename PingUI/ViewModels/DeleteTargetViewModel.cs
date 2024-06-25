using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using DialogHostAvalonia;
using PingUI.Extensions;
using PingUI.Models;
using ReactiveUI;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface confirming deletion of a <see cref="Models.Target" />.
/// </summary>
public class DeleteTargetViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="DeleteTargetViewModel" />.
	/// </summary>
	/// <param name="target">The <see cref="Models.Target" /> to confirm deletion of.</param>
	/// <exception cref="ArgumentNullException"><paramref name="target" /> is <see langword="null" /></exception>
	public DeleteTargetViewModel(Target target)
	{
		ArgumentNullException.ThrowIfNull(target);
		Target = target;
		CoolDown = target.CoolDown.ToWords();
		Tags = target.Tags.Select(tag => new TargetTagViewModel(new TargetTag(tag, false)));
		CancelDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
		AcceptDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close(true));
	}

	/// <summary>
	/// The <see cref="Models.Target" /> to confirm deletion of.
	/// </summary>
	public Target Target
	{
		get;
	}

	/// <summary>
	/// A text representation of the cool down specified on <see cref="Target" />.
	/// </summary>
	public string CoolDown
	{
		get;
	}

	/// <summary>
	/// The tags specified on <see cref="Target" />.
	/// </summary>
	public IEnumerable<TargetTagViewModel> Tags
	{
		get;
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
