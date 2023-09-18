using System;
using System.Net;
using System.Text.Json.Serialization;
using PingUI.JsonConverters;

namespace PingUI.Models;

/// <summary>
/// Represents a network address to ping with optional name and specific cool down between pings.
/// </summary>
public record Target
{
	/// <summary>
	/// Initializes a new <see cref="Target" /> with the specified properties.
	/// </summary>
	/// <param name="address">The <see cref="IPAddress" /> to ping.</param>
	/// <param name="label">An optional human readable label.</param>
	/// <param name="coolDown">The time to wait between pings.</param>
	/// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" /></exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="coolDown" /> is less than <see cref="TimeSpan.Zero" /> or greater than one day.</exception>
	public Target(IPAddress address, string? label, TimeSpan coolDown)
	{
		ArgumentNullException.ThrowIfNull(address);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(coolDown, TimeSpan.Zero);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(coolDown, TimeSpan.FromDays(1));
		Address = address;
		Label = label;
		CoolDown = coolDown;
	}

	/// <summary>
	/// Gets the <see cref="IPAddress" /> to ping.
	/// </summary>
	/// <value>An <see cref="IPAddress" />.</value>
	[JsonConverter(typeof(IPAddressJsonConverter))]
	public IPAddress Address
	{
		get;
		init;
	}

	/// <summary>
	/// Gets the human readable label for this <see cref="Target" />.
	/// </summary>
	/// <value>A <see cref="string" /> or <see langword="null" /> if not specified.</value>
	public string? Label
	{
		get;
		init;
	}

	/// <summary>
	/// Gets the cool down between pings.
	/// </summary>
	/// <value>A <see cref="TimeSpan" /> that is greater than zero but less than a day.</value>
	public TimeSpan CoolDown
	{
		get;
		init;
	}
}
