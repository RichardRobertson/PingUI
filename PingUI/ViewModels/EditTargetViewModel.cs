using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using DialogHostAvalonia;
using PingUI.I18N;
using PingUI.Models;
using ReactiveUI;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface to edit or create a <see cref="Target" />.
/// </summary>
public class EditTargetViewModel : ViewModelBase
{
	/// <summary>
	/// Backing store for <see cref="Address" />.
	/// </summary>
	private string? _Address;

	/// <summary>
	/// Backing store for <see cref="Label" />.
	/// </summary>
	private string? _Label;

	/// <summary>
	/// Backing store for <see cref="Hours" />.
	/// </summary>
	private int? _Hours;

	/// <summary>
	/// Backing store for <see cref="Minutes" />.
	/// </summary>
	private int? _Minutes;

	/// <summary>
	/// Backing store for <see cref="Seconds" />.
	/// </summary>
	private int? _Seconds;

	/// <summary>
	/// Backing store for <see cref="IsZeroCoolDown" />.
	/// </summary>
	private bool _IsZeroCoolDown;

	/// <summary>
	/// Initializes a new <see cref="EditTargetViewModel" />.
	/// </summary>
	/// <param name="target">The <see cref="Target" /> to edit or <see langword="null" /> to create a new one.</param>
	public EditTargetViewModel(Target? target)
	{
		this.ValidationRule(vm => vm.Address, address => !string.IsNullOrWhiteSpace(address), Strings.EditTargetView_IPAddressBlank);
		this.ValidationRule(vm => vm.Address, address => string.IsNullOrWhiteSpace(address) || IPAddress.TryParse(address, out var _), Strings.EditTargetView_IPAddressParseError);
		this.ValidationRule(
			this.WhenAnyValue(
				vm => vm.Hours,
				vm => vm.Minutes,
				vm => vm.Seconds,
				(hours, minutes, seconds) => (hours ?? 0) + (minutes ?? 0) + (seconds ?? 0) != 0)
				.Do(isNotZeroCoolDown => IsZeroCoolDown = !isNotZeroCoolDown),
			Strings.EditTargetView_ZeroCoolDownError);
		IsNewTarget = target is null;
		Address = target?.Address.ToString();
		Label = target?.Label;
		Hours = target?.CoolDown.Hours ?? 0;
		Minutes = target?.CoolDown.Minutes ?? 0;
		Seconds = target?.CoolDown.Seconds ?? 0;
		CancelDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
		AcceptDialogCommand = ReactiveCommand.Create(
			() =>
				DialogHost.GetDialogSession(null)
					?.Close(
						new Target(
							IPAddress.Parse(Address!),
							Label,
							new TimeSpan(_Hours ?? 0, _Minutes ?? 0, _Seconds ?? 0))),
			ValidationContext.Valid);
	}

	/// <summary>
	/// The address text that can be parsed to produce an <see cref="IPAddress" />.
	/// </summary>
	/// <value>A <see cref="string" /> that should be resolved to an <see cref="IPAddress" /> or <see langword="null" /> if not specified.</value>
	public string? Address
	{
		get => _Address;
		set => this.RaiseAndSetIfChanged(ref _Address, value);
	}

	/// <summary>
	/// A human readable label for the <see cref="Target" />.
	/// </summary>
	/// <value>A <see cref="string" /> or <see langword="null" />.</value>
	public string? Label
	{
		get => _Label;
		set => this.RaiseAndSetIfChanged(ref _Label, value);
	}

	/// <summary>
	/// The number of hours to set in the ping cool down.
	/// </summary>
	/// <value>An integer that should be from 0 and 23 inclusive or <see langword="null" />.</value>
	public int? Hours
	{
		get => _Hours;
		set => this.RaiseAndSetIfChanged(ref _Hours, value);
	}

	/// <summary>
	/// The number of minutes to set in the ping cool down.
	/// </summary>
	/// <value>An integer that should be from 0 and 59 inclusive or <see langword="null" />.</value>
	public int? Minutes
	{
		get => _Minutes;
		set => this.RaiseAndSetIfChanged(ref _Minutes, value);
	}

	/// <summary>
	/// The number of seconds to set in the ping cool down.
	/// </summary>
	/// <value>An integer that should be from 0 and 59 inclusive or <see langword="null" />.</value>
	public int? Seconds
	{
		get => _Seconds;
		set => this.RaiseAndSetIfChanged(ref _Seconds, value);
	}

	/// <summary>
	/// Gets a value indicating whether the hours, minutes, and seconds fields are all zero.
	/// </summary>
	/// <value><see langword="true" /> if <see cref="Hours" /> + <see cref="Minutes" /> + <see cref="Seconds" /> == 0; otherwise <see langword="false" />.</value>
	public bool IsZeroCoolDown
	{
		get => _IsZeroCoolDown;
		private set => this.RaiseAndSetIfChanged(ref _IsZeroCoolDown, value);
	}

	/// <summary>
	/// Gets a value indicating whether a target is being edited or created.
	/// </summary>
	/// <value><see langword="true" /> if the interface should use wording for a new target; <see langword="false" /> if the interface should use wording to edit an existing target.</value>
	public bool IsNewTarget
	{
		get;
	}

	/// <summary>
	/// A command to cancel the editing or creation process.
	/// </summary>
	public ReactiveCommand<Unit, Unit> CancelDialogCommand
	{
		get;
	}

	/// <summary>
	/// A command to accept the editing or creation process.
	/// </summary>
	public ReactiveCommand<Unit, Unit> AcceptDialogCommand
	{
		get;
	}
}
