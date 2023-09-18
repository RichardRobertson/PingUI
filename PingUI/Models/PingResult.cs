using System;
using System.Net.NetworkInformation;

namespace PingUI.Models;

/// <summary>
/// An <see cref="IPStatus" /> and <see cref="DateTime" /> tuple.
/// </summary>
/// <param name="Status">The result of the ping.</param>
/// <param name="Timestamp">The time of the result.</param>
public record PingResult(IPStatus Status, DateTime Timestamp);
