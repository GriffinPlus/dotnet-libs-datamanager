///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.Logging;
using GriffinPlus.Lib.Threading;

namespace GriffinPlus.Lib.DataManager;

sealed partial class DataTreeManagerHost
{
	private sealed class HostThread
	{
		private readonly WeakReference<DataTreeManagerHost> mHostWeakReference;
		private readonly object                             mParameterSync;

		#region Construction/Start and Shutdown

		/// <summary>
		/// Initializes a new instance of the <see cref="HostThread"/> class.
		/// </summary>
		/// <param name="host">The <see cref="DataTreeManagerHost"/> to operate upon.</param>
		/// <param name="name">Name of the data </param>
		/// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		public HostThread(DataTreeManagerHost host, string name)
		{
			if (host == null) throw new ArgumentNullException(nameof(host));
			mParameterSync = new object();
			mHostWeakReference = new WeakReference<DataTreeManagerHost>(host);
			mShutdownCancellationTokenSource = new CancellationTokenSource();
			mParametersChangedEvent = new AsyncAutoResetEvent();
			mThreadStartedEvent = new ManualResetEventSlim(false);
			name = name != null ? $"Data Tree Manager Host (name: {name})" : "Data Tree Manager Host";
			mThread = new Thread(ThreadProc) { IsBackground = true, Name = name };
			mThread.Start();
			mThreadStartedEvent.Wait();
		}

		/// <summary>
		/// Shuts the host thread down.
		/// </summary>
		public void Shutdown()
		{
			// signal cancellation token to let the thread shut down gracefully
			mShutdownCancellationTokenSource.Cancel();

			// wait for the host thread to shut down
			if (mThread.Join(2000)) return;
			sLog.Write(LogLevel.Error, "Waiting for data tree manager thread to join seems to hang...");
			mThread.Join();

			// clean up
			mShutdownCancellationTokenSource.Dispose();
		}

		#endregion

		#region PeriodicCheckInterval

		private TimeSpan mPeriodicCheckInterval = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Gets or sets the interval between two periodic checks for dead <see cref="Data{T}"/> objects (default: 10 seconds).
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> was less than or equal to zero.</exception>
		public TimeSpan PeriodicCheckInterval
		{
			get => mPeriodicCheckInterval;
			set
			{
				if (value <= TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(value), "The interval must be greater than zero.");

				lock (mParameterSync)
				{
					mPeriodicCheckInterval = value;
					mParametersChangedEvent.Set();
				}
			}
		}

		#endregion

		#region SynchronizationContext

		/// <summary>
		/// Gets the <see cref="System.Threading.SynchronizationContext"/> of the data tree manager thread
		/// (can be used to schedule work on the thread).
		/// </summary>
		public SynchronizationContext SynchronizationContext { get; private set; }

		#endregion

		#region Processing Thread

		private readonly Thread                  mThread;
		private readonly ManualResetEventSlim    mThreadStartedEvent;
		private readonly CancellationTokenSource mShutdownCancellationTokenSource;
		private readonly AsyncAutoResetEvent     mParametersChangedEvent;

		/// <summary>
		/// Gets the <see cref="System.Threading.Thread"/> of the data tree manager thread.
		/// </summary>
		// ReSharper disable once ConvertToAutoPropertyWhenPossible
		internal Thread Thread => mThread;

		/// <summary>
		/// Thread calling event handlers that have been put into the queue.
		/// </summary>
		private void ThreadProc()
		{
			AsyncContext.Run(Operation);
			return;

			async Task Operation()
			{
				// retrieve the synchronization context of the executing thread,
				// so other threads can schedule code on it
				SynchronizationContext = SynchronizationContext.Current;

				// get cancellation token that is signaled to shut the loop down
				CancellationToken shutdownCancellationToken = mShutdownCancellationTokenSource.Token;

				// load parameters initially
				TimeSpan periodicCheckInterval;
				lock (mParameterSync)
				{
					periodicCheckInterval = mPeriodicCheckInterval;
				}

				// signal that the thread has started up and initialized the synchronization context
				mThreadStartedEvent.Set();

				while (true)
				{
					// wait for the timeout to elapse
					// (scheduled event/method invocations are processed meanwhile)
					Task task1 = Task.Delay(periodicCheckInterval, shutdownCancellationToken);
					Task task2 = mParametersChangedEvent.WaitAsync(shutdownCancellationToken);
					await Task.WhenAny(task1, task2); // does not throw exceptions, so cancellation must be checked separately...
					if (shutdownCancellationToken.IsCancellationRequested) return;

					// reload parameters, if they have changed
					if (task2.Status == TaskStatus.RanToCompletion)
					{
						lock (mParameterSync)
						{
							periodicCheckInterval = mPeriodicCheckInterval;
						}
					}

					// try to get the manager host instance the thread operates upon and abort,
					// if the host has been collected meanwhile as there will never be something
					// to do without the manager host
					if (!mHostWeakReference.TryGetTarget(out DataTreeManagerHost host))
						return;

					// perform the periodic cleanup
					// (checks for dead Data<T> objects in all managed data trees)
					Debug.Assert(host != null);
					host.DoPeriodicCleanup();
				}
			}
		}

		#endregion
	}
}
