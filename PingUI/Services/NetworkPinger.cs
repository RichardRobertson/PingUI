using System;
using System.Net.NetworkInformation;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using PingUI.Models;
using PingUI.ServiceModels;
using ReactiveUI;

namespace PingUI.Services;

/// <summary>
/// An <see cref="IPinger" /> that uses <see cref="Ping" /> internally.
/// </summary>
public class NetworkPinger : IPinger
{
	/// <summary>
	/// The standard ping timeout.
	/// </summary>
	private static readonly TimeSpan FiveSeconds = TimeSpan.FromSeconds(5);

	/// <inheritdoc />
	public IObservable<PingResult> GetObservable(Target target)
	{
		ArgumentNullException.ThrowIfNull(target);
		return Observable.Create<PingResult>(observer => RxApp.TaskpoolScheduler.ScheduleAsync(
			(target, observer),
			(_, state, cancellationToken) =>
				PingLoopAsync(state.target, state.observer, cancellationToken)
			)
		);
	}

	/// <summary>
	/// Asynchronously generates ping results.
	/// </summary>
	/// <param name="target">The <see cref="Target" /> to generate results for.</param>
	/// <param name="observer">The <see cref="IObserver{T}" /> of <see cref="PingResult" /> to notify.</param>
	/// <param name="cancellationToken">A <see cref="CancellationToken" /> to stop the task with.</param>
	/// <returns>A <see cref="Task" /> representing the asynchronous job.</returns>
	private static async Task PingLoopAsync(Target target, IObserver<PingResult> observer, CancellationToken cancellationToken)
	{
		try
		{
			var ping = new Ping();
			while (!cancellationToken.IsCancellationRequested)
			{
				var result = await ping.SendPingAsync(target.Address, FiveSeconds, null, null, cancellationToken).ConfigureAwait(true);
				RxApp.MainThreadScheduler.Schedule(() => observer.OnNext(new PingResult(result.Status, DateTime.Now)));
				await Task.Delay(target.CoolDown, cancellationToken).ConfigureAwait(true);
			}
			RxApp.MainThreadScheduler.Schedule(observer.OnCompleted);
		}
		catch (TaskCanceledException)
		{
			RxApp.MainThreadScheduler.Schedule(observer.OnCompleted);
		}
		catch (Exception error)
		{
			RxApp.MainThreadScheduler.Schedule(() => observer.OnError(error));
		}
	}
}
