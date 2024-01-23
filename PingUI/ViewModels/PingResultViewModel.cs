using System;
using System.Net.NetworkInformation;
using PingUI.I18N;
using PingUI.Models;

namespace PingUI.ViewModels;

/// <summary>
/// Represents an interface to display a <see cref="PingResult" />.
/// </summary>
public class PingResultViewModel : ViewModelBase
{
	/// <summary>
	/// Initializes a new <see cref="PingResultViewModel" />.
	/// </summary>
	/// <param name="pingResult">The result to display.</param>
	public PingResultViewModel(PingResult pingResult)
	{
		ArgumentNullException.ThrowIfNull(pingResult);
		Status = pingResult.Status;
		Timestamp = pingResult.Timestamp;
		IsSuccess = pingResult.Status == IPStatus.Success;
		IsFailure = pingResult.Status != IPStatus.Unknown && pingResult.Status != IPStatus.Success;
		IsUnknown = pingResult.Status == IPStatus.Unknown;
	}

	/// <summary>
	/// Gets the status of the ping.
	/// </summary>
	public IPStatus Status
	{
		get;
	}

	public string LocalizedStatus
	{
		get
		{
			return Status switch
			{
				IPStatus.Success => Strings.System_Net_NetworkInformation_IPStatus_Success,
				IPStatus.DestinationNetworkUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationNetworkUnreachable,
				IPStatus.DestinationHostUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationHostUnreachable,
				// DestinationProhibited and DestinationProtocolUnreachable have the same value
				// DestinationProtocolUnreachable is for IPv4 and DestinationProhibited is for IPv6
				// IPStatus.DestinationProhibited => Strings.System_Net_NetworkInformation_IPStatus_DestinationProhibited,
				IPStatus.DestinationProtocolUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationProtocolUnreachable,
				IPStatus.DestinationPortUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationPortUnreachable,
				IPStatus.NoResources => Strings.System_Net_NetworkInformation_IPStatus_NoResources,
				IPStatus.BadOption => Strings.System_Net_NetworkInformation_IPStatus_BadOption,
				IPStatus.HardwareError => Strings.System_Net_NetworkInformation_IPStatus_HardwareError,
				IPStatus.PacketTooBig => Strings.System_Net_NetworkInformation_IPStatus_PacketTooBig,
				IPStatus.TimedOut => Strings.System_Net_NetworkInformation_IPStatus_TimedOut,
				IPStatus.BadRoute => Strings.System_Net_NetworkInformation_IPStatus_BadRoute,
				IPStatus.TtlExpired => Strings.System_Net_NetworkInformation_IPStatus_TtlExpired,
				IPStatus.TtlReassemblyTimeExceeded => Strings.System_Net_NetworkInformation_IPStatus_TtlReassemblyTimeExceeded,
				IPStatus.ParameterProblem => Strings.System_Net_NetworkInformation_IPStatus_ParameterProblem,
				IPStatus.SourceQuench => Strings.System_Net_NetworkInformation_IPStatus_SourceQuench,
				IPStatus.BadDestination => Strings.System_Net_NetworkInformation_IPStatus_BadDestination,
				IPStatus.DestinationUnreachable => Strings.System_Net_NetworkInformation_IPStatus_DestinationUnreachable,
				IPStatus.TimeExceeded => Strings.System_Net_NetworkInformation_IPStatus_TimeExceeded,
				IPStatus.BadHeader => Strings.System_Net_NetworkInformation_IPStatus_BadHeader,
				IPStatus.UnrecognizedNextHeader => Strings.System_Net_NetworkInformation_IPStatus_UnrecognizedNextHeader,
				IPStatus.IcmpError => Strings.System_Net_NetworkInformation_IPStatus_IcmpError,
				IPStatus.DestinationScopeMismatch => Strings.System_Net_NetworkInformation_IPStatus_DestinationScopeMismatch,
				IPStatus.Unknown => Strings.System_Net_NetworkInformation_IPStatus_Unknown,
				_ => Strings.System_Net_NetworkInformation_IPStatus_Unknown,
			};
		}
	}

	/// <summary>
	/// Gets the time the ping result was reported.
	/// </summary>
	public DateTime Timestamp
	{
		get;
	}

	/// <summary>
	/// Gets a value indicating whether the ping was successful.
	/// </summary>
	/// <value><see langword="true" /> if <see cref="Status" /> is equal to <see cref="IPStatus.Success" />; otherwise <see langword="false" />.</value>
	public bool IsSuccess
	{
		get;
	}

	/// <summary>
	/// Gets a value indicating whether the ping failed.
	/// </summary>
	/// <value><see langword="true" /> if <see cref="Status" /> is not equal to <see cref="IPStatus.Success" /> or <see cref="IPStatus.Unknown" />; otherwise <see langword="false" />.</value>
	public bool IsFailure
	{
		get;
	}

	/// <summary>
	/// Gets a value indicating whether the ping status is unknown.
	/// </summary>
	/// <value><see langword="true" /> if <see cref="Status" /> is equal to <see cref="IPStatus.Unknown" />; otherwise <see langword="false" />.</value>
	public bool IsUnknown
	{
		get;
	}
}
