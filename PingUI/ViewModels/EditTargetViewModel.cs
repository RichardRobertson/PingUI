using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using DialogHostAvalonia;
using DynamicData.Binding;
using PingUI.I18N;
using PingUI.Models;
using ReactiveUI;
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
	/// Backing store for <see cref="IPSuggestions" />.
	/// </summary>
	private readonly ObservableCollectionExtended<IPAddress> _IPSuggestions;

	/// <summary>
	/// Backing store for <see cref="IPResolutionState" />.
	/// </summary>
	private DnsState _IPResolutionState;

	/// <summary>
	/// Initializes a new <see cref="EditTargetViewModel" />.
	/// </summary>
	/// <param name="target">The <see cref="Target" /> to edit or <see langword="null" /> to create a new one.</param>
	public EditTargetViewModel(Target? target)
	{
		IsNewTarget = target is null;
		Address = target?.Address.ToString();
		Label = target?.Label;
		Hours = target?.CoolDown.Hours ?? 0;
		Minutes = target?.CoolDown.Minutes ?? 0;
		Seconds = target?.CoolDown.Seconds ?? 0;
		Tags = new ObservableCollectionExtended<string>(target?.Tags ?? Enumerable.Empty<string>());
		CancelDialogCommand = ReactiveCommand.Create(() => DialogHost.GetDialogSession(null)?.Close());
		AcceptDialogCommand = ReactiveCommand.Create(
			() =>
				DialogHost.GetDialogSession(null)
					?.Close(
						new Target(
							IPAddress.Parse(Address!),
							Label,
							new TimeSpan(_Hours ?? 0, _Minutes ?? 0, _Seconds ?? 0),
							Tags.ToImmutableSortedSet())),
			ValidationContext.Valid);
		_IPSuggestions = [];
		IPSuggestions = new ReadOnlyObservableCollection<IPAddress>(_IPSuggestions);
		ApplyIPSuggestionCommand = ReactiveCommand.Create<IPAddress>(ipAddress =>
		{
			Label = Address;
			Address = ipAddress.ToString();
			_IPSuggestions.Clear();
			IPResolutionState = DnsState.None;
		});
		this.WhenAnyValue(vm => vm.Address)
			.Throttle(TimeSpan.FromMilliseconds(500))
			.Select(name => Observable.FromAsync(async (cancellationToken) =>
			{
				_IPSuggestions.Clear();
				IPResolutionState = DnsState.None;
				if (Uri.CheckHostName(name) == UriHostNameType.Dns)
				{
					IPResolutionState = DnsState.Resolving;
					try
					{
						var entry = await Dns.GetHostEntryAsync(name!, cancellationToken).ConfigureAwait(false);
						_IPSuggestions.AddRange(entry.AddressList.OrderBy(ip => ip.AddressFamily));
					}
					catch
					{
						// catch block intentionally empty
					}
					IPResolutionState = _IPSuggestions.Count == 0 ? DnsState.Failure : DnsState.Success;
				}
			}))
			.Concat()
			.Subscribe();
		this.ValidationRule(
			vm => vm.Address,
			this.WhenAnyValue(
				vm => vm.Address,
				vm => vm.IPResolutionState,
				(address, state) => !(Uri.CheckHostName(address) == UriHostNameType.Dns && state == DnsState.Success)),
			Strings.EditTargetView_AddressSelectIP);
		this.ValidationRule(
			vm => vm.Address,
			this.WhenAnyValue(
				vm => vm.Address,
				vm => vm.IPResolutionState,
				(address, state) => !(Uri.CheckHostName(address) == UriHostNameType.Dns && state == DnsState.Resolving)),
			Strings.EditTargetView_AddressChecking);
		this.ValidationRule(vm => vm.Address, address => !string.IsNullOrWhiteSpace(address), Strings.EditTargetView_AddressBlank);
		this.ValidationRule(
			vm => vm.Address,
			this.WhenAnyValue(
				vm => vm.Address,
				vm => vm.IPResolutionState,
				(address, state) =>
				{
					if (string.IsNullOrWhiteSpace(address))
					{
						return true;
					}
					return Uri.CheckHostName(address) switch
					{
						UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6 => state != DnsState.Failure,
						_ => false,
					};
				}),
			Strings.EditTargetView_AddressParseError);
		this.ValidationRule(
			this.WhenAnyValue(
				vm => vm.Hours,
				vm => vm.Minutes,
				vm => vm.Seconds,
				(hours, minutes, seconds) => (hours ?? 0) + (minutes ?? 0) + (seconds ?? 0) != 0)
				.Do(isNotZeroCoolDown => IsZeroCoolDown = !isNotZeroCoolDown),
			Strings.EditTargetView_ZeroCoolDownError);
	}

	/// <summary>
	/// Contains a list indicating the resolved IP addresses of a host name entered into <see cref="Address" />.
	/// </summary>
	public ReadOnlyObservableCollection<IPAddress> IPSuggestions
	{
		get;
	}

	/// <summary>
	/// Indicates the state of the DNS resolution process.
	/// </summary>
	public DnsState IPResolutionState
	{
		get => _IPResolutionState;
		private set => this.RaiseAndSetIfChanged(ref _IPResolutionState, value);
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
	/// The tags to associate with this target.
	/// </summary>
	/// <value>A collection of zero or more strings.</value>
	public ObservableCollectionExtended<string> Tags
	{
		get;
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

	/// <summary>
	/// A command to apply one of the suggested IP addresses.
	/// </summary>
	public ReactiveCommand<IPAddress, Unit> ApplyIPSuggestionCommand
	{
		get;
	}

	/// <summary>
	/// Indicates the state of the DNS resolution process.
	/// </summary>
	public enum DnsState
	{
		/// <summary>
		/// No process running.
		/// </summary>
		None,

		/// <summary>
		/// Currently running.
		/// </summary>
		Resolving,

		/// <summary>
		/// Resolution success.
		/// </summary>
		Success,

		/// <summary>
		/// Resolution failure.
		/// </summary>
		Failure,
	}
}
