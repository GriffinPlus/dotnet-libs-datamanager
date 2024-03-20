using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;

using Xunit;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A data value in the expected data tree structure used to check whether actual data trees are as expected.
/// </summary>
/// <param name="name">Name of the expected data value.</param>
/// <param name="properties">Properties of the expected data value.</param>
/// <param name="value">Value of the expected data value.</param>
[DebuggerDisplay("Data Value => Name: {Name} | Properties: {Properties} | Path: {Path} | Value: {Value}")]
partial class ExpectedDataValue<T>(
	string                      name,
	DataValuePropertiesInternal properties,
	T                           value)
	: ExpectedItem, IExpectedUntypedDataValue
{
	#region ActualDataValue

	private DataValue<T> mActualDataValue;

	/// <inheritdoc cref="IExpectedUntypedDataValue.ActualDataValue"/>
	public DataValue<T> ActualDataValue
	{
		get => mActualDataValue;
		set
		{
			if (mActualDataValue == value)
				return;

			// unregister event handlers from the old data value
			if (mActualDataValue != null)
			{
				mActualDataValue.Changed -= ChangedHandler;
				mActualDataValue.ChangedAsync -= ChangedAsyncHandler;
				mActualDataValue.UntypedChanged -= UntypedChangedHandler;
				mActualDataValue.UntypedChangedAsync -= UntypedChangedAsyncHandler;
				mActualDataValue.ViewerChanged -= ViewerChangedHandler;
				mActualDataValue.ViewerChangedAsync -= ViewerChangedAsyncHandler;
				mActualDataValue.ViewerUntypedChanged -= ViewerUntypedChangedHandler;
				mActualDataValue.ViewerUntypedChangedAsync -= ViewerUntypedChangedAsyncHandler;
			}

			// update the actual data node
			mActualDataValue = value;

			// clear all expected and received events before registering any event of the new node
			// (the node will immediately fire an initial changed event...)
			ResetExpectedEvents();
			ResetReceivedEvents();

			// register event handlers at the new data value
			// ReSharper disable once InvertIf
			if (mActualDataValue != null)
			{
				// add expected initial 'Changed' event
				mExpectedChangedEvents.Add(
					new ExpectedChangedEventItem(
						nameof(DataValue<T>.Changed),
						mActualDataValue,
						new ExpectedDataValueChangedEventArgs<T>(
							mActualDataValue,
							new ExpectedDataValueSnapshot<T>(Timestamp, mProperties, Value),
							DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate)));

				// register 'Changed' event
				mActualDataValue.Changed += ChangedHandler;

				// add expected initial 'ChangedAsync' event
				mExpectedChangedAsyncEvents.Add(
					new ExpectedChangedEventItem(
						nameof(DataValue<T>.ChangedAsync),
						mActualDataValue,
						new ExpectedDataValueChangedEventArgs<T>(
							mActualDataValue,
							new ExpectedDataValueSnapshot<T>(Timestamp, mProperties, Value),
							DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate)));

				// register 'ChangedAsync' event
				mActualDataValue.ChangedAsync += ChangedAsyncHandler;

				// add expected initial 'UntypedChanged' event
				mExpectedUntypedChangedEvents.Add(
					new ExpectedUntypedChangedEventItem(
						nameof(DataValue<T>.UntypedChanged),
						mActualDataValue,
						new ExpectedUntypedDataValueChangedEventArgs(
							mActualDataValue,
							new ExpectedUntypedDataValueSnapshot(Timestamp, mProperties, Value),
							DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate)));

				// register 'UntypedChanged' event
				mActualDataValue.UntypedChanged += UntypedChangedHandler;

				// add expected initial 'UntypedChangedAsync' event
				mExpectedUntypedChangedAsyncEvents.Add(
					new ExpectedUntypedChangedEventItem(
						nameof(DataValue<T>.UntypedChangedAsync),
						mActualDataValue,
						new ExpectedUntypedDataValueChangedEventArgs(
							mActualDataValue,
							new ExpectedUntypedDataValueSnapshot(Timestamp, mProperties, Value),
							DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate)));

				// register 'UntypedChangedAsync' event
				mActualDataValue.UntypedChangedAsync += UntypedChangedAsyncHandler;

				// add expected initial 'ViewerChanged' event
				mExpectedViewerChangedEvents.Add(
					new ExpectedViewerChangedEventItem(
						nameof(DataValue<T>.ViewerChanged),
						mActualDataValue.ViewerWrapper,
						new ExpectedViewerDataValueChangedEventArgs<T>(
							mActualDataValue.ViewerWrapper,
							new ExpectedViewerDataValueSnapshot<T>(Timestamp, mProperties, Value),
							ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate)));

				// register 'ViewerChanged' event
				mActualDataValue.ViewerChanged += ViewerChangedHandler;

				// add expected initial 'ViewerChangedAsync' event
				mExpectedViewerChangedAsyncEvents.Add(
					new ExpectedViewerChangedEventItem(
						nameof(DataValue<T>.ViewerChangedAsync),
						mActualDataValue.ViewerWrapper,
						new ExpectedViewerDataValueChangedEventArgs<T>(
							mActualDataValue.ViewerWrapper,
							new ExpectedViewerDataValueSnapshot<T>(Timestamp, mProperties, Value),
							ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate)));

				// register 'ViewerChangedAsync' event
				mActualDataValue.ViewerChangedAsync += ViewerChangedAsyncHandler;

				// add expected initial 'ViewerUntypedChanged' event
				mExpectedViewerUntypedChangedEvents.Add(
					new ExpectedViewerUntypedChangedEventItem(
						nameof(DataValue<T>.ViewerUntypedChanged),
						mActualDataValue.ViewerWrapper,
						new ExpectedUntypedViewerDataValueChangedEventArgs(
							mActualDataValue.ViewerWrapper,
							new ExpectedUntypedViewerDataValueSnapshot(Timestamp, mProperties, Value),
							ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate)));

				// register 'ViewerUntypedChanged' event
				mActualDataValue.ViewerUntypedChanged += ViewerUntypedChangedHandler;

				// add expected initial 'ViewerUntypedChangedAsync' event
				mExpectedViewerUntypedChangedAsyncEvents.Add(
					new ExpectedViewerUntypedChangedEventItem(
						nameof(DataValue<T>.ViewerUntypedChangedAsync),
						mActualDataValue.ViewerWrapper,
						new ExpectedUntypedViewerDataValueChangedEventArgs(
							mActualDataValue.ViewerWrapper,
							new ExpectedUntypedViewerDataValueSnapshot(Timestamp, mProperties, Value),
							ViewerDataValueChangedFlags.All | ViewerDataValueChangedFlags.InitialUpdate)));

				// register 'ViewerUntypedChangedAsync' event
				mActualDataValue.ViewerUntypedChangedAsync += ViewerUntypedChangedAsyncHandler;
			}
		}
	}

	/// <inheritdoc/>
	IUntypedDataValueInternal IExpectedUntypedDataValue.ActualDataValue
	{
		get => mActualDataValue;
		set => ActualDataValue = (DataValue<T>)value;
	}

	#endregion

	#region Name

	/// <inheritdoc/>
	public string Name { get; set; } = name;

	#endregion

	#region Path

	/// <inheritdoc/>
	public string Path
	{
		get
		{
			// warning: does not consider escaping issues...
			if (Parent == null) return "/";
			string parentPath = Parent.Path;
			if (parentPath.Length == 1) return Parent.Path + Name;
			return Parent.Path + "/" + Name;
		}
	}

	/// <inheritdoc/>
	void IExpectedUntypedDataValue.NotifyPathChanged()
	{
		// add expected regular events
		// ------------------------------------------------------------------------------------

		const DataValueChangedFlags changedFlags = DataValueChangedFlags.Path;

		// expect 'Changed' event to be raised
		mExpectedChangedEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataValue<T>.Changed),
				ActualDataValue,
				new ExpectedDataValueChangedEventArgs<T>(
					ActualDataValue,
					new ExpectedDataValueSnapshot<T>(Timestamp, mProperties, Value),
					changedFlags)));

		// expect 'ChangedAsync' event to be raised
		mExpectedChangedAsyncEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataValue<T>.ChangedAsync),
				ActualDataValue,
				new ExpectedDataValueChangedEventArgs<T>(
					ActualDataValue,
					new ExpectedDataValueSnapshot<T>(Timestamp, mProperties, Value),
					changedFlags)));

		// expect 'UntypedChanged' event to be raised
		mExpectedUntypedChangedEvents.Add(
			new ExpectedUntypedChangedEventItem(
				nameof(DataValue<T>.UntypedChanged),
				ActualDataValue,
				new ExpectedUntypedDataValueChangedEventArgs(
					ActualDataValue,
					new ExpectedUntypedDataValueSnapshot(Timestamp, mProperties, Value),
					changedFlags)));

		// expect 'UntypedChangedAsync' event to be raised
		mExpectedUntypedChangedAsyncEvents.Add(
			new ExpectedUntypedChangedEventItem(
				nameof(DataValue<T>.UntypedChangedAsync),
				ActualDataValue,
				new ExpectedUntypedDataValueChangedEventArgs(
					ActualDataValue,
					new ExpectedUntypedDataValueSnapshot(Timestamp, mProperties, Value),
					changedFlags)));

		// add expected viewer events
		// ------------------------------------------------------------------------------------
		const ViewerDataValueChangedFlags viewerChangedFlags = ViewerDataValueChangedFlags.Path;

		// expect 'ViewerChanged' event to be raised
		mExpectedViewerChangedEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataValue<T>.ViewerChanged),
				ActualDataValue.ViewerWrapper,
				new ExpectedViewerDataValueChangedEventArgs<T>(
					ActualDataValue.ViewerWrapper,
					new ExpectedViewerDataValueSnapshot<T>(Timestamp, mProperties, Value),
					viewerChangedFlags)));

		// expect 'ViewerChangedAsync' event to be raised
		mExpectedViewerChangedAsyncEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataValue<T>.ViewerChangedAsync),
				ActualDataValue.ViewerWrapper,
				new ExpectedViewerDataValueChangedEventArgs<T>(
					ActualDataValue.ViewerWrapper,
					new ExpectedViewerDataValueSnapshot<T>(Timestamp, mProperties, Value),
					viewerChangedFlags)));

		// expect 'ViewerUntypedChanged' event to be raised
		mExpectedViewerUntypedChangedEvents.Add(
			new ExpectedViewerUntypedChangedEventItem(
				nameof(DataValue<T>.ViewerUntypedChanged),
				ActualDataValue.ViewerWrapper,
				new ExpectedUntypedViewerDataValueChangedEventArgs(
					ActualDataValue.ViewerWrapper,
					new ExpectedUntypedViewerDataValueSnapshot(Timestamp, mProperties, Value),
					viewerChangedFlags)));

		// expect 'ViewerChangedAsync' event to be raised
		mExpectedViewerUntypedChangedAsyncEvents.Add(
			new ExpectedViewerUntypedChangedEventItem(
				nameof(DataValue<T>.ViewerUntypedChangedAsync),
				ActualDataValue.ViewerWrapper,
				new ExpectedUntypedViewerDataValueChangedEventArgs(
					ActualDataValue.ViewerWrapper,
					new ExpectedUntypedViewerDataValueSnapshot(Timestamp, mProperties, Value),
					viewerChangedFlags)));
	}

	#endregion

	#region Properties

	private DataValuePropertiesInternal mProperties = properties;

	/// <inheritdoc/>
	public DataValueProperties Properties
	{
		get => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);
		set => mProperties = (DataValuePropertiesInternal)value; // TODO: add expected events...
	}

	#endregion

	#region IsPersistent

	/// <inheritdoc/>
	public bool IsPersistent => mProperties.HasFlag(DataValuePropertiesInternal.Persistent);

	#endregion

	#region IsDummy

	/// <inheritdoc/>
	public bool IsDummy => mProperties.HasFlag(DataValuePropertiesInternal.Dummy);

	#endregion

	#region IsDummy

	/// <inheritdoc/>
	public bool IsDetached => mProperties.HasFlag(DataValuePropertiesInternal.Detached);

	#endregion

	#region RootNode

	/// <inheritdoc/>
	public ExpectedDataNode RootNode => Parent.RootNode;

	#endregion

	#region Timestamp

	/// <inheritdoc/>
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;

	#endregion

	#region Type

	/// <inheritdoc/>
	public Type Type => typeof(T);

	#endregion

	#region Value

	/// <inheritdoc cref="IExpectedUntypedDataValue.Value"/>
	public T Value { get; set; } = value;

	/// <inheritdoc/>
	object IExpectedUntypedDataValue.Value
	{
		get => Value;
		set => Value = (T)value;
	}

	#endregion

	#region InitializeActualDataValue()

	/// <inheritdoc/>
	void IExpectedUntypedDataValue.InitializeActualDataValue(DataNode actualParent)
	{
		ActualDataValue = actualParent.Values.AddInternalUnsynced(Name.AsSpan(), mProperties, Value, Timestamp);
	}

	#endregion

	#region BindAndValidateActualDataTreeInternal()

	/// <inheritdoc/>
	public void AssertConsistency()
	{
		// IUntypedDataValue
		Assert.Equal(Name, ActualDataValue.Name);
		Assert.Equal(Path, ActualDataValue.Path);
		Assert.Equal(Timestamp, ActualDataValue.Timestamp);
		Assert.Equal(Type, ActualDataValue.Type);
		Assert.Equal(Value, ActualDataValue.Value);
		Assert.Equal(Properties, ActualDataValue.Properties);
		Assert.Equal(IsDetached, ActualDataValue.IsDetached);
		Assert.Equal(IsPersistent, ActualDataValue.IsPersistent);
		Assert.Equal(Parent.ActualDataNode, ActualDataValue.ParentNode);
		Assert.Same(RootNode.ActualDataNode, ActualDataValue.RootNode);
		Assert.Same(RootNode.ActualDataNode.DataTreeManager, ActualDataValue.DataTreeManager);

		// IUntypedDataValueInternal
		Assert.Equal(Name, ActualDataValue.NameUnsynced);
		Assert.Equal(Path, ActualDataValue.PathUnsynced);
		Assert.Equal(Timestamp, ActualDataValue.TimestampUnsynced);
		Assert.Equal(Value, ActualDataValue.ValueUnsynced);
		Assert.Equal(mProperties, ActualDataValue.PropertiesUnsynced);
		Assert.Equal(IsDummy, ActualDataValue.IsDummyUnsynced);
		Assert.Equal(Parent.ActualDataNode, ActualDataValue.ParentNodeUnsynced);
		Assert.Equal(RootNode.ActualDataNode, ActualDataValue.RootNodeUnsynced);
	}

	/// <inheritdoc/>
	public void CheckViewerConsistency(IUntypedViewerDataValue actualViewerDataValue)
	{
		Assert.NotNull(actualViewerDataValue);

		// unwrapping the viewer data value should return the current data value
		Assert.Same(ActualDataValue, actualViewerDataValue.WrappedValue);

		// IUntypedViewerDataValue
		Assert.Equal(Name, actualViewerDataValue.Name);
		Assert.Equal(Path, actualViewerDataValue.Path);
		Assert.Equal(Timestamp, actualViewerDataValue.Timestamp);
		Assert.Equal(Type, actualViewerDataValue.Type);
		Assert.Equal(Value, actualViewerDataValue.Value);
		Assert.Equal(Properties, actualViewerDataValue.Properties);
		Assert.Equal(IsDetached, actualViewerDataValue.IsDetached);
		Assert.Equal(IsPersistent, actualViewerDataValue.IsPersistent);
		Assert.Equal(IsDummy, actualViewerDataValue.IsDummy);
		Assert.Equal(Parent.ActualDataNode.ViewerWrapper, actualViewerDataValue.ParentNode);
		Assert.Same(RootNode.ActualDataNode.ViewerWrapper, actualViewerDataValue.RootNode);
		Assert.Same(RootNode.ActualDataNode.DataTreeManager, actualViewerDataValue.DataTreeManager);
	}

	/// <inheritdoc/>
	public void BindAndValidateActualDataTreeInternal(IUntypedDataValueInternal actualDataValue)
	{
		Assert.NotNull(actualDataValue);
		ActualDataValue = (DataValue<T>)actualDataValue;
		AssertConsistency();
		CheckViewerConsistency(actualDataValue.ViewerWrapper);
	}

	#endregion

	#region AssertExpectedEventsWereRaised()

	/// <inheritdoc/>
	public void AssertExpectedEventsWereRaised()
	{
		// check whether 'Changed' events and 'ChangedAsync' events were raised properly
		AssertExpectedChangedEventsWereRaised(mExpectedChangedEvents, ReceivedChangedEvents, SynchronizationContext.Current);
		AssertExpectedChangedEventsWereRaised(mExpectedChangedAsyncEvents, ReceivedChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether 'UntypedChanged' events and 'UntypedChangedAsync' events were raised properly
		AssertExpectedChangedEventsWereRaised(mExpectedUntypedChangedEvents, ReceivedUntypedChangedEvents, SynchronizationContext.Current);
		AssertExpectedChangedEventsWereRaised(mExpectedUntypedChangedAsyncEvents, ReceivedUntypedChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether 'ViewerChanged' events and 'ViewerChangedAsync' events were raised properly
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedEvents, ReceivedViewerChangedEvents, SynchronizationContext.Current);
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedAsyncEvents, ReceivedViewerChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether 'ViewerUntypedChanged' events and 'ViewerUntypedChangedAsync' events were raised properly
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerUntypedChangedEvents, ReceivedUntypedViewerChangedEvents, SynchronizationContext.Current);
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerUntypedChangedAsyncEvents, ReceivedUntypedViewerChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);
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
			Assert.Same(expected.EventArgs.DataValue, actual.EventArgs.DataValue);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.Snapshot.IsDetached, actual.EventArgs.Snapshot.IsDetached);
			Assert.Equal(expected.EventArgs.Snapshot.Timestamp, actual.EventArgs.Snapshot.Timestamp);
			Assert.Equal(expected.EventArgs.Snapshot.Value, actual.EventArgs.Snapshot.Value);
			Assert.Equal(expected.EventArgs.ChangedFlags, actual.EventArgs.ChangedFlags);
		}
	}

	/// <summary>
	/// Checks whether the specified expected events equal the specified received events (regular events).
	/// </summary>
	/// <param name="expectedChangedEvents">The expected events.</param>
	/// <param name="receivedChangedEvents">The received events.</param>
	/// <param name="expectedSynchronizationContext">Synchronization context of the thread that should have executed the event handler.</param>
	private static void AssertExpectedChangedEventsWereRaised(
		IReadOnlyCollection<ExpectedUntypedChangedEventItem> expectedChangedEvents,
		IReadOnlyCollection<ReceivedUntypedChangedEventItem> receivedChangedEvents,
		SynchronizationContext                               expectedSynchronizationContext)
	{
		Assert.Equal(expectedChangedEvents.Count, receivedChangedEvents.Count);

		foreach ((ExpectedUntypedChangedEventItem expected, ReceivedUntypedChangedEventItem actual) in expectedChangedEvents.Zip(
			         receivedChangedEvents,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// check whether the event name is as expected
			Assert.Equal(expected.EventName, actual.EventName);

			// check whether the event was raised in the expected synchronization context
			Assert.Same(expectedSynchronizationContext, actual.SynchronizationContext);

			// check whether event sender and arguments are as expected
			Assert.Same(expected.Sender, actual.Sender);
			Assert.Same(expected.EventArgs.DataValue, actual.EventArgs.DataValue);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.Snapshot.IsDetached, actual.EventArgs.Snapshot.IsDetached);
			Assert.Equal(expected.EventArgs.Snapshot.Timestamp, actual.EventArgs.Snapshot.Timestamp);
			Assert.Equal(expected.EventArgs.Snapshot.Value, actual.EventArgs.Snapshot.Value);
			Assert.Equal(expected.EventArgs.ChangedFlags, actual.EventArgs.ChangedFlags);
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
			Assert.Same(expected.EventArgs.DataValue, actual.EventArgs.DataValue);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.Snapshot.IsDetached, actual.EventArgs.Snapshot.IsDetached);
			Assert.Equal(expected.EventArgs.Snapshot.Timestamp, actual.EventArgs.Snapshot.Timestamp);
			Assert.Equal(expected.EventArgs.Snapshot.Value, actual.EventArgs.Snapshot.Value);
			Assert.Equal(expected.EventArgs.ChangedFlags, actual.EventArgs.ChangedFlags);
		}
	}

	/// <summary>
	/// Checks whether the specified expected events equal the specified received events (viewer events).
	/// </summary>
	/// <param name="expectedChangedEvents">The expected events.</param>
	/// <param name="receivedChangedEvents">The received events.</param>
	/// <param name="expectedSynchronizationContext">Synchronization context of the thread that should have executed the event handler.</param>
	private static void AssertExpectedViewerChangedEventsWereRaised(
		IReadOnlyCollection<ExpectedViewerUntypedChangedEventItem> expectedChangedEvents,
		IReadOnlyCollection<ReceivedViewerUntypedChangedEventItem> receivedChangedEvents,
		SynchronizationContext                                     expectedSynchronizationContext)
	{
		Assert.Equal(expectedChangedEvents.Count, receivedChangedEvents.Count);

		foreach ((ExpectedViewerUntypedChangedEventItem expected, ReceivedViewerUntypedChangedEventItem actual) in expectedChangedEvents.Zip(
			         receivedChangedEvents,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// check whether the event name is as expected
			Assert.Equal(expected.EventName, actual.EventName);

			// check whether the event was raised in the expected synchronization context
			Assert.Same(expectedSynchronizationContext, actual.SynchronizationContext);

			// check whether event sender and arguments are as expected
			Assert.Same(expected.Sender, actual.Sender);
			Assert.Same(expected.EventArgs.DataValue, actual.EventArgs.DataValue);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.Snapshot.IsDetached, actual.EventArgs.Snapshot.IsDetached);
			Assert.Equal(expected.EventArgs.Snapshot.Timestamp, actual.EventArgs.Snapshot.Timestamp);
			Assert.Equal(expected.EventArgs.Snapshot.Value, actual.EventArgs.Snapshot.Value);
			Assert.Equal(expected.EventArgs.ChangedFlags, actual.EventArgs.ChangedFlags);
		}
	}

	#endregion

	#region Event Monitoring

	private readonly List<ExpectedChangedEventItem>              mExpectedChangedEvents                   = [];
	private readonly List<ExpectedChangedEventItem>              mExpectedChangedAsyncEvents              = [];
	private readonly List<ExpectedUntypedChangedEventItem>       mExpectedUntypedChangedEvents            = [];
	private readonly List<ExpectedUntypedChangedEventItem>       mExpectedUntypedChangedAsyncEvents       = [];
	private readonly List<ExpectedViewerChangedEventItem>        mExpectedViewerChangedEvents             = [];
	private readonly List<ExpectedViewerChangedEventItem>        mExpectedViewerChangedAsyncEvents        = [];
	private readonly List<ExpectedViewerUntypedChangedEventItem> mExpectedViewerUntypedChangedEvents      = [];
	private readonly List<ExpectedViewerUntypedChangedEventItem> mExpectedViewerUntypedChangedAsyncEvents = [];

	private readonly List<ReceivedChangedEventItem>              mReceivedChangedEvents                   = [];
	private readonly List<ReceivedChangedEventItem>              mReceivedChangedAsyncEvents              = [];
	private readonly List<ReceivedUntypedChangedEventItem>       mReceivedUntypedChangedEvents            = [];
	private readonly List<ReceivedUntypedChangedEventItem>       mReceivedUntypedChangedAsyncEvents       = [];
	private readonly List<ReceivedViewerChangedEventItem>        mReceivedViewerChangedEvents             = [];
	private readonly List<ReceivedViewerChangedEventItem>        mReceivedViewerChangedAsyncEvents        = [];
	private readonly List<ReceivedViewerUntypedChangedEventItem> mReceivedViewerUntypedChangedEvents      = [];
	private readonly List<ReceivedViewerUntypedChangedEventItem> mReceivedViewerUntypedChangedAsyncEvents = [];

	/// <summary>
	/// Gets the expected <see cref="DataValue{T}.Changed"/> events.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedEvents => [.. mExpectedChangedEvents];

	/// <summary>
	/// Gets the expected <see cref="DataValue{T}.ChangedAsync"/> events.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedAsync => [.. mExpectedChangedAsyncEvents];

	/// <summary>
	/// Gets the expected <see cref="DataValue{T}.UntypedChanged"/> events.
	/// </summary>
	public ExpectedUntypedChangedEventItem[] ExpectedUntypedChangedEvents => [.. mExpectedUntypedChangedEvents];

	/// <summary>
	/// Gets the expected <see cref="DataValue{T}.UntypedChangedAsync"/> events.
	/// </summary>
	public ExpectedUntypedChangedEventItem[] ExpectedUntypedChangedAsync => [.. mExpectedUntypedChangedAsyncEvents];

	/// <summary>
	/// Gets the expected <see cref="ViewerDataValue{T}.Changed"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerChanged"/> events.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedEvents => [.. mExpectedViewerChangedEvents];

	/// <summary>
	/// Gets the expected <see cref="ViewerDataValue{T}.ChangedAsync"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerChangedAsync"/> events.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedAsync => [.. mExpectedViewerChangedAsyncEvents];

	/// <summary>
	/// Gets the expected <see cref="ViewerDataValue{T}.UntypedChanged"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChanged"/> events.
	/// </summary>
	public ExpectedViewerUntypedChangedEventItem[] ExpectedUntypedViewerChangedEvents => [.. mExpectedViewerUntypedChangedEvents];

	/// <summary>
	/// Gets the expected <see cref="ViewerDataValue{T}.UntypedChangedAsync"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChangedAsync"/> events.
	/// </summary>
	public ExpectedViewerUntypedChangedEventItem[] ExpectedUntypedViewerChangedAsync => [.. mExpectedViewerUntypedChangedAsyncEvents];

	/// <summary>
	/// Gets the expected <see cref="DataValue{T}.Changed"/> events.
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
	/// Gets the received <see cref="DataValue{T}.ChangedAsync"/> events.
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
	/// Gets the received <see cref="DataValue{T}.UntypedChanged"/> events.
	/// </summary>
	public ReceivedUntypedChangedEventItem[] ReceivedUntypedChangedEvents
	{
		get
		{
			lock (mReceivedUntypedChangedEvents)
			{
				return [.. mReceivedUntypedChangedEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="DataValue{T}.UntypedChangedAsync"/> events.
	/// </summary>
	public ReceivedUntypedChangedEventItem[] ReceivedUntypedChangedAsyncEvents
	{
		get
		{
			lock (mReceivedUntypedChangedAsyncEvents)
			{
				return [.. mReceivedUntypedChangedAsyncEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="ViewerDataValue{T}.Changed"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerChanged"/> events.
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
	/// Gets the received <see cref="ViewerDataValue{T}.ChangedAsync"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerChangedAsync"/> events.
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
	/// Gets the received <see cref="ViewerDataValue{T}.UntypedChanged"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChanged"/> events.
	/// </summary>
	public ReceivedViewerUntypedChangedEventItem[] ReceivedUntypedViewerChangedEvents
	{
		get
		{
			lock (mReceivedUntypedChangedEvents)
			{
				return [.. mReceivedViewerUntypedChangedEvents];
			}
		}
	}

	/// <summary>
	/// Gets the received <see cref="ViewerDataValue{T}.UntypedChangedAsync"/> events,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChangedAsync"/> events.
	/// </summary>
	public ReceivedViewerUntypedChangedEventItem[] ReceivedUntypedViewerChangedAsyncEvents
	{
		get
		{
			lock (mReceivedUntypedChangedAsyncEvents)
			{
				return [.. mReceivedViewerUntypedChangedAsyncEvents];
			}
		}
	}

	/// <inheritdoc/>
	public void ResetExpectedEvents()
	{
		mExpectedChangedEvents.Clear();
		mExpectedChangedAsyncEvents.Clear();
		mExpectedUntypedChangedEvents.Clear();
		mExpectedUntypedChangedAsyncEvents.Clear();
		mExpectedViewerChangedEvents.Clear();
		mExpectedViewerChangedAsyncEvents.Clear();
		mExpectedViewerUntypedChangedEvents.Clear();
		mExpectedViewerUntypedChangedAsyncEvents.Clear();
	}

	/// <inheritdoc/>
	public void ResetReceivedEvents()
	{
		lock (mReceivedChangedEvents) mReceivedChangedEvents.Clear();
		lock (mReceivedChangedAsyncEvents) mReceivedChangedAsyncEvents.Clear();
		lock (mReceivedUntypedChangedEvents) mReceivedUntypedChangedEvents.Clear();
		lock (mReceivedUntypedChangedAsyncEvents) mReceivedUntypedChangedAsyncEvents.Clear();
		lock (mReceivedViewerChangedEvents) mReceivedViewerChangedEvents.Clear();
		lock (mReceivedViewerChangedAsyncEvents) mReceivedViewerChangedAsyncEvents.Clear();
		lock (mReceivedViewerUntypedChangedEvents) mReceivedViewerUntypedChangedEvents.Clear();
		lock (mReceivedViewerUntypedChangedAsyncEvents) mReceivedViewerUntypedChangedAsyncEvents.Clear();
	}

	/// <summary>
	/// Handler for the <see cref="DataValue{T}.Changed"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ChangedHandler(object sender, DataValueChangedEventArgs<T> e)
	{
		lock (mReceivedChangedEvents)
		{
			mReceivedChangedEvents.Add(
				new ReceivedChangedEventItem(
					nameof(DataValue<T>.Changed),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="DataValue{T}.ChangedAsync"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ChangedAsyncHandler(object sender, DataValueChangedEventArgs<T> e)
	{
		lock (mReceivedChangedAsyncEvents)
		{
			mReceivedChangedAsyncEvents.Add(
				new ReceivedChangedEventItem(
					nameof(DataValue<T>.ChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="DataValue{T}.UntypedChanged"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void UntypedChangedHandler(object sender, UntypedDataValueChangedEventArgs e)
	{
		lock (mReceivedUntypedChangedEvents)
		{
			mReceivedUntypedChangedEvents.Add(
				new ReceivedUntypedChangedEventItem(
					nameof(DataValue<T>.UntypedChanged),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="DataValue{T}.UntypedChangedAsync"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void UntypedChangedAsyncHandler(object sender, UntypedDataValueChangedEventArgs e)
	{
		lock (mReceivedUntypedChangedAsyncEvents)
		{
			mReceivedUntypedChangedAsyncEvents.Add(
				new ReceivedUntypedChangedEventItem(
					nameof(DataValue<T>.UntypedChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="ViewerDataValue{T}.Changed"/> event,
	/// respectively the <see cref="DataValue{T}.ViewerChanged"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ViewerChangedHandler(object sender, ViewerDataValueChangedEventArgs<T> e)
	{
		lock (mReceivedViewerChangedEvents)
		{
			mReceivedViewerChangedEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(DataValue<T>.ViewerChanged),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="ViewerDataValue{T}.ChangedAsync"/> event,
	/// respectively the <see cref="DataValue{T}.ViewerChangedAsync"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ViewerChangedAsyncHandler(object sender, ViewerDataValueChangedEventArgs<T> e)
	{
		lock (mReceivedViewerChangedAsyncEvents)
		{
			mReceivedViewerChangedAsyncEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(DataValue<T>.ViewerChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="ViewerDataValue{T}.UntypedChanged"/> event,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChanged"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ViewerUntypedChangedHandler(object sender, UntypedViewerDataValueChangedEventArgs e)
	{
		lock (mReceivedViewerUntypedChangedEvents)
		{
			mReceivedViewerUntypedChangedEvents.Add(
				new ReceivedViewerUntypedChangedEventItem(
					nameof(DataValue<T>.ViewerUntypedChanged),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	/// <summary>
	/// Handler for the <see cref="ViewerDataValue{T}.UntypedChangedAsync"/> event,
	/// respectively the <see cref="DataValue{T}.ViewerUntypedChangedAsync"/> event.
	/// </summary>
	/// <param name="sender">Sender of the event (the data value itself).</param>
	/// <param name="e">Event arguments.</param>
	private void ViewerUntypedChangedAsyncHandler(object sender, UntypedViewerDataValueChangedEventArgs e)
	{
		lock (mReceivedViewerUntypedChangedAsyncEvents)
		{
			mReceivedViewerUntypedChangedAsyncEvents.Add(
				new ReceivedViewerUntypedChangedEventItem(
					nameof(DataValue<T>.ViewerUntypedChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	#endregion
}
