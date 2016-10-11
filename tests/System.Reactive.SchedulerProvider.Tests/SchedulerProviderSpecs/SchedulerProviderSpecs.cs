using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Reactive.Tests.SchedulerProviderSpecs
{
	[TestFixture]
	public class If_the_user_creates_a_SchedulerProvider_instance_using_the_default_constructor : Spec
	{
		private SchedulerProvider _sut;

		protected override void EstablishContext() {
			_sut = new SchedulerProvider();
		}

		[Test]
		public void Shall_Default_be_set() {
			_sut.Default.Should().Be(DefaultScheduler.Instance);
		}

		[Test]
		public void Shall_NewThreadScheduler_be_set() {
			_sut.NewThread.Should().Be(NewThreadScheduler.Default);
		}

		[Test]
		public void Shall_CurrentThreadScheduler_be_set() {
			_sut.CurrentThread.Should().Be(CurrentThreadScheduler.Instance);
		}

		[Test]
		public void Shall_ImmediateScheduler_be_set() {
			_sut.Immediate.Should().Be(ImmediateScheduler.Instance);
		}

		[Test]
		public void Shall_TaskPoolScheduler_be_set() {
			_sut.TaskPool.Should().Be(TaskPoolScheduler.Default);
		}

		[Test]
		public void Shall_ThreadPoolScheduler_be_set() {
			_sut.ThreadPool.Should().Be(ThreadPoolScheduler.Instance);
		}
	}

	[TestFixture]
	public class If_a_NewThreadScheduler_is_being_used : Spec
	{
		private static string ScheduleAndWaitForThreadName(IScheduler scheduler, EventWaitHandle resetEvent) {
			string workerThreadName = null;
			using (scheduler.Schedule(() => {
				workerThreadName = Thread.CurrentThread.Name;
				resetEvent.Set();
			})) {
				// wait max. 10 seconds for the test
				resetEvent.WaitOne(TimeSpan.FromSeconds(10));
			}
			return workerThreadName;
		}

		[Test]
		public void Shall_jobs_be_started_from_the_new_named_thread() {
			var resetEvent = new AutoResetEvent(false);
			var expectedThreadName = "TEST-" + Guid.NewGuid();
			var scheduler = SchedulerProvider.Instance.NewNamedThread(expectedThreadName);

			var workerThreadName = ScheduleAndWaitForThreadName(scheduler, resetEvent);

			workerThreadName
				.Should()
				.Be(expectedThreadName);
		}

		[Test]
		public void Shall_jobs_be_started_from_the_new_thread() {
			var resetEvent = new AutoResetEvent(false);
			var expectedThreadName = "TEST-" + Guid.NewGuid();

			var scheduler = SchedulerProvider.Instance.NewNamedThread(start =>
				new Thread(start) {
					Name = expectedThreadName
				});

			var workerThreadName = ScheduleAndWaitForThreadName(scheduler, resetEvent);

			workerThreadName
				.Should()
				.Be(expectedThreadName);
		}

	}

	[TestFixture]
	public class If_an_EventLoopScheduler_is_being_used : Spec
	{
		private static string ScheduleAndWaitForThreadName(IScheduler scheduler, EventWaitHandle resetEvent) {
			string workerThreadName = null;
			using (scheduler.Schedule(() => {
				workerThreadName = Thread.CurrentThread.Name;
				resetEvent.Set();
			})) {
				// wait max. 10 seconds for the test
				resetEvent.WaitOne(TimeSpan.FromSeconds(10));
			}
			return workerThreadName;
		}

		[Test]
		public void Shall_jobs_be_started_from_the_new_named_thread() {
			var resetEvent = new AutoResetEvent(false);
			var expectedThreadName = "TEST-" + Guid.NewGuid();
			var scheduler = SchedulerProvider.Instance.NewEventLoop(expectedThreadName);

			var workerThreadName = ScheduleAndWaitForThreadName(scheduler, resetEvent);

			workerThreadName
				.Should()
				.Be(expectedThreadName);
		}

		[Test]
		public void Shall_jobs_be_started_from_the_new_thread() {
			var resetEvent = new AutoResetEvent(false);
			var expectedThreadName = "TEST-" + Guid.NewGuid();

			var scheduler = SchedulerProvider.Instance.NewEventLoop(start =>
				new Thread(start) {
					Name = expectedThreadName
				});

			var workerThreadName = ScheduleAndWaitForThreadName(scheduler, resetEvent);

			workerThreadName
				.Should()
				.Be(expectedThreadName);
		}

	}

	[TestFixture]
	public class If_the_user_wraps_an_observable_by_using_the_EventLoopScheduler : Spec
	{
		private ISchedulerProvider _sut;

		protected override void EstablishContext() {
			_sut = SchedulerProvider.Instance;
		}

		[Test]
		public void Shall_all_jobs_started_by_the_worker_thread() {
			var resetEvent = new AutoResetEvent(false);
			var expectedThreadName = "TEST-" + Guid.NewGuid();

			string workerThreadName = null;
			var subject = new Subject<string>();

			using (_sut.UsingEventLoop(
				start => new Thread(start) {
					Name = expectedThreadName
				},
				s => subject
					.ObserveOn(s)
					.Do(text => {
						workerThreadName = Thread.CurrentThread.Name;
						resetEvent.Set();
					}))
				.Subscribe()) {

				// trigger EventLoopScheduler
				subject.OnNext("Test");

				// wait max. 10 seconds for the test
				resetEvent.WaitOne(TimeSpan.FromSeconds(10));

				workerThreadName
					.Should()
					.Be(expectedThreadName);
			}
		}
	}

	[TestFixture]
	public class If_the_user_creates_an_observable_using_the_EventLoopScheduler_and_disposes_the_scheduler_after_starting_a_long_running_job : Spec
	{
		private readonly AutoResetEvent _longRunningActionStarted = new AutoResetEvent(false);
		private readonly AutoResetEvent _longRunningActionFinished = new AutoResetEvent(false);
		private readonly Subject<string> _eventStream = new Subject<string>();
		private int _actionsCompleted;
		private IDisposable _subscription;

		protected override void EstablishContext() {
			var sut = new SchedulerProvider();
			_subscription = sut.UsingEventLoop(
				scheduler => _eventStream
				.ObserveOn(scheduler)
				.Do(LongRunningAction))
				.Subscribe();
		}

		private void LongRunningAction(string text) {
			var actionCount = Interlocked.Increment(ref _actionsCompleted);
			Debug.Print("Action {0} started", actionCount);
			_longRunningActionStarted.Set();
			_longRunningActionFinished.WaitOne(TimeSpan.FromSeconds(10));
			Debug.Print("Action {0} finished", actionCount);
		}

		protected override void BecauseOf() {
			_eventStream.OnNext("First action..");
			_longRunningActionStarted.WaitOne(TimeSpan.FromSeconds(10));
			_subscription.Dispose();
			_longRunningActionFinished.Set();
			_eventStream.OnNext("Second action..");
			_longRunningActionFinished.Set();
		}

		[Test]
		public void Shall_the_long_running_job_being_completed() {
			_actionsCompleted.Should().BeGreaterThan(0);
		}

		[Test]
		public void Shall_no_further_jobs_being_scheduled_after_dispose() {
			_actionsCompleted.Should().Be(1);
		}
	}
}