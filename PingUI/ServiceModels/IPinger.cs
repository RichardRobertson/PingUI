using System;
using PingUI.Models;

namespace PingUI.ServiceModels;

/// <summary>
/// Represents a service that produces Reactive <see cref="PingResult" /> values.
/// </summary>
public interface IPinger
{
	/// <summary>
	/// Creates a Reactive Observable that represents ping results.
	/// </summary>
	/// <param name="target">The <see cref="Target" /> to ping.</param>
	/// <returns>An <see cref="IObservable{T}" /> of <see cref="PingResult" />.</returns>
	IObservable<PingResult> GetObservable(Target target);
}
