///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using GriffinPlus.Lib.Logging;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Spawns a thread firing event notifications for all managed data trees and monitors the lifetime
/// of <see cref="Data{T}"/> objects referencing <see cref="DataValue{T}"/> objects in the trees in
/// order to clean up dummy paths, that are not needed any more. The thread dispatches all asynchronous
/// events associated with managed data trees, i.e. notifications about changes are executed in the order
/// of their occurrence. When using TPL, continuations can be scheduled to run on the same thread,
/// if the synchronization context supports marshalling calls into the thread, usually STA threads or
/// threads with a message loop in general. The thread hosted by this class is thread with a message loop.
/// </summary>
public sealed partial class DataTreeManagerHost
{
	private static readonly LogWriter                          sLog              = LogWriter.Get<DataTreeManagerHost>();
	private static readonly WeakReference<DataTreeManagerHost> sDefault          = new(null);
	private static readonly ReaderWriterLockSlim               sDefaultLock      = new(LockRecursionPolicy.NoRecursion);
	private readonly        List<DataTreeManager>              mDataTreeManagers = [];
	private readonly        HostThread                         mHostThread;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataTreeManagerHost"/> class.
	/// </summary>
	/// <param name="name">
	/// Name of the data tree manager thread instance
	/// (primarily useful for distinguishing multiple threads in the debugger, may be <c>null</c>).
	/// </param>
	public DataTreeManagerHost(string name = null)
	{
		mHostThread = new HostThread(this, name);
	}

	/// <summary>
	/// Gets the default instance of the data tree manager host.
	/// </summary>
	public static DataTreeManagerHost Default
	{
		get
		{
			sDefaultLock.EnterReadLock();
			try
			{
				if (sDefault.TryGetTarget(out DataTreeManagerHost host))
					return host;
			}
			finally
			{
				sDefaultLock.ExitReadLock();
			}

			sDefaultLock.EnterWriteLock();
			try
			{
				if (sDefault.TryGetTarget(out DataTreeManagerHost host))
					return host;

				host = new DataTreeManagerHost();
				sDefault.SetTarget(host);
				return host;
			}
			finally
			{
				sDefaultLock.ExitWriteLock();
			}
		}
	}

	/// <summary>
	/// Gets the <see cref="System.Threading.SynchronizationContext"/> of the data tree manager thread
	/// (can be used to schedule work on the thread).
	/// </summary>
	public SynchronizationContext SynchronizationContext => mHostThread.SynchronizationContext;

	/// <summary>
	/// Creates a new instance of the <see cref="DataTreeManager"/> class associated with the current host.
	/// </summary>
	/// <param name="root">Root node of the data tree to manage.</param>
	/// <param name="serializer">Serializer to use for serializing, deserializing and copying data in the data tree.</param>
	/// <returns>A new instance of the <see cref="DataTreeManager"/> class.</returns>
	internal DataTreeManager CreateDataTreeManager(DataNode root, IDataManagerSerializer serializer)
	{
		var manager = new DataTreeManager(this, root, serializer);
		lock (mDataTreeManagers) mDataTreeManagers.Add(manager);
		return manager;
	}

	/// <summary>
	/// Enqueues an event to raise in the context of the data tree manager host thread.
	/// </summary>
	/// <param name="event">Event to raise.</param>
	/// <param name="sender">The sender of the event (is passed along to the event handler).</param>
	/// <param name="e">Event Arguments (is passed along to the event handler).</param>
	public void EnqueueEventNotification<TEventArgs>(EventHandler<TEventArgs> @event, object sender, TEventArgs e)
		where TEventArgs : EventArgs
	{
		if (@event == null)
			return;

		Delegate[] handlers = @event.GetInvocationList();
		if (handlers.Length > 0)
		{
			mHostThread.SynchronizationContext.Post(
				_ =>
				{
					// call event handlers
					int handlerCount = handlers.Length;
					for (int i = 0; i < handlerCount; i++)
					{
						try
						{
							((EventHandler<TEventArgs>)handlers[i]).Invoke(sender, e);
						}
						catch (Exception ex)
						{
							sLog.Write(
								LogLevel.Error,
								"An unhandled exception occurred in event handler {0}.{1}.\r\nException: {2}",
								handlers[i].Method.DeclaringType?.FullName,
								handlers[i].Method.Name,
								ex);

							Debug.Assert(
								false,
								"An unhandled exception occurred in the event handler.",
								"Event handler: {0}.{1}.\r\nException Text: {2}",
								handlers[i].Method.DeclaringType?.FullName,
								handlers[i].Method.Name,
								ex.Message);
						}
					}
				},
				null);
		}
	}

	/// <summary>
	/// Enqueues a method to call in the context of the data tree manager host thread.
	/// </summary>
	/// <param name="method">Method to call.</param>
	/// <param name="args">Arguments to apply to the method.</param>
	public void EnqueueMethodCall(Delegate method, params object[] args)
	{
		if (method == null)
			return;

		mHostThread.SynchronizationContext.Post(
			_ =>
			{
				try
				{
					method.DynamicInvoke(args);
				}
				catch (TargetInvocationException ex)
				{
					sLog.Write(
						LogLevel.Error,
						"An unhandled exception occurred in event handler {0}.{1}.\r\nException: {2}",
						method.Method.DeclaringType?.FullName,
						method.Method.Name,
						ex.InnerException);

					Debug.Assert(
						false,
						"An unhandled exception occurred in the event handler.",
						"Event handler: {0}.{1}.\r\nException Text: {2}",
						method.Method.DeclaringType?.FullName,
						method.Method.Name,
						ex.InnerException?.Message);
				}
			},
			null);
	}

	/// <summary>
	/// Checks for dead <see cref="Data{T}"/> objects and removes the data values they reference.
	/// This method is called by the <see cref="HostThread"/> class.
	/// </summary>
	private void DoPeriodicCleanup()
	{
		DataTreeManager[] managers;
		lock (mDataTreeManagers)
		{
			managers = [.. mDataTreeManagers];
		}

		List<DataTreeManager> managersToRemove = null;
		foreach (DataTreeManager manager in managers)
		{
			if (manager.CheckPeriodically()) continue;
			managersToRemove ??= [];
			managersToRemove.Add(manager);
		}

		if (managersToRemove == null)
			return;

		lock (mDataTreeManagers)
		{
			foreach (DataTreeManager manager in managersToRemove)
			{
				mDataTreeManagers.Remove(manager);
			}
		}
	}
}
