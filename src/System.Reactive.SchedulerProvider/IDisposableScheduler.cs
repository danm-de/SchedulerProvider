using System.Reactive.Concurrency;

namespace System.Reactive
{
	/// <summary>
	/// <summary>Represents an object that schedules units of work.</summary>
	/// </summary>
	public interface IDisposableScheduler : IScheduler, IDisposable
	{ }
}