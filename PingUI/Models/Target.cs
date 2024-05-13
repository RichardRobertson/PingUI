using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	/// Backing store for <see cref="GetHashCode" />.
	/// </summary>
	private readonly Lazy<int> hashCode;

	/// <summary>
	/// Initializes a new <see cref="Target" /> with the specified properties.
	/// </summary>
	/// <param name="address">The <see cref="IPAddress" /> to ping.</param>
	/// <param name="label">An optional human readable label.</param>
	/// <param name="coolDown">The time to wait between pings.</param>
	/// <param name="tags">A set of <see cref="string" /> tags or <see langword="null" />.</param>
	/// <exception cref="ArgumentNullException"><paramref name="address" /> is <see langword="null" /></exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="coolDown" /> is less than <see cref="TimeSpan.Zero" /> or greater than one day.</exception>
	public Target(IPAddress address, string? label, TimeSpan coolDown, ImmutableSortedSet<string>? tags)
	{
		ArgumentNullException.ThrowIfNull(address);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(coolDown, TimeSpan.Zero);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(coolDown, TimeSpan.FromDays(1));
		Address = address;
		Label = label;
		CoolDown = coolDown;
		Tags = tags ?? ImmutableSortedSet<string>.Empty;
		hashCode = new Lazy<int>(() =>
		{
			var hash = new HashCode();
			hash.Add(Address);
			hash.Add(Label);
			hash.Add(CoolDown);
			foreach (var tag in Tags)
			{
				hash.Add(tag);
			}
			return hash.ToHashCode();
		});
	}

	/// <summary>
	/// Gets the <see cref="IPAddress" /> to ping.
	/// </summary>
	/// <value>An <see cref="IPAddress" />.</value>
	[JsonConverter(typeof(IPAddressJsonConverter))]
	public IPAddress Address
	{
		get;
	}

	/// <summary>
	/// Gets the human readable label for this <see cref="Target" />.
	/// </summary>
	/// <value>A <see cref="string" /> or <see langword="null" /> if not specified.</value>
	public string? Label
	{
		get;
	}

	/// <summary>
	/// Gets the cool down between pings.
	/// </summary>
	/// <value>A <see cref="TimeSpan" /> that is greater than zero but less than a day.</value>
	public TimeSpan CoolDown
	{
		get;
	}

	/// <summary>
	/// Gets the collection of tags on this <see cref="Target" />.
	/// </summary>
	/// <value>An <see cref="ImmutableSortedSet{T}" /> of <see cref="string" />.</value>
	public ImmutableSortedSet<string> Tags
	{
		get;
	}

	/// <inheritdoc />
	public virtual bool Equals(Target? other)
	{
		return other is not null
			&& hashCode.Value == other.hashCode.Value
			&& Address == other.Address
			&& Label == other.Label
			&& CoolDown == other.CoolDown
			&& Tags.SetEquals(other.Tags);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return hashCode.Value;
	}
}
