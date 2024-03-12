///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;

namespace GriffinPlus.Lib.DataManager;

partial class DataValue<T>
{
	#region ViewerChanged / ViewerChangedAsync

	internal const string ViewerChangedEventName = "ViewerChanged";

	/// <summary>
	/// Occurs when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataValueChangedEventArgs<T>> ViewerChanged
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataValueChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					SynchronizationContext.Current ?? SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataValueChangedEventArgs<T>(this, ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<ViewerDataValueChangedEventArgs<T>>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	/// <summary>
	/// Is called when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataValueChangedEventArgs<T>> ViewerChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataValueChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataValueChangedEventArgs<T>(this, ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<ViewerDataValueChangedEventArgs<T>>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	#endregion

	#region ViewerUntypedChanged / ViewerUntypedChangedAsync

	internal const string ViewerUntypedChangedEventName = "ViewerUntypedChanged";

	/// <summary>
	/// Occurs when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<UntypedViewerDataValueChangedEventArgs> ViewerUntypedChanged
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<UntypedViewerDataValueChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerUntypedChangedEventName,
					value,
					SynchronizationContext.Current ?? SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedViewerDataValueChangedEventArgs(this, ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<UntypedViewerDataValueChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerUntypedChangedEventName,
			value);
	}

	/// <summary>
	/// Is called when something in the current data value changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the data value.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<UntypedViewerDataValueChangedEventArgs> ViewerUntypedChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<UntypedViewerDataValueChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerUntypedChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedViewerDataValueChangedEventArgs(this, ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<UntypedViewerDataValueChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerUntypedChangedEventName,
			value);
	}

	#endregion

	#region ViewerWrapper

	private ViewerDataValue<T> mViewerWrapper;

	/// <summary>
	/// Gets a <see cref="ViewerDataValue{T}"/> wrapping the current value for a viewer.
	/// </summary>
	/// <returns>The wrapper value.</returns>
	internal ViewerDataValue<T> ViewerWrapper
	{
		get
		{
			if (mViewerWrapper != null)
				return mViewerWrapper;

			lock (DataTreeManager.Sync)
			{
				mViewerWrapper ??= new ViewerDataValue<T>(this);
				return mViewerWrapper;
			}
		}
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.ViewerWrapper"/>
	IUntypedViewerDataValue IUntypedDataValueInternal.ViewerWrapper => ViewerWrapper;

	#endregion

	#region ViewerIsDummy

	/// <summary>
	/// Gets or sets a value indicating whether the current value is a dummy value,
	/// i.e. a value that exists for administrative purposes only.
	/// </summary>
	public bool ViewerIsDummy
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return IsDummyUnsynced;
			}
		}
	}

	#endregion

	#region ViewerExecuteAtomically(...)

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <param name="action">Action to perform within the locked section.</param>
	internal void ViewerExecuteAtomically(ViewerDataValueAction<T> action)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper);
		}
	}

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <typeparam name="TState">Some custom object passed to the action.</typeparam>
	/// <param name="action">Action to perform within the locked section.</param>
	/// <param name="state">Some state object passed to the action.</param>
	internal void ViewerExecuteAtomically<TState>(ViewerDataValueAction<T, TState> action, TState state)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper, state);
		}
	}

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <param name="action">Action to perform within the locked section.</param>
	internal void ViewerExecuteAtomically(UntypedViewerDataValueAction action)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper);
		}
	}

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <typeparam name="TState">Some custom object passed to the action.</typeparam>
	/// <param name="action">Action to perform within the locked section.</param>
	/// <param name="state">Some state object passed to the action.</param>
	internal void ViewerExecuteAtomically<TState>(UntypedViewerDataValueAction<TState> action, TState state)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper, state);
		}
	}

	#endregion
}
