using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;

using Xunit;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An expected data node.
/// </summary>
[DebuggerDisplay("Count: {" + nameof(Count) + "}")]
sealed partial class ExpectedChildDataNodeCollection : ObservableCollection<ExpectedDataNode>
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedChildDataNodeCollection"/> class.
	/// </summary>
	public ExpectedChildDataNodeCollection()
	{
		CollectionChanged += EH_Children_CollectionChanged;
	}

	#endregion

	#region ActualCollection

	private ChildDataNodeCollection mActualCollection;

	/// <summary>
	/// Gets or sets the actual collection associated with the current expected collection.
	/// </summary>
	public ChildDataNodeCollection ActualCollection
	{
		get => mActualCollection;
		set
		{
			if (mActualCollection == value)
				return;

			// unregister event handlers bound to the old actual collection
			UnregisterEventsRecursively();

			// set the actual collection
			mActualCollection = value;

			// clear all expected and received events before registering any event of the new node
			// (the node will immediately fire an initial changed event...)
			ResetExpectedEvents(recursive: false);
			ResetReceivedEvents(recursive: false);
		}
	}

	#endregion

	#region Parent

	private ExpectedDataNode mParent;

	/// <summary>
	/// Gets or sets the data node the collection resides in.
	/// </summary>
	public ExpectedDataNode Parent
	{
		get => mParent;
		set
		{
			mParent = value;
			SetParentOfItems(this);
		}
	}

	#endregion

	#region AssertExpectedEventsWereRaised()

	/// <summary>
	/// Recursively check whether collection events and events of contained data nodes and values were raised as expected.
	/// </summary>
	internal void AssertExpectedEventsWereRaised()
	{
		// check whether 'Changed' events and 'ChangedAsync' events were raised properly
		AssertExpectedChangedEventsWereRaised(mExpectedChangedEvents, ReceivedChangedEvents, SynchronizationContext.Current);
		AssertExpectedChangedEventsWereRaised(mExpectedChangedAsyncEvents, ReceivedChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether 'ViewerChanged' events and 'ViewerChangedAsync' events were raised properly
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedEvents, ReceivedViewerChangedEvents, SynchronizationContext.Current);
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedAsyncEvents, ReceivedViewerChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check contained data nodes as well
		foreach (ExpectedDataNode expectedChildNode in Items)
		{
			expectedChildNode.AssertExpectedEventsWereRaised();
		}
	}

	/// <summary>
	/// Checks whether the specified expected events equal the specified received events (regular events).
	/// </summary>
	/// <param name="expectedChangedEvents">The expected events.</param>
	/// <param name="receivedChangedEvents">The received events.</param>
	/// <param name="expectedSynchronizationContext">Synchronization context of the thread that should have executed the event handler.</param>
	private static void AssertExpectedChangedEventsWereRaised(
		IReadOnlyCollection<ExpectedChangedEventItem> expectedChangedEvents,
		IReadOnlyCollection<ReceivedChangedEventItem> receivedChangedEvents,
		SynchronizationContext                        expectedSynchronizationContext)
	{
		Assert.Equal(expectedChangedEvents.Count, receivedChangedEvents.Count);

		foreach ((ExpectedChangedEventItem expected, ReceivedChangedEventItem actual) in expectedChangedEvents.Zip(
			         receivedChangedEvents,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// check whether the event name is as expected
			Assert.Equal(expected.EventName, actual.EventName);

			// check whether the event was raised in the expected synchronization context
			Assert.Same(expectedSynchronizationContext, actual.SynchronizationContext);

			// check whether event sender and arguments are as expected
			Assert.Same(expected.Sender, actual.Sender);
			Assert.Equal(expected.EventArgs.Action, actual.EventArgs.Action);
			Assert.Equal(expected.EventArgs.ParentNode, actual.EventArgs.ParentNode);
			Assert.Same(expected.EventArgs.ChildNode, actual.EventArgs.ChildNode);
			Assert.Equal(expected.EventArgs.ChildNodes, actual.EventArgs.ChildNodes);
		}
	}

	/// <summary>
	/// Checks whether the specified expected events equal the specified received events (viewer events).
	/// </summary>
	/// <param name="expectedChangedEvents">The expected events.</param>
	/// <param name="receivedChangedEvents">The received events.</param>
	/// <param name="expectedSynchronizationContext">Synchronization context of the thread that should have executed the event handler.</param>
	private static void AssertExpectedViewerChangedEventsWereRaised(
		IReadOnlyCollection<ExpectedViewerChangedEventItem> expectedChangedEvents,
		IReadOnlyCollection<ReceivedViewerChangedEventItem> receivedChangedEvents,
		SynchronizationContext                              expectedSynchronizationContext)
	{
		Assert.Equal(expectedChangedEvents.Count, receivedChangedEvents.Count);

		foreach ((ExpectedViewerChangedEventItem expected, ReceivedViewerChangedEventItem actual) in expectedChangedEvents.Zip(
			         receivedChangedEvents,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// check whether the event name is as expected
			Assert.Equal(expected.EventName, actual.EventName);

			// check whether the event was raised in the expected synchronization context
			Assert.Same(expectedSynchronizationContext, actual.SynchronizationContext);

			// check whether event sender and arguments are as expected
			Assert.Same(expected.Sender, actual.Sender);
			Assert.Equal(expected.EventArgs.Action, actual.EventArgs.Action);
			Assert.Equal(expected.EventArgs.ParentNode, actual.EventArgs.ParentNode);
			Assert.Same(expected.EventArgs.ChildNode, actual.EventArgs.ChildNode);
			Assert.Equal(expected.EventArgs.ChildNodes, actual.EventArgs.ChildNodes);
		}
	}

	#endregion

	#region Tracking Collection Changes

	/// <summary>
	/// Handles changes to the collection.
	/// </summary>
	/// <param name="sender">The collection itself.</param>
	/// <param name="e">Event arguments.</param>
	private void EH_Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
			// add: some items have been added
			// => add parent to new items
			case NotifyCollectionChangedAction.Add:
				SetParentOfItems(e.NewItems!.Cast<IExpectedItem>());
				break;

			// remove: some items have been removed
			// => reset parent of old items
			case NotifyCollectionChangedAction.Remove:
				ResetParent(e.OldItems!.Cast<IExpectedItem>());
				break;

			// replace: some items are replaced with other items
			// => reset parent of old items and set parent for new items
			case NotifyCollectionChangedAction.Replace:
				ResetParent(e.OldItems!.Cast<IExpectedItem>());
				SetParentOfItems(e.NewItems!.Cast<IExpectedItem>());
				break;

			// move: no items added/removed
			// => nothing to do
			case NotifyCollectionChangedAction.Move:
				break;

			// reset: everything (might) have changed
			// => process all items in the collection
			// NOTE: the parent of old items is not cleared!!!
			case NotifyCollectionChangedAction.Reset:
				SetParentOfItems(this);
				break;

			// unhandled action
			default:
				throw new ArgumentOutOfRangeException(
					nameof(e),
					e.Action,
					"The value of the NotifyCollectionChangedEventArgs.Action property is unhandled.");
		}
	}

	/// <summary>
	/// Sets the parent node for all specified items.
	/// </summary>
	/// <param name="items">Items to set the parent for.</param>
	private void SetParentOfItems(IEnumerable<IExpectedItem> items)
	{
		if (items == null) return;
		foreach (IExpectedItem item in items) item.Parent = Parent;
	}

	/// <summary>
	/// Resets the parent node for all specified items.
	/// </summary>
	/// <param name="items">Items to reset the parent for.</param>
	private static void ResetParent(IEnumerable<IExpectedItem> items)
	{
		if (items == null) return;
		foreach (IExpectedItem item in items) item.Parent = null;
	}

	#endregion

	#region Event Monitoring

	private readonly List<ExpectedChangedEventItem>       mExpectedChangedEvents            = [];
	private readonly List<ExpectedChangedEventItem>       mExpectedChangedAsyncEvents       = [];
	private readonly List<ExpectedViewerChangedEventItem> mExpectedViewerChangedEvents      = [];
	private readonly List<ExpectedViewerChangedEventItem> mExpectedViewerChangedAsyncEvents = [];
	private readonly List<ReceivedChangedEventItem>       mReceivedChangedEvents            = [];
	private readonly List<ReceivedChangedEventItem>       mReceivedChangedAsyncEvents       = [];
	private readonly List<ReceivedViewerChangedEventItem> mReceivedViewerChangedEvents      = [];
	private readonly List<ReceivedViewerChangedEventItem> mReceivedViewerChangedAsyncEvents = [];

	/// <summary>
	/// Gets the <see cref="ChildDataNodeCollection.Changed"/> events that are expected to be fired.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedEvents => [.. mExpectedChangedEvents];

	/// <summary>
	/// Gets the <see cref="ChildDataNodeCollection.ChangedAsync"/> events that are expected to be fired.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedAsync => [.. mExpectedChangedAsyncEvents];

	/// <summary>
	/// Gets the <see cref="ViewerChildDataNodeCollection.Changed"/> events,
	/// respectively the <see cref="ChildDataNodeCollection.ViewerChanged"/> events,
	/// that are expected to be fired.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedEvents => [.. mExpectedViewerChangedEvents];

	/// <summary>
	/// Gets the <see cref="ViewerChildDataNodeCollection.ChangedAsync"/> events,
	/// respectively the <see cref="ChildDataNodeCollection.ViewerChangedAsync"/> events,
	/// that are expected to be fired.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedAsync => [.. mExpectedViewerChangedAsyncEvents];

	/// <summary>
	/// Gets the received <see cref="ChildDataNodeCollection.Changed"/> events.
	/// </summary>
	public ReceivedChangedEventItem[] ReceivedChangedEvents
	{
		get
		{
			lock (mReceivedChangedEvents)
			{
				return [.. mReceivedChangedEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="ChildDataNodeCollection.ChangedAsync"/> events.
	/// </summary>
	public ReceivedChangedEventItem[] ReceivedChangedAsyncEvents
	{
		get
		{
			lock (mReceivedChangedAsyncEvents)
			{
				return [.. mReceivedChangedAsyncEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="ViewerChildDataNodeCollection.Changed"/> events,
	/// respectively the <see cref="ChildDataNodeCollection.ViewerChanged"/> events.
	/// </summary>
	public ReceivedViewerChangedEventItem[] ReceivedViewerChangedEvents
	{
		get
		{
			lock (mReceivedViewerChangedEvents)
			{
				return [.. mReceivedViewerChangedEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="ViewerChildDataNodeCollection.ChangedAsync"/> events,
	/// respectively the <see cref="ChildDataNodeCollection.ViewerChangedAsync"/> events.
	/// </summary>
	public ReceivedViewerChangedEventItem[] ReceivedViewerChangedAsyncEvents
	{
		get
		{
			lock (mReceivedViewerChangedAsyncEvents)
			{
				return [.. mReceivedViewerChangedAsyncEvents];
			}
		}
	}

	/// <summary>
	/// Registers all events of the actual child node collection and all data nodes and data values within them recursively.
	/// </summary>
	internal void RegisterEventsRecursively()
	{
		Assert.NotNull(mActualCollection);
		Assert.NotNull(mParent);

		// register 'Changed' event
		mActualCollection.Changed += ChangedHandler;

		// add expected initial 'Changed' event
		mExpectedChangedEvents.Add(
			new ExpectedChangedEventItem(
				nameof(ChildDataNodeCollection.Changed),
				mActualCollection,
				new ExpectedDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.InitialUpdate,
					Parent.ActualDataNode,
					Items.Select(x => x.ActualDataNode).ToArray())));

		// register 'ChangedAsync' event
		mActualCollection.ChangedAsync += ChangedAsyncHandler;

		// add expected initial 'ChangedAsync' event
		mExpectedChangedAsyncEvents.Add(
			new ExpectedChangedEventItem(
				nameof(ChildDataNodeCollection.ChangedAsync),
				mActualCollection,
				new ExpectedDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.InitialUpdate,
					Parent.ActualDataNode,
					Items.Select(x => x.ActualDataNode).ToArray())));

		// register 'ViewerChanged' event
		mActualCollection.ViewerChanged += ViewerChangedHandler;

		// add expected initial 'ViewerChanged' event
		mExpectedViewerChangedEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(ChildDataNodeCollection.ViewerChanged),
				mActualCollection.ViewerWrapper,
				new ExpectedViewerDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.InitialUpdate,
					Parent.ActualDataNode.ViewerWrapper,
					Items.Select(x => x.ActualDataNode.ViewerWrapper).ToArray())));

		// register 'ViewerChangedAsync' event
		mActualCollection.ViewerChangedAsync += ViewerChangedAsyncHandler;

		// add expected initial 'ViewerChangedAsync' event
		mExpectedViewerChangedAsyncEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(ChildDataNodeCollection.ViewerChangedAsync),
				mActualCollection.ViewerWrapper,
				new ExpectedViewerDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.InitialUpdate,
					Parent.ActualDataNode.ViewerWrapper,
					Items.Select(x => x.ActualDataNode.ViewerWrapper).ToArray())));

		// register events downstream
		foreach (ExpectedDataNode node in Items)
		{
			node.RegisterEventsRecursively();
		}
	}

	/// <summary>
	/// Unregisters all events of the actual child node collection and all data nodes and data values within them recursively.
	/// </summary>
	internal void UnregisterEventsRecursively()
	{
		// abort if no actual collection is set
		if (mActualCollection == null)
			return;

		// unregister collection events
		mActualCollection.Changed -= ChangedHandler;
		mActualCollection.ChangedAsync -= ChangedAsyncHandler;
		mActualCollection.ViewerChanged -= ViewerChangedHandler;
		mActualCollection.ViewerChangedAsync -= ViewerChangedAsyncHandler;

		// unregister events downstream
		foreach (ExpectedDataNode node in Items)
		{
			node.UnregisterEventsRecursively();
		}
	}

	/// <summary>
	/// Clears the lists of received events.
	/// </summary>
	/// <param name="recursive">
	/// <c>true</c> to clear expected events recursively;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	public void ResetExpectedEvents(bool recursive)
	{
		mExpectedChangedEvents.Clear();
		mExpectedChangedAsyncEvents.Clear();
		mExpectedViewerChangedEvents.Clear();
		mExpectedViewerChangedAsyncEvents.Clear();

		if (!recursive)
			return;

		foreach (ExpectedDataNode child in Items)
		{
			child.ResetExpectedEvents();
		}
	}

	/// <summary>
	/// Clears the lists of received events.
	/// </summary>
	/// <param name="recursive">
	/// <c>true</c> to clear received events recursively;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	public void ResetReceivedEvents(bool recursive = true)
	{
		lock (mReceivedChangedEvents) mReceivedChangedEvents.Clear();
		lock (mReceivedChangedAsyncEvents) mReceivedChangedAsyncEvents.Clear();
		lock (mReceivedViewerChangedEvents) mReceivedViewerChangedEvents.Clear();
		lock (mReceivedViewerChangedAsyncEvents) mReceivedViewerChangedAsyncEvents.Clear();

		if (!recursive)
			return;

		foreach (ExpectedDataNode child in Items)
		{
			child.ResetReceivedEvents();
		}
	}

	private void ChangedHandler(object sender, DataNodeCollectionChangedEventArgs e)
	{
		lock (mReceivedChangedEvents)
		{
			mReceivedChangedEvents.Add(
				new ReceivedChangedEventItem(
					nameof(ChildDataNodeCollection.Changed),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ChangedAsyncHandler(object sender, DataNodeCollectionChangedEventArgs e)
	{
		lock (mReceivedChangedAsyncEvents)
		{
			mReceivedChangedAsyncEvents.Add(
				new ReceivedChangedEventItem(
					nameof(ChildDataNodeCollection.ChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ViewerChangedHandler(object sender, ViewerDataNodeCollectionChangedEventArgs e)
	{
		lock (mReceivedViewerChangedEvents)
		{
			mReceivedViewerChangedEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(ChildDataNodeCollection.ViewerChanged),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ViewerChangedAsyncHandler(object sender, ViewerDataNodeCollectionChangedEventArgs e)
	{
		lock (mReceivedViewerChangedAsyncEvents)
		{
			mReceivedViewerChangedAsyncEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(ChildDataNodeCollection.ViewerChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	#endregion

	#region AssertActualTreeEqualsExpectedTreeInternal()

	/// <summary>
	/// Checks whether the bound actual data tree matches the current expected data tree (for internal use only).
	/// </summary>
	internal void AssertActualTreeEqualsExpectedTreeInternal()
	{
		ExpectedDataNode[] expectedChildren = Items.Where(x => !x.IsDummy).ToArray();
		Assert.Equal(expectedChildren.Length, ActualCollection.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, DataNode actualChildDataNode) in expectedChildren.Zip(
			         ActualCollection,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Same(expectedChildDataNode.ActualDataNode, actualChildDataNode);
			expectedChildDataNode.AssertActualTreeEqualsExpectedTreeInternal();
		}
	}

	/// <summary>
	/// Checks whether the bound actual viewer data tree matches the current expected data tree (for internal use only).
	/// </summary>
	/// <param name="actualViewerDataNode">Actual viewer data node to compare the current expected viewer data node with.</param>
	internal void AssertActualTreeEqualsExpectedTreeInternal(ViewerDataNode actualViewerDataNode)
	{
		ExpectedDataNode[] expectedViewerChildren = [.. Items];
		Assert.Equal(expectedViewerChildren.Length, actualViewerDataNode.Children.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, ViewerDataNode actualViewerChildDataNode) in expectedViewerChildren.Zip(
			         actualViewerDataNode.Children,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Same(expectedChildDataNode.ActualDataNode.ViewerWrapper, actualViewerChildDataNode);
			expectedChildDataNode.AssertActualTreeEqualsExpectedTreeInternal();
		}
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Adds actual data nodes to contained expected child data nodes and sets <see cref="ExpectedDataNode.ActualDataNode"/>
	/// property to this data node.
	/// </summary>
	internal void InitializeActualDataNode()
	{
		foreach (ExpectedDataNode node in Items)
		{
			node.InitializeActualDataNode(Parent.ActualDataNode);
		}
	}

	/// <summary>
	/// Raises events informing about path changes.
	/// </summary>
	internal void NotifyPathChanged()
	{
		foreach (ExpectedDataNode node in Items)
		{
			node.NotifyPathChanged();
		}
	}

	#endregion
}
