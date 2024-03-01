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

#if DEBUG
/// <summary>
/// An <see cref="IPinger" /> for testing UI changes that produces alternating results of <see cref="IPStatus.Success" /> and <see cref="IPStatus.TimedOut" />.
/// </summary>
/// <param name="repeats">How many times to repeat a state before toggling.</param>
public class FakeTogglePinger(int repeats = 1) : IPinger
{
	/// <summary>
	/// Counts how many times a status has been reported.
	/// </summary>
	private volatile int _Count;

	/// <summary>
	/// Lock marker for <see cref="_Count" />.
	/// </summary>
	private readonly object _CountLock = new();

	/// <summary>
	/// Gets a value indicating how many times to repeat a state before toggling.
	/// </summary>
	public int Repeats
	{
		get;
	} = repeats;

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
	private async Task PingLoopAsync(Target target, IObserver<PingResult> observer, CancellationToken cancellationToken)
	{
		try
		{
			var status = IPStatus.Success;
			while (!cancellationToken.IsCancellationRequested)
			{
				observer.OnNext(new PingResult(status, DateTime.Now));
				await Task.Delay(target.CoolDown, cancellationToken).ConfigureAwait(false);
				lock (_CountLock)
				{
					_Count++;
					if (_Count == Repeats)
					{
						status = status == IPStatus.Success ? IPStatus.TimedOut : IPStatus.Success;
						_Count = 0;
					}
				}
			}
		}
		catch (TaskCanceledException)
		{
			observer.OnCompleted();
		}
		catch (Exception error)
		{
			observer.OnError(error);
		}
	}
}
#endif
