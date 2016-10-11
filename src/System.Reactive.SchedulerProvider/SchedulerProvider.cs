using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive
{
	/// <summary>
	/// Default scheduler provider implementation
	/// </summary>
	public class SchedulerProvider : ISchedulerProvider
	{
		private static readonly Lazy<ISchedulerProvider> _instance = new Lazy<ISchedulerProvider>(() => new SchedulerProvider());

		/// <summary>
		/// Instance singleton
		/// </summary>
		public static ISchedulerProvider Instance {
			[DebuggerStepThrough]
			get { return _instance.Value; }
		}

		/// <summary>
		/// Represents an object that schedules units of work on the current thread.
		/// </summary>
		public virtual IScheduler CurrentThread {
			[DebuggerStepThrough]
			get { return CurrentThreadScheduler.Instance; }
		}

		/// <summary>
		/// Represents an object that schedules units of work to run immediately on the current thread.
		/// </summary>
		public virtual IScheduler Immediate {
			[DebuggerStepThrough]
			get { return ImmediateScheduler.Instance; }
		}

		/// <summary>
		/// Represents an object that schedules each unit of work on a separate thread.
		/// </summary>
		public virtual IScheduler NewThread {
			[DebuggerStepThrough]
			get { return NewThreadScheduler.Default; }
		}

		/// <summary>
		/// Represents an object that schedules units of work on the CLR thread pool.
		/// </summary>
		public virtual IScheduler ThreadPool {
			[DebuggerStepThrough]
			get { return ThreadPoolScheduler.Instance; }
		}

		/// <summary>
		/// Represents an object that schedules units of work on the Task Parallel Library (TPL) task pool.
		/// </summary>
		public virtual IScheduler TaskPool {
			[DebuggerStepThrough]
			get { return TaskPoolScheduler.Default; }
		}

		/// <summary>
		/// Represents an object that schedules units of work on the platform's default scheduler.
		/// </summary>
		public virtual IScheduler Default {
			[DebuggerStepThrough]
			get { return DefaultScheduler.Instance; }
		}

		/// <summary>
		/// Creates an object that schedules each unit of work on a separate named thread.
		/// </summary>
		/// <param name="threadName"></param>
		/// <returns>A scheduler that schedules each unit of work on a separate named thread.</returns>
		[DebuggerStepThrough]
		public virtual IScheduler NewNamedThread(string threadName) {
			return new NewThreadScheduler(start => new Thread(start) {
				IsBackground = true,
				Name = threadName
			});
		}

		/// <summary>
		/// Represents an object that schedules each unit of work on a separate thread.
		/// </summary>
		/// <param name="threadFactory">Factory function for thread creation.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> is null.</exception>
		/// <returns>A scheduler that schedules each unit of work on a separate thread.</returns>
		[DebuggerStepThrough]
		public virtual IScheduler NewNamedThread(Func<ThreadStart, Thread> threadFactory) {
			return new NewThreadScheduler(threadFactory);
		}

		/// <summary>
		/// Represents an object that schedules units of work on a designated thread.
		/// </summary>
		/// <returns>A scheduler that schedules units of work on a designated thread.</returns>
		[DebuggerStepThrough]
		public virtual IDisposableScheduler NewEventLoop() {
			var scheduler = new EventLoopScheduler();
			return DisposableScheduler.Create(scheduler, ScheduleDisposeAction);
		}

		/// <summary>
		/// Represents an object that schedules units of work on a designated thread.
		/// </summary>
		/// <param name="threadFactory">Factory function for creation of the designated thread.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> is null.</exception>
		/// <returns>A scheduler that schedules units of work on a designated thread.</returns>
		[DebuggerStepThrough]
		public virtual IDisposableScheduler NewEventLoop(Func<ThreadStart, Thread> threadFactory) {
			var scheduler = new EventLoopScheduler(threadFactory);
			return DisposableScheduler.Create(scheduler, ScheduleDisposeAction);
		}

		/// <summary>
		/// Represents an object that schedules units of work on a designated named thread.
		/// </summary>
		/// <param name="threadName">Name of the designated thread</param>
		/// <returns>A scheduler that schedules units of work on a designated named thread.</returns>
		[DebuggerStepThrough]
		public virtual IDisposableScheduler NewEventLoop(string threadName) {
			var scheduler = new EventLoopScheduler(start => new Thread(start) {
				IsBackground = true,
				Name = threadName
			});
			return DisposableScheduler.Create(scheduler, ScheduleDisposeAction);
		}

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="threadName">Name of the designated thread</param>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		[DebuggerStepThrough]
		public virtual IObservable<T> UsingEventLoop<T>(string threadName, Func<IScheduler, IObservable<T>> observableFactory) {
			return Observable.Using(
				() => NewEventLoop(threadName),
				scheduler => observableFactory(scheduler));
		}

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="threadFactory">Factory function for creation of the designated thread.</param>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> or <paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		[DebuggerStepThrough]
		public virtual IObservable<T> UsingEventLoop<T>(Func<ThreadStart, Thread> threadFactory, Func<IScheduler, IObservable<T>> observableFactory) {
			return Observable.Using(
				() => NewEventLoop(threadFactory),
				scheduler => observableFactory(scheduler));
		}

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		[DebuggerStepThrough]
		public virtual IObservable<T> UsingEventLoop<T>(Func<IScheduler, IObservable<T>> observableFactory) {
			return Observable.Using(
				NewEventLoop,
				scheduler => observableFactory(scheduler));
		}

		private static void ScheduleDisposeAction(EventLoopScheduler scheduler) {
			/* Wenn Dispose() am Scheduler direkt aufgerufen wird, und sich in der Queue
			 * noch "Work"-Item befindet, stirbt der Thread mit einer ObjectDisposedException
			 * http://stackoverflow.com/questions/13109054/rx2-0-objectdisposedexception-after-diposing-eventloopscheduler
			 */
			scheduler.Schedule(scheduler.Dispose);
		}
	}
}