using System.Reactive.Concurrency;
using System.Threading;

namespace System.Reactive
{
	/// <summary>
	/// Scheduler provider 
	/// </summary>
	/// <remarks>
	/// See http://introtorx.com/Content/v1.0.10621.0/16_TestingRx.html#SchedulerDI
	/// </remarks>
	public interface ISchedulerProvider
	{
		/// <summary>
		/// Represents an object that schedules units of work on the current thread.
		/// </summary>
		IScheduler CurrentThread { get; }

		/// <summary>
		/// Represents an object that schedules units of work to run immediately on the current thread.
		/// </summary>
		IScheduler Immediate { get; }

		/// <summary>
		/// Represents an object that schedules each unit of work on a separate thread.
		/// </summary>
		IScheduler NewThread { get; }

		/// <summary>
		/// Represents an object that schedules units of work on the CLR thread pool.
		/// </summary>
		IScheduler ThreadPool { get; }

		/// <summary>
		/// Represents an object that schedules units of work on the Task Parallel Library (TPL) task pool.
		/// </summary>
		IScheduler TaskPool { get; }

		/// <summary>
		/// Represents an object that schedules units of work on the platform's default scheduler.
		/// </summary>
		IScheduler Default { get; }

		/// <summary>
		/// Creates an object that schedules each unit of work on a separate named thread.
		/// </summary>
		/// <param name="threadName"></param>
		/// <returns>A scheduler that schedules each unit of work on a separate named thread.</returns>
		IScheduler NewNamedThread(string threadName);

		/// <summary>
		/// Represents an object that schedules each unit of work on a separate thread.
		/// </summary>
		/// <param name="threadFactory">Factory function for thread creation.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> is null.</exception>
		/// <returns>A scheduler that schedules each unit of work on a separate thread.</returns>
		IScheduler NewNamedThread(Func<ThreadStart, Thread> threadFactory);

		/// <summary>
		/// Represents an object that schedules units of work on a designated thread.
		/// </summary>
		/// <returns>A scheduler that schedules units of work on a designated thread.</returns>
		IDisposableScheduler NewEventLoop();

		/// <summary>
		/// Represents an object that schedules units of work on a designated named thread.
		/// </summary>
		/// <param name="threadName">Name of the designated thread</param>
		/// <returns>A scheduler that schedules units of work on a designated named thread.</returns>
		IDisposableScheduler NewEventLoop(string threadName);

		/// <summary>
		/// Represents an object that schedules units of work on a designated thread.
		/// </summary>
		/// <param name="threadFactory">Factory function for creation of the designated thread.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> is null.</exception>
		/// <returns>A scheduler that schedules units of work on a designated thread.</returns>
		IDisposableScheduler NewEventLoop(Func<ThreadStart, Thread> threadFactory);

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="threadName">Name of the designated thread</param>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		IObservable<T> UsingEventLoop<T>(string threadName, Func<IScheduler, IObservable<T>> observableFactory);

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="threadFactory">Factory function for creation of the designated thread.</param>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="threadFactory"/> or <paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		IObservable<T> UsingEventLoop<T>(Func<ThreadStart, Thread> threadFactory, Func<IScheduler, IObservable<T>> observableFactory);

		/// <summary>
		/// Constructs an observable sequence that depends on a <see cref="EventLoopScheduler"/>, whose lifetime is tied to the resulting observable sequence's lifetime.
		/// </summary>
		/// <typeparam name="T">The type of the elements in the produced sequence.</typeparam>
		/// <param name="observableFactory"></param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="observableFactory"/> is null.</exception>
		/// <returns>An observable sequence whose lifetime controls the lifetime of the dependent resource object.</returns>
		IObservable<T> UsingEventLoop<T>(Func<IScheduler, IObservable<T>> observableFactory);
	}
}
