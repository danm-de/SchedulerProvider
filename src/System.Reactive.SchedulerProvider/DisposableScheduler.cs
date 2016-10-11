using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;

namespace System.Reactive
{
	internal static class DisposableScheduler
	{
		[DebuggerStepThrough]
		public static IDisposableScheduler Create<TScheduler>(TScheduler scheduler, Action<TScheduler> disposeAction)
			where TScheduler : IScheduler {
			return new DisposableScheduler<TScheduler>(scheduler, disposeAction);
		}
	}

	[DebuggerDisplay("{_scheduler}")]
	internal sealed class DisposableScheduler<TScheduler> : IDisposableScheduler
		where TScheduler : IScheduler
	{
		private const int NOT_DISPOSED = 0;
		private const int DISPOSED = 1;
		private readonly TScheduler _scheduler;
		private readonly Action<TScheduler> _disposeAction;

		private int _state;

		[DebuggerStepThrough]
		public DisposableScheduler(TScheduler scheduler, Action<TScheduler> disposeAction) {
			if (scheduler == null) {
				throw new ArgumentNullException(nameof(scheduler));
			}
			if (disposeAction == null) {
				throw new ArgumentNullException(nameof(disposeAction));
			}

			_scheduler = scheduler;
			_disposeAction = disposeAction;
		}

		/// <summary>
		/// Schedules an action to be executed.
		/// </summary>
		/// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
		/// <param name="state">State passed to the action to be executed.</param>
		/// <param name="action">Action to be executed.</param>
		/// <returns>
		/// The disposable object used to cancel the scheduled action (best effort).
		/// </returns>
		[DebuggerStepThrough]
		public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) {
			return _scheduler.Schedule(state, action);
		}

		/// <summary>
		/// Schedules an action to be executed after dueTime.
		/// </summary>
		/// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
		/// <param name="state">State passed to the action to be executed.</param>
		/// <param name="action">Action to be executed.</param>
		/// <param name="dueTime">Relative time after which to execute the action.</param>
		/// <returns>
		/// The disposable object used to cancel the scheduled action (best effort).
		/// </returns>
		[DebuggerStepThrough]
		public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
			return _scheduler.Schedule(state, dueTime, action);
		}

		/// <summary>
		/// Schedules an action to be executed at dueTime.
		/// </summary>
		/// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam><param name="state">State passed to the action to be executed.</param><param name="action">Action to be executed.</param><param name="dueTime">Absolute time at which to execute the action.</param>
		/// <returns>
		/// The disposable object used to cancel the scheduled action (best effort).
		/// </returns>
		[DebuggerStepThrough]
		public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) {
			return _scheduler.Schedule(state, dueTime, action);
		}

		/// <summary>
		/// Gets the scheduler's notion of current time.
		/// </summary>
		public DateTimeOffset Now {
			[DebuggerStepThrough]
			get { return _scheduler.Now; }
		}

		/// <summary>
		/// Disposes instance
		/// </summary>
		/// <filterpriority>2</filterpriority>
		[DebuggerStepThrough]
		public void Dispose() {
			Dispose(true);
		}

		[DebuggerStepThrough]
		private void Dispose(bool disposing) {
			if (!disposing) {
				return;
			}

			var currentState = Interlocked.CompareExchange(
				ref _state,
				DISPOSED,
				NOT_DISPOSED);

			if (currentState == DISPOSED) {
				return;
			}

			try {
				_disposeAction(_scheduler);
			} catch {
				_state = NOT_DISPOSED;
				throw;
			}
		}
	}
}