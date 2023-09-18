using System;
using System.Net.NetworkInformation;
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
