///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
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
		private readonly Thread                             mThread;
		private readonly ManualResetEventSlim               mThreadStartedEvent;
		private readonly CancellationTokenSource            mShutdownCancellationTokenSource;

		/// <summary>
		/// Initializes a new instance of the <see cref="HostThread"/> class.
		/// </summary>
		/// <param name="host">The <see cref="DataTreeManagerHost"/> to operate upon.</param>
		/// <param name="name">Name of the data </param>
		/// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		public HostThread(DataTreeManagerHost host, string name)
		{
			if (host == null) throw new ArgumentNullException(nameof(host));
			mHostWeakReference = new WeakReference<DataTreeManagerHost>(host);
			mShutdownCancellationTokenSource = new CancellationTokenSource();
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
		}

		/// <summary>
		/// Gets the interval between two periodic checks for dead <see cref="Data{T}"/> objects.
		/// </summary>
		private TimeSpan PeriodicCheckInterval { get; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Gets the <see cref="System.Threading.SynchronizationContext"/> of the data tree manager thread
		/// (can be used to schedule work on the thread).
		/// </summary>
		public SynchronizationContext SynchronizationContext { get; private set; }

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

				// signal that the thread has started up and initialized the synchronization context
				mThreadStartedEvent.Set();

				while (true)
				{
					// wait for the timeout to elapse
					// (scheduled event/method invocations are processed meanwhile)
					try
					{
						await Task.Delay(PeriodicCheckInterval, shutdownCancellationToken);
					}
					catch (OperationCanceledException)
					{
						return;
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
	}
}
