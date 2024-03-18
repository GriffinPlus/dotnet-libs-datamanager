///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;
using GriffinPlus.Lib.Threading;

namespace GriffinPlus.Lib.DataManager;

sealed partial class ChildDataNodeCollection
{
	#region ViewerChanged / ViewerChangedAsync

	private const string ViewerChangedEventName = "Viewer" + ChangedEventName;

	/// <summary>
	/// Occurs when a node is added to or removed from the collection (considers regular and dummy nodes).<br/>
	/// The first invocation of the event handler notifies about the initial set of nodes in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataNodeCollectionChangedEventArgs> ViewerChanged
	{
		add
		{
			lock (mNode.DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataNodeCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					SynchronizationContext.Current ?? mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					ViewerWrapper,
					new ViewerDataNodeCollectionChangedEventArgs(mNode.ViewerWrapper, mBuffer));
			}
		}

		remove => EventManager<ViewerDataNodeCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a node is added to or removed from the collection (considers regular and dummy nodes).<br/>
	/// The first invocation of the event handler notifies about the initial set of nodes in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataNodeCollectionChangedEventArgs> ViewerChangedAsync
	{
		add
		{
			lock (mNode.DataTreeManager.Sync)
			{
				EventManager<ViewerDataNodeCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					ViewerWrapper,
					new ViewerDataNodeCollectionChangedEventArgs(mNode.ViewerWrapper, mBuffer));
			}
		}

		remove => EventManager<ViewerDataNodeCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	#endregion

	#region ViewerWrapper

	private ViewerChildDataNodeCollection mViewerWrapper;

	/// <summary>
	/// Gets a <see cref="ViewerChildDataNodeCollection"/> wrapping the current collection for a viewer.
	/// </summary>
	/// <returns>The wrapper collection.</returns>
	internal ViewerChildDataNodeCollection ViewerWrapper
	{
		get
		{
			if (mViewerWrapper != null)
				return mViewerWrapper;

			lock (mNode.DataTreeManager.Sync)
			{
				mViewerWrapper ??= new ViewerChildDataNodeCollection(this);
				return mViewerWrapper;
			}
		}
	}

	#endregion

	#region ViewerCount

	/// <summary>
	/// Gets the number of nodes in the collection (considers regular and dummy nodes).<br/>
	/// For regular nodes only, use the <see cref="Count"/> property.
	/// </summary>
	internal int ViewerCount
	{
		get
		{
			lock (mNode.DataTreeManager.Sync)
			{
				return mBuffer.Count;
			}
		}
	}

	#endregion

	#region ViewerContains(...)

	/// <summary>
	/// Checks whether a node with the specified name is in the collection.
	/// </summary>
	/// <param name="name">Name of the node to check for.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy nodes;<br/>
	/// <c>false</c> to consider regular nodes only.
	/// </param>
	/// <returns>
	/// <c>true</c> if the collection contains a node with the specified name;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	internal bool ViewerContains(string name, bool considerDummies)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name, considerDummies) != null;
		}
	}

	#endregion

	#region ViewerGetByName(...)

	/// <summary>
	/// Gets the data node with the specified name.
	/// </summary>
	/// <param name="name">Name of the data node to get.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy nodes;<br/>
	/// <c>false</c> to consider regular nodes only.
	/// </param>
	/// <returns>
	/// Requested data node;<br/>
	/// <c>null</c> if there is no node with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	internal ViewerDataNode ViewerGetByName(string name, bool considerDummies)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name, considerDummies)?.ViewerWrapper;
		}
	}

	#endregion

	#region ViewerGetEnumerator()

	/// <summary>
	/// Gets an enumerator that can be used to iterate through the data node collection (considers regular and dummy nodes).
	/// </summary>
	/// <returns>An enumerator.</returns>
	/// <remarks>
	/// This method returns a thread-safe enumerator for the collection. The enumerator locks the entire tree when
	/// constructed and releases the lock when it gets disposed. To ensure the lock is released properly, call its
	/// <see cref="IDisposable.Dispose"/> method. Using a foreach statement calls the <see cref="IDisposable.Dispose"/>
	/// method automatically.
	/// </remarks>
	internal IEnumerator<ViewerDataNode> ViewerGetEnumerator()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return new MonitorSynchronizedEnumerator<ViewerDataNode>(
				mBuffer.Select(x => x.ViewerWrapper).GetEnumerator(),
				mNode.DataTreeManager.Sync);
		}
	}

	#endregion

	#region ViewerRemoveAll(...)

	/// <summary>
	/// Removes all nodes that match the specified predicate.
	/// </summary>
	/// <param name="predicate">A predicate nodes to remove must match.</param>
	/// <returns>Number of removed nodes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
	internal int ViewerRemoveAll(Predicate<ViewerDataNode> predicate)
	{
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		lock (mNode.DataTreeManager.Sync)
		{
			int count = 0;
			for (int i = mBuffer.Count - 1; i >= 0; i--)
			{
				DataNode node = mBuffer[i];
				if (node.IsDummyUnsynced || !predicate(node.ViewerWrapper)) continue;
				RemoveByIndexInternalUnsynced(i);
				count++;
			}

			return count;
		}
	}

	#endregion

	#region ViewerToArray()

	/// <summary>
	/// Gets all nodes in the collection as an array (considers regular and dummy nodes).
	/// </summary>
	/// <returns>An array containing all child nodes that are currently in the collection.</returns>
	internal ViewerDataNode[] ViewerToArray()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return [.. mBuffer.Select(x => x.ViewerWrapper)];
		}
	}

	#endregion
}
