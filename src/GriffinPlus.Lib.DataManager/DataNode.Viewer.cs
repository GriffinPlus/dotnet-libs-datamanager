///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;

namespace GriffinPlus.Lib.DataManager;

partial class DataNode
{
	#region ViewerChanged / ViewerChangedAsync

	internal const string ViewerChangedEventName = "ViewerChanged";

	/// <summary>
	/// Occurs when a property of the current node changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the node.<br/>
	/// Subsequent invocations notify about changes to the node.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataNodeChangedEventArgs> ViewerChanged
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataNodeChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					SynchronizationContext.Current ?? SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataNodeChangedEventArgs(this, ViewerDataNodeChangedFlags.All | ViewerDataNodeChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<ViewerDataNodeChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a property of the current node changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the node.<br/>
	/// Subsequent invocations notify about changes to the node.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataNodeChangedEventArgs> ViewerChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataNodeChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataNodeChangedEventArgs(this, ViewerDataNodeChangedFlags.All | ViewerDataNodeChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<ViewerDataNodeChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	#endregion

	#region ViewerWrapper

	private ViewerDataNode mViewerWrapper;

	/// <summary>
	/// Gets a <see cref="ViewerDataNode"/> wrapping the current node for a viewer.
	/// </summary>
	/// <returns>The wrapper node.</returns>
	internal ViewerDataNode ViewerWrapper
	{
		get
		{
			if (mViewerWrapper != null)
				return mViewerWrapper;

			lock (DataTreeManager.Sync)
			{
				mViewerWrapper ??= new ViewerDataNode(this);
				return mViewerWrapper;
			}
		}
	}

	#endregion

	#region ViewerIsDummy

	/// <summary>
	/// Gets or sets a value indicating whether the current node is a dummy node,
	/// i.e. a node that exists for administrative purposes only.
	/// </summary>
	internal bool ViewerIsDummy
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

	#region ViewerParent

	/// <summary>
	/// Gets the parent of the current node.
	/// </summary>
	/// <value>
	/// The parent node;<br/>
	/// <c>null</c> if the node is the root node.
	/// </value>
	internal ViewerDataNode ViewerParent
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return new ViewerDataNode(mParent);
			}
		}
	}

	#endregion

	#region ViewerChildren

	/// <summary>
	/// Gets the collection of child nodes associated with the current node.
	/// </summary>
	internal ViewerChildDataNodeCollection ViewerChildren { get; }

	#endregion

	#region ViewerValues

	/// <summary>
	/// Gets collection of values associated with the current node.
	/// </summary>
	internal ViewerDataValueCollection ViewerValues { get; }

	#endregion

	#region ViewerCopy(...)

	/// <summary>
	/// Copies the current node (including its child nodes and values) to the specified node.
	/// </summary>
	/// <param name="destinationNode">Data node to copy the current node beneath.</param>
	/// <param name="renameIfNecessary">
	/// <c>true</c> to rename the copy of the current node, if there is already a node with the name of the current node;<br/>
	/// <c>false</c> to throw a <see cref="DataNodeExistsAlreadyException"/> in this case.
	/// </param>
	/// <returns>The copy of the current node in the destination data tree.</returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="destinationNode"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The current data node is dummy and can therefore not be copied.
	/// </exception>
	/// <exception cref="DataNodeExistsAlreadyException">
	/// There is already a data node with the name of the current node and <paramref name="renameIfNecessary"/> is <c>false</c>.
	/// </exception>
	/// <remarks>
	/// This method inserts a copy of the current node (including its child nodes and values) beneath the specified node
	/// and optionally renames the copied node, if a node with the same name exists already by suffixing the name with
	/// a hashtag and an incrementing number (i.e. #2, #3, #4, etc.).
	/// </remarks>
	internal ViewerDataNode ViewerCopy(ViewerDataNode destinationNode, bool renameIfNecessary)
	{
		lock (DataTreeManager.Sync)
		lock (destinationNode.WrappedNode.DataTreeManager.Sync)
		{
			return CopyUnsynced(destinationNode.WrappedNode, renameIfNecessary).ViewerWrapper;
		}
	}

	#endregion

	#region ViewerExecuteAtomically(...)

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock
	/// (do not perform excessive calculations in the callback method and do not block, since the entire
	/// data tree is locked and blocking might result in a deadlock!!!)
	/// </summary>
	/// <param name="action">Method to call within the locked section.</param>
	internal void ViewerExecuteAtomically(ViewerDataNodeAction action)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper);
		}
	}

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock
	/// (do not perform excessive calculations in the callback method and do not block, since the entire
	/// data tree is locked and blocking might result in a deadlock!!!)
	/// </summary>
	/// <param name="action">Method to call within the locked section.</param>
	/// <param name="state">Some state object to pass to the action.</param>
	internal void ViewerExecuteAtomically<TState>(ViewerDataNodeAction<TState> action, TState state)
	{
		lock (DataTreeManager.Sync)
		{
			action(ViewerWrapper, state);
		}
	}

	#endregion
}
