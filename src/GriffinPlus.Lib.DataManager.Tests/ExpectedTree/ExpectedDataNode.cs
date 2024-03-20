using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Threading;

using Xunit;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An expected data node.
/// </summary>
[DebuggerDisplay("Data Node => Name: {Name} | Properties: {Properties} | Path: {Path}")]
partial class ExpectedDataNode : ExpectedItem
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedDataNode"/> class.
	/// </summary>
	/// <param name="name">Name of the expected data node.</param>
	/// <param name="properties">Properties of the expected data node.</param>
	/// <param name="actualDataNode">The actual data node to associate the expected data node with.</param>
	public ExpectedDataNode(string name, DataNodePropertiesInternal properties, DataNode actualDataNode = null)
	{
		ActualDataNode = actualDataNode;
		Name = name;
		mProperties = properties;
		mChildren.CollectionChanged += EH_Children_CollectionChanged;
		mValues.CollectionChanged += EH_Values_CollectionChanged;
	}

	#region ActualDataNode

	private DataNode mActualDataNode;

	/// <summary>
	/// Gets or sets the actual data node associated with the current expected data node.
	/// </summary>
	public DataNode ActualDataNode
	{
		get => mActualDataNode;
		set
		{
			if (mActualDataNode == value)
				return;

			// unregister event handlers from the old data node
			if (mActualDataNode != null)
			{
				mActualDataNode.Changed -= ChangedHandler;
				mActualDataNode.ChangedAsync -= ChangedAsyncHandler;
				mActualDataNode.ViewerChanged -= ViewerChangedHandler;
				mActualDataNode.ViewerChangedAsync -= ViewerChangedAsyncHandler;
			}

			mActualDataNode = value;

			// clear all expected and received events before registering any event of the new node
			// (the node will immediately fire an initial changed event...)
			ResetExpectedEvents(false);
			ResetReceivedEvents(false);

			// register event handlers at the new data node
			// ReSharper disable once InvertIf
			if (mActualDataNode != null)
			{
				// add expected initial 'Changed' event
				mExpectedChangedEvents.Add(
					new ExpectedChangedEventItem(
						nameof(DataNode.Changed),
						mActualDataNode,
						new ExpectedDataNodeChangedEventArgs(
							mActualDataNode,
							new ExpectedDataNodeSnapshot(Name, Path, mProperties),
							DataNodeChangedFlags.All | DataNodeChangedFlags.InitialUpdate)));

				// register 'Changed' event
				mActualDataNode.Changed += ChangedHandler;

				// add expected initial 'ChangedAsync' event
				mExpectedChangedAsyncEvents.Add(
					new ExpectedChangedEventItem(
						nameof(DataNode.ChangedAsync),
						mActualDataNode,
						new ExpectedDataNodeChangedEventArgs(
							mActualDataNode,
							new ExpectedDataNodeSnapshot(Name, Path, mProperties),
							DataNodeChangedFlags.All | DataNodeChangedFlags.InitialUpdate)));

				// register 'ChangedAsync' event
				mActualDataNode.ChangedAsync += ChangedAsyncHandler;

				// add expected initial 'ViewerChanged' event
				mExpectedViewerChangedEvents.Add(
					new ExpectedViewerChangedEventItem(
						nameof(DataNode.ViewerChanged),
						mActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeChangedEventArgs(
							mActualDataNode.ViewerWrapper,
							new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
							ViewerDataNodeChangedFlags.All | ViewerDataNodeChangedFlags.InitialUpdate)));

				// register 'ViewerChanged' event
				mActualDataNode.ViewerChanged += ViewerChangedHandler;

				// add expected initial 'ViewerChangedAsync' event
				mExpectedViewerChangedAsyncEvents.Add(
					new ExpectedViewerChangedEventItem(
						nameof(DataNode.ViewerChangedAsync),
						mActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeChangedEventArgs(
							mActualDataNode.ViewerWrapper,
							new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
							ViewerDataNodeChangedFlags.All | ViewerDataNodeChangedFlags.InitialUpdate)));

				// register 'ViewerChangedAsync' event
				mActualDataNode.ViewerChangedAsync += ViewerChangedAsyncHandler;
			}
		}
	}

	#endregion

	#region Children

	private ObservableCollection<ExpectedDataNode> mChildren = [];

	/// <summary>
	/// Gets or sets the child data nodes of the current expected data node.
	/// </summary>
	public ObservableCollection<ExpectedDataNode> Children
	{
		get => mChildren;
		set
		{
			mChildren.CollectionChanged -= EH_Children_CollectionChanged;
			mChildren = value;
			mChildren.CollectionChanged += EH_Children_CollectionChanged;
			SetParentToThis(mChildren);
		}
	}

	private void EH_Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
			// add: some items have been added
			// => add parent to new items
			case NotifyCollectionChangedAction.Add:
				SetParentToThis(e.NewItems!.Cast<IExpectedItem>());
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
				SetParentToThis(e.NewItems!.Cast<IExpectedItem>());
				break;

			// move: no items added/removed
			// => nothing to do
			case NotifyCollectionChangedAction.Move:
				break;

			// reset: everything (might) have changed
			// => process all items in the collection
			// NOTE: the parent of old items is not cleared!!!
			case NotifyCollectionChangedAction.Reset:
				SetParentToThis(mChildren);
				break;

			// unhandled action
			default:
				throw new ArgumentOutOfRangeException(
					nameof(e),
					e.Action,
					"The value of the NotifyCollectionChangedEventArgs.Action property is unhandled.");
		}
	}

	#endregion

	#region Name

	private string mName;

	/// <summary>
	/// Gets or sets the name of the expected data node.
	/// </summary>
	public string Name
	{
		get => mName;
		set
		{
			// abort if the name does not change
			if (mName == value)
				return;

			// set the expected name of the data node
			mName = value;

			// abort if there is no actual data node, yet
			if (ActualDataNode == null)
				return;

			// add expected regular events
			// ------------------------------------------------------------------------------------
			var changedFlags = DataNodeChangedFlags.Name;
			changedFlags |= Parent != null ? DataNodeChangedFlags.Path : DataNodeChangedFlags.None;

			// expect 'Changed' event to be raised
			mExpectedChangedEvents.Add(
				new ExpectedChangedEventItem(
					nameof(DataNode.Changed),
					ActualDataNode,
					new ExpectedDataNodeChangedEventArgs(
						ActualDataNode,
						new ExpectedDataNodeSnapshot(Name, Path, mProperties),
						changedFlags)));

			// expect 'ChangedAsync' event to be raised
			mExpectedChangedAsyncEvents.Add(
				new ExpectedChangedEventItem(
					nameof(DataNode.ChangedAsync),
					ActualDataNode,
					new ExpectedDataNodeChangedEventArgs(
						ActualDataNode,
						new ExpectedDataNodeSnapshot(Name, Path, mProperties),
						changedFlags)));

			// add expected viewer events
			// ------------------------------------------------------------------------------------
			var viewerChangedFlags = ViewerDataNodeChangedFlags.Name;
			viewerChangedFlags |= Parent != null ? ViewerDataNodeChangedFlags.Path : ViewerDataNodeChangedFlags.None;

			// expect 'ViewerChanged' event to be raised
			mExpectedViewerChangedEvents.Add(
				new ExpectedViewerChangedEventItem(
					nameof(DataNode.ViewerChanged),
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeChangedEventArgs(
						ActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
						viewerChangedFlags)));

			// expect 'ViewerChangedAsync' event to be raised
			mExpectedViewerChangedAsyncEvents.Add(
				new ExpectedViewerChangedEventItem(
					nameof(DataNode.ViewerChangedAsync),
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeChangedEventArgs(
						ActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
						viewerChangedFlags)));

			// update paths of child nodes a data values, if the change affected a non-root node
			// ------------------------------------------------------------------------------------
			// ReSharper disable once InvertIf
			if (Parent != null)
			{
				foreach (ExpectedDataNode dataNode in Children) dataNode.NotifyPathChanged();
				foreach (IExpectedUntypedDataValue dataValue in Values) dataValue.NotifyPathChanged();
			}
		}
	}

	#endregion

	#region Path

	/// <summary>
	/// Gets the path of the current expected data node.
	/// </summary>
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

	#endregion

	#region Properties

	private DataNodePropertiesInternal mProperties;

	/// <summary>
	/// Gets or sets the properties of the expected data node.
	/// </summary>
	public DataNodeProperties Properties
	{
		get => (DataNodeProperties)(mProperties & DataNodePropertiesInternal.UserProperties);
		set
		{
			DataNodePropertiesInternal newProperties = (mProperties & DataNodePropertiesInternal.AdministrativeProperties) | (DataNodePropertiesInternal)value;

			// make all nodes on the path to this node persistent as well, if the value is regular
			// (for dummy nodes this is done as soon as the node becomes regular)
			if ((newProperties & DataNodePropertiesInternal.Dummy) == 0 && (newProperties & DataNodePropertiesInternal.Persistent) != 0)
			{
				Parent?.MakePersistent();
			}

			// abort if the properties does not change
			if (mProperties == newProperties)
				return;

			// capture old properties
			DataNodePropertiesInternal oldProperties = mProperties;
			DataNodePropertiesInternal oldUserProperties = mProperties & DataNodePropertiesInternal.UserProperties;

			// change properties
			mProperties = newProperties;

			// abort if user properties have not changed
			DataNodePropertiesInternal newUserProperties = mProperties & DataNodePropertiesInternal.UserProperties;
			if (oldUserProperties == newUserProperties)
				return;

			// abort if there is no actual data node, yet
			if (ActualDataNode == null)
				return;

			// add expected regular events
			// ------------------------------------------------------------------------------------

			// determine flags reflecting the property changes
			var changedFlags = DataNodeChangedFlags.None;
			// add changed flags for all properties (user flags + administrative flags)
			changedFlags |= (DataNodeChangedFlags)((~oldProperties & newProperties) | (oldProperties & ~newProperties));
			// add changed flag for the 'Properties' property, if user properties have changed
			changedFlags |= ((~oldUserProperties & newUserProperties) | (oldUserProperties & ~newUserProperties)) != 0
				                ? DataNodeChangedFlags.Properties
				                : DataNodeChangedFlags.None;

			// expect 'Changed' event to be raised
			mExpectedChangedEvents.Add(
				new ExpectedChangedEventItem(
					nameof(DataNode.Changed),
					ActualDataNode,
					new ExpectedDataNodeChangedEventArgs(
						ActualDataNode,
						new ExpectedDataNodeSnapshot(Name, Path, mProperties),
						changedFlags)));

			// expect 'ChangedAsync' event to be raised
			mExpectedChangedAsyncEvents.Add(
				new ExpectedChangedEventItem(
					nameof(DataNode.ChangedAsync),
					ActualDataNode,
					new ExpectedDataNodeChangedEventArgs(
						ActualDataNode,
						new ExpectedDataNodeSnapshot(Name, Path, mProperties),
						changedFlags)));

			// add expected viewer events
			// ------------------------------------------------------------------------------------

			// determine flags reflecting the property changes
			var viewerChangedFlags = ViewerDataNodeChangedFlags.None;
			// add changed flags for all properties (user flags + administrative flags)
			viewerChangedFlags |= (ViewerDataNodeChangedFlags)((~oldProperties & newProperties) | (oldProperties & ~newProperties));
			// add changed flag for the 'Properties' property, if user properties have changed
			viewerChangedFlags |= ((~oldUserProperties & newUserProperties) | (oldUserProperties & ~newUserProperties)) != 0
				                      ? ViewerDataNodeChangedFlags.Properties
				                      : ViewerDataNodeChangedFlags.None;

			// expect 'ViewerChanged' event to be raised
			mExpectedViewerChangedEvents.Add(
				new ExpectedViewerChangedEventItem(
					nameof(DataNode.ViewerChanged),
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeChangedEventArgs(
						ActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
						viewerChangedFlags)));

			// expect 'ViewerChangedAsync' event to be raised
			mExpectedViewerChangedAsyncEvents.Add(
				new ExpectedViewerChangedEventItem(
					nameof(DataNode.ViewerChangedAsync),
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeChangedEventArgs(
						ActualDataNode.ViewerWrapper,
						new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
						viewerChangedFlags)));
		}
	}

	#endregion

	#region IsPersistent

	/// <summary>
	/// Gets or sets the expected value of the <see cref="DataNode.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent
	{
		get => mProperties.HasFlag(DataNodePropertiesInternal.Persistent);
		set
		{
			if (value) Properties |= DataNodeProperties.Persistent;
			else Properties &= ~DataNodeProperties.Persistent;
		}
	}

	#endregion

	#region IsDummy

	/// <summary>
	/// Gets the expected value of the <see cref="DataNode.IsDummyUnsynced"/> property.
	/// </summary>
	public bool IsDummy => mProperties.HasFlag(DataNodePropertiesInternal.Dummy);

	#endregion

	#region RootNode

	/// <summary>
	/// Gets the root node of the current expected data node.
	/// </summary>
	public ExpectedDataNode RootNode => Parent == null ? this : Parent.RootNode;

	#endregion

	#region Values

	private ObservableCollection<IExpectedUntypedDataValue> mValues = [];

	/// <summary>
	/// Gets or sets the data values of the current expected data node.
	/// </summary>
	public ObservableCollection<IExpectedUntypedDataValue> Values
	{
		get => mValues;
		set
		{
			mValues.CollectionChanged -= EH_Values_CollectionChanged;
			mValues = value;
			mValues.CollectionChanged += EH_Values_CollectionChanged;
			SetParentToThis(mValues);
		}
	}

	private void EH_Values_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
			// add: some items have been added
			// => add parent to new items
			case NotifyCollectionChangedAction.Add:
				SetParentToThis(e.NewItems!.Cast<ExpectedItem>());
				break;

			// remove: some items have been removed
			// => reset parent of old items
			case NotifyCollectionChangedAction.Remove:
				ResetParent(e.OldItems!.Cast<ExpectedItem>());
				break;

			// replace: some items are replaced with other items
			// => reset parent of old items and set parent for new items
			case NotifyCollectionChangedAction.Replace:
				ResetParent(e.OldItems!.Cast<ExpectedItem>());
				SetParentToThis(e.NewItems!.Cast<ExpectedItem>());
				break;

			// move: no items added/removed
			// => nothing to do
			case NotifyCollectionChangedAction.Move:
				break;

			// reset: everything (might) have changed
			// => process all items in the collection
			// NOTE: the parent of old items is not cleared!!!
			case NotifyCollectionChangedAction.Reset:
				SetParentToThis(mValues);
				break;

			// unhandled action
			default:
				throw new ArgumentOutOfRangeException(
					nameof(e),
					e.Action,
					"The value of the NotifyCollectionChangedEventArgs.Action property is unhandled.");
		}
	}

	#endregion

	#region InitializeActualDataNode()

	/// <summary>
	/// Creates the actual data tree as specified by the expected data tree and initializes the
	/// <see cref="ActualDataNode"/> properties and <see cref="ExpectedDataValue{T}.ActualDataValue"/>
	/// properties to link the actual data node/value to their expected equivalent.
	/// </summary>
	/// <returns>The root node of the actual data tree.</returns>
	public DataNode InitializeActualDataNode()
	{
		if (Parent != null) throw new InvalidOperationException("Must be called on root nodes only!");
		InitializeActualDataNode(null);
		return ActualDataNode;
	}

	/// <summary>
	/// Adds an actual data node to the specified actual parent node as specified by the current expected data node
	/// and sets <see cref="ActualDataNode"/> property to this data node.
	/// </summary>
	/// <param name="actualParent">Parent of the current node (<c>null</c> if this is the root node).</param>
	private void InitializeActualDataNode(DataNode actualParent)
	{
		if (actualParent == null)
		{
			ActualDataNode = new DataNode(Name, Properties, null, null);

			lock (ActualDataNode.DataTreeManager.Sync)
			{
				foreach (ExpectedDataNode expectedChildNode in Children)
				{
					expectedChildNode.InitializeActualDataNode(ActualDataNode);
				}

				foreach (IExpectedUntypedDataValue expectedValue in Values)
				{
					expectedValue.InitializeActualDataValue(ActualDataNode);
				}
			}
		}
		else
		{
			Debug.Assert(Monitor.IsEntered(actualParent.DataTreeManager.Sync));

			ActualDataNode = actualParent.Children.AddInternalUnsynced(Name.AsSpan(), mProperties);

			foreach (ExpectedDataNode expectedChildNode in Children)
			{
				expectedChildNode.InitializeActualDataNode(ActualDataNode);
			}

			foreach (IExpectedUntypedDataValue expectedValue in Values)
			{
				expectedValue.InitializeActualDataValue(ActualDataNode);
			}
		}
	}

	#endregion

	#region BindActualDataNode()

	/// <summary>
	/// Binds an existing actual data tree to the expected data tree and initializes the
	/// <see cref="ActualDataNode"/> properties and <see cref="ExpectedDataValue{T}.ActualDataValue"/>
	/// properties to link the actual data node/value to their expected equivalent.
	/// </summary>
	/// <param name="root">The root not of the actual data tree.</param>
	public void BindActualDataNode(DataNode root)
	{
		if (Parent != null) throw new InvalidOperationException("Must be called on root nodes only!");
		BindActualDataNodeInternal(root);
	}

	/// <summary>
	/// Binds the specified actual node to the expected node setting the <see cref="ActualDataNode"/> property of the current node.
	/// </summary>
	/// <param name="actualDataNode">
	/// The actual node corresponding to the current expected node.
	/// </param>
	private void BindActualDataNodeInternal(DataNode actualDataNode)
	{
		// the name and the path of the actual data node should match the name of the expected data node
		Assert.Equal(Name, actualDataNode.Name);
		Assert.Equal(Path, actualDataNode.Path);

		// set the ActualDataNode properties to allow walking backward in the following
		ActualDataNode = actualDataNode;

		// bind actual child data nodes to the corresponding expected data nodes
		ExpectedDataNode[] expectedChildren = [.. mChildren];
		Assert.Equal(expectedChildren.Length, actualDataNode.Children.InternalBuffer.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, DataNode actualChildDataNode) in expectedChildren.Zip(
			         actualDataNode.Children.InternalBuffer,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// the name of the actual data node should match the name of the expected data node
			Assert.Equal(expectedChildDataNode.Name, actualChildDataNode.Name);

			// proceed binding recursively
			expectedChildDataNode.BindActualDataNodeInternal(actualChildDataNode);
		}

		// bind actual data values to the corresponding expected data values
		IExpectedUntypedDataValue[] expectedValues = [.. mValues];
		Assert.Equal(expectedValues.Length, actualDataNode.Values.InternalBuffer.Count);
		foreach ((IExpectedUntypedDataValue expectedDataValue, IUntypedDataValueInternal actualDataValue) in expectedValues.Zip(
			         actualDataNode.Values.InternalBuffer,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			// the name of the actual data value should match the name of the expected data value
			Assert.Equal(expectedDataValue.Name, actualDataValue.Name);

			// bind the actual data value to its expected pendent
			expectedDataValue.ActualDataValue = actualDataValue;
		}
	}

	#endregion

	#region GetNode()

	/// <summary>
	/// Gets the expected node at the specified path.
	/// </summary>
	/// <param name="path">Path to get the node at.</param>
	/// <returns>
	/// The expected node at the specified path;<br/>
	/// <c>null</c> if the node does not exist.
	/// </returns>
	public ExpectedDataNode GetNode(string path)
	{
		List<string> tokens = TokenizePath(path);
		return GetNode(tokens, 0);
	}

	/// <summary>
	/// Gets the expected node at the specified path.
	/// </summary>
	/// <param name="tokens">Tokens of the path to get the node at (each token is a node name).</param>
	/// <param name="index">Index of the token in <paramref name="tokens"/> to process.</param>
	/// <returns>
	/// The expected node at the specified path;<br/>
	/// <c>null</c> if the node does not exist.
	/// </returns>
	private ExpectedDataNode GetNode(IReadOnlyList<string> tokens, int index)
	{
		if (index == tokens.Count) return this;
		string name = tokens[index];
		ExpectedDataNode child = Children.FirstOrDefault(x => x.Name == name);
		return child?.GetNode(tokens, index + 1);
	}

	#endregion

	#region GetValue()

	/// <summary>
	/// Gets the expected value at the specified path.
	/// </summary>
	/// <param name="path">Path to get the value at.</param>
	/// <returns>
	/// The expected value at the specified path;<br/>
	/// <c>null</c> if the value does not exist.
	/// </returns>
	public IExpectedUntypedDataValue GetValue(string path)
	{
		List<string> tokens = TokenizePath(path);
		return GetValue(tokens, 0);
	}

	/// <summary>
	/// Gets the expected value at the specified path.
	/// </summary>
	/// <param name="tokens">Tokens of the path to get the value at (each token is a node/value name).</param>
	/// <param name="index">Index of the token in <paramref name="tokens"/> to process.</param>
	/// <returns>
	/// The expected value at the specified path;<br/>
	/// <c>null</c> if the value does not exist.
	/// </returns>
	private IExpectedUntypedDataValue GetValue(IReadOnlyList<string> tokens, int index)
	{
		if (index == tokens.Count) return null;
		string name = tokens[index];
		if (index < tokens.Count - 1)
		{
			ExpectedDataNode child = Children.FirstOrDefault(x => x.Name == name);
			return child?.GetValue(tokens, index + 1);
		}

		IExpectedUntypedDataValue value = Values.FirstOrDefault(x => x.Name == name);
		return value;
	}

	#endregion

	#region RunTest()

	/// <summary>
	/// Builds an actual data tree as defined by the current expected data tree,
	/// executes the specified action to modify both data trees and
	/// checks whether both data trees match each other after that and that the events
	/// are raised as expected. The order of events is checked as well.
	/// </summary>
	/// <param name="testMethod">Test method modifying the actual and the expected data tree.</param>
	public void RunTest(Action<ExpectedDataNode> testMethod)
	{
		if (Parent != null)
			throw new InvalidOperationException("Must be called on root nodes only!");

		AsyncContext.Run(
			async () =>
			{
				// build actual data tree as defined by the current expected data tree and register all events
				// (the ExpectedDataNode.ActualDataNode properties and ExpectedDataValue<T>.ActualDataValue properties
				// contain the corresponding actual DataNode respectively the actual DataValue<T> objects)
				InitializeActualDataNode();

				// invoke specified test method that fiddles with properties/methods on the actual data tree
				// and modifies the expected tree to the expected result
				testMethod(this);

				// wait for all raised events to finish
				// (both the current thread and the data tree manager host thread must have executed all queued event handlers)
				using SemaphoreSlim finishedSemaphore1 = new(0);
				using SemaphoreSlim finishedSemaphore2 = new(0);
				SynchronizationContext.Current!.Post(state => ReleaseSemaphore((SemaphoreSlim)state), finishedSemaphore1);
				DataTreeManagerHost.Default.SynchronizationContext.Post(state => ReleaseSemaphore((SemaphoreSlim)state), finishedSemaphore2);
				Task<bool> finishedTask1 = finishedSemaphore1.WaitAsync(MaximumTimeToWaitForEvents);
				Task<bool> finishedTask2 = finishedSemaphore2.WaitAsync(MaximumTimeToWaitForEvents);
				await Task.WhenAll(finishedTask1, finishedTask2);
				Assert.True(finishedTask1.Result, "The executing thread did not finish executing event handlers in time.");
				Assert.True(finishedTask2.Result, "The data tree manager host thread did not finish executing event handlers in time.");

				// recursively check whether the actual data tree matches the expected data tree
				AssertActualTreeEqualsExpectedTree();

				// recursively check whether events were raised properly
				AssertExpectedEventsWereRaised();
			});
	}

	#endregion

	#region AssertActualTreeEqualsExpectedTree()

	/// <summary>
	/// Checks whether the bound actual data tree matches the current expected data tree.
	/// </summary>
	public void AssertActualTreeEqualsExpectedTree()
	{
		if (Parent != null) throw new InvalidOperationException("Must be called on root nodes only!");

		lock (ActualDataNode.DataTreeManager.Sync)
		{
			AssertActualTreeEqualsExpectedTreeInternal();
		}
	}

	/// <summary>
	/// Checks whether the bound actual data tree matches the current expected data tree (for internal use only).
	/// </summary>
	private void AssertActualTreeEqualsExpectedTreeInternal()
	{
		Debug.Assert(Monitor.IsEntered(ActualDataNode.DataTreeManager.Sync));

		// check node itself
		Assert.Equal(Name, ActualDataNode.Name);
		Assert.Equal(Path, ActualDataNode.Path);
		Assert.Equal(Properties, ActualDataNode.Properties);
		Assert.Equal(IsPersistent, ActualDataNode.IsPersistent);
		Assert.Equal(IsPersistent, ActualDataNode.IsPersistentUnsynced);
		Assert.Equal(IsDummy, ActualDataNode.IsDummyUnsynced);
		Assert.Equal(IsDummy, ActualDataNode.ViewerIsDummy);
		Assert.Equal(RootNode.ActualDataNode.DataTreeManager, ActualDataNode.DataTreeManager);
		Assert.Same(Parent?.ActualDataNode, ActualDataNode.Parent);

		// check child nodes
		ExpectedDataNode[] expectedChildren = mChildren.Where(x => !x.IsDummy).ToArray();
		Assert.Equal(expectedChildren.Length, ActualDataNode.Children.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, DataNode actualChildDataNode) in expectedChildren.Zip(
			         ActualDataNode.Children,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Same(expectedChildDataNode.ActualDataNode, actualChildDataNode);
			expectedChildDataNode.AssertActualTreeEqualsExpectedTreeInternal();
		}

		// check values
		IExpectedUntypedDataValue[] expectedValues = mValues.Where(x => !x.IsDummy).ToArray();
		Assert.Equal(expectedValues.Length, ActualDataNode.Children.Count);
		foreach ((IExpectedUntypedDataValue expectedDataValue, IUntypedDataValueInternal actualDataValue) in expectedValues.Zip(
			         ActualDataNode.Values,
			         (expected, actual) => (Expected: expected, Actual: (IUntypedDataValueInternal)actual)))
		{
			Assert.Same(expectedDataValue.ActualDataValue, actualDataValue);
			expectedDataValue.AssertConsistency();
		}

		// check viewer data node wrapping the data node
		AssertActualTreeEqualsExpectedTreeInternal(ActualDataNode.ViewerWrapper);
	}

	/// <summary>
	/// Checks whether the bound actual viewer data tree matches the current expected data tree (for internal use only).
	/// </summary>
	/// <param name="actualViewerDataNode">Actual viewer data node to compare the current expected viewer data node with.</param>
	private void AssertActualTreeEqualsExpectedTreeInternal(ViewerDataNode actualViewerDataNode)
	{
		// check whether the current viewer node is the same as the wrapped node
		// (just to be sure that enumeration was right...)
		Assert.NotNull(actualViewerDataNode);
		Assert.Same(ActualDataNode.ViewerWrapper, actualViewerDataNode);

		// unwrapping viewer data node should return the current data node
		Assert.Same(ActualDataNode, actualViewerDataNode.WrappedNode);

		// check node itself
		Assert.Equal(Name, actualViewerDataNode.Name);
		Assert.Equal(Path, actualViewerDataNode.Path);
		Assert.Equal(Properties, actualViewerDataNode.Properties);
		Assert.Equal(IsPersistent, actualViewerDataNode.IsPersistent);
		Assert.Equal(IsDummy, actualViewerDataNode.IsDummy);
		Assert.Equal(RootNode.ActualDataNode.DataTreeManager, actualViewerDataNode.DataTreeManager);
		Assert.Same(Parent?.ActualDataNode.ViewerWrapper, actualViewerDataNode.Parent);

		// check child nodes
		ExpectedDataNode[] expectedViewerChildren = [.. mChildren];
		Assert.Equal(expectedViewerChildren.Length, actualViewerDataNode.Children.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, ViewerDataNode actualViewerChildDataNode) in expectedViewerChildren.Zip(
			         actualViewerDataNode.Children,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Same(expectedChildDataNode.ActualDataNode.ViewerWrapper, actualViewerChildDataNode);
			expectedChildDataNode.AssertActualTreeEqualsExpectedTreeInternal();
		}

		// check values
		IExpectedUntypedDataValue[] expectedViewerValues = [.. mValues];
		Assert.Equal(expectedViewerValues.Length, actualViewerDataNode.Values.Count);
		foreach ((IExpectedUntypedDataValue expectedDataValue, IUntypedViewerDataValue actualViewerDataValue) in expectedViewerValues.Zip(
			         actualViewerDataNode.Values,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Same(expectedDataValue.ActualDataValue.ViewerWrapper, actualViewerDataValue);
			expectedDataValue.CheckViewerConsistency(actualViewerDataValue);
		}
	}

	#endregion

	#region AssertExpectedEventsWereRaised()

	/// <summary>
	/// Recursively check whether data node events and data value events were raised as expected.
	/// </summary>
	private void AssertExpectedEventsWereRaised()
	{
		// check whether 'Changed' events and 'ChangedAsync' events were raised properly
		AssertExpectedChangedEventsWereRaised(mExpectedChangedEvents, ReceivedChangedEvents, SynchronizationContext.Current);
		AssertExpectedChangedEventsWereRaised(mExpectedChangedAsyncEvents, ReceivedChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether 'ViewerChanged' events and 'ViewerChangedAsync' events were raised properly
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedEvents, ReceivedViewerChangedEvents, SynchronizationContext.Current);
		AssertExpectedViewerChangedEventsWereRaised(mExpectedViewerChangedAsyncEvents, ReceivedViewerChangedAsyncEvents, DataTreeManagerHost.Default.SynchronizationContext);

		// check whether data value events were raised properly
		foreach (IExpectedUntypedDataValue expectedDataValue in Values)
		{
			expectedDataValue.AssertExpectedEventsWereRaised();
		}

		// dive downward the data tree and check child nodes and their values
		foreach (ExpectedDataNode expectedChildNode in Children)
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
			Assert.Same(expected.EventArgs.Node, actual.EventArgs.Node);
			Assert.Equal(expected.EventArgs.Snapshot.Name, actual.EventArgs.Snapshot.Name);
			Assert.Equal(expected.EventArgs.Snapshot.Path, actual.EventArgs.Snapshot.Path);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.ChangeFlags, actual.EventArgs.ChangeFlags);
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
			Assert.Same(expected.EventArgs.Node, actual.EventArgs.Node);
			Assert.Equal(expected.EventArgs.Snapshot.Name, actual.EventArgs.Snapshot.Name);
			Assert.Equal(expected.EventArgs.Snapshot.Path, actual.EventArgs.Snapshot.Path);
			Assert.Equal(expected.EventArgs.Snapshot.Properties, actual.EventArgs.Snapshot.Properties);
			Assert.Equal(expected.EventArgs.Snapshot.IsPersistent, actual.EventArgs.Snapshot.IsPersistent);
			Assert.Equal(expected.EventArgs.Snapshot.IsDummy, actual.EventArgs.Snapshot.IsDummy);
			Assert.Equal(expected.EventArgs.ChangeFlags, actual.EventArgs.ChangeFlags);
		}
	}

	#endregion

	#region BindActualDataTree(DataNode root)

	/// <summary>
	/// Binds the specified actual data node to the current expected node.
	/// </summary>
	/// <param name="root">Actual data node to compare the current expected data node with.</param>
	public void BindActualDataTree(DataNode root)
	{
		if (Parent != null) throw new InvalidOperationException("Must be called on root nodes only!");

		lock (root.DataTreeManager.Sync)
		{
			BindActualDataTreeInternal(root);
		}
	}

	/// <summary>
	/// Binds the specified actual data node to the current expected node (for internal use only).
	/// </summary>
	/// <param name="actualDataNode">Actual data node to bind to the expected data node.</param>
	private void BindActualDataTreeInternal(DataNode actualDataNode)
	{
		Debug.Assert(Monitor.IsEntered(actualDataNode.DataTreeManager.Sync));

		// set the actual data node to allow walking up the tree later on
		ActualDataNode = actualDataNode;

		// bind child nodes
		ExpectedDataNode[] expectedChildren = mChildren.Where(x => !x.IsDummy).ToArray();
		Assert.Equal(expectedChildren.Length, actualDataNode.Children.Count);
		foreach ((ExpectedDataNode expectedChildDataNode, DataNode actualChildDataNode) in expectedChildren.Zip(
			         actualDataNode.Children,
			         (expected, actual) => (Expected: expected, Actual: actual)))
		{
			Assert.Equal(expectedChildDataNode.Name, actualChildDataNode.Name);
			expectedChildDataNode.BindActualDataTreeInternal(actualChildDataNode);
		}

		// check values
		IExpectedUntypedDataValue[] expectedValues = mValues.Where(x => !x.IsDummy).ToArray();
		Assert.Equal(expectedValues.Length, actualDataNode.Children.Count);
		foreach ((IExpectedUntypedDataValue expectedDataValue, IUntypedDataValueInternal actualDataValue) in expectedValues.Zip(
			         actualDataNode.Values,
			         (expected, actual) => (Expected: expected, Actual: (IUntypedDataValueInternal)actual)))
		{
			Assert.Equal(expectedDataValue.Name, actualDataValue.Name);
			Assert.Equal(expectedDataValue.Type, actualDataValue.Type);
			expectedDataValue.BindAndValidateActualDataTreeInternal(actualDataValue);
		}
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
	/// Gets the <see cref="DataNode.Changed"/> events that are expected to be fired.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedEvents => [.. mExpectedChangedEvents];

	/// <summary>
	/// Gets the <see cref="DataNode.ChangedAsync"/> events that are expected to be fired.
	/// </summary>
	public ExpectedChangedEventItem[] ExpectedChangedAsync => [.. mExpectedChangedAsyncEvents];

	/// <summary>
	/// Gets the <see cref="ViewerDataNode.Changed"/> events,
	/// respectively the <see cref="DataNode.ViewerChanged"/> events,
	/// that are expected to be fired.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedEvents => [.. mExpectedViewerChangedEvents];

	/// <summary>
	/// Gets the <see cref="ViewerDataNode.ChangedAsync"/> events,
	/// respectively the <see cref="DataNode.ViewerChangedAsync"/> events,
	/// that are expected to be fired.
	/// </summary>
	public ExpectedViewerChangedEventItem[] ExpectedViewerChangedAsync => [.. mExpectedViewerChangedAsyncEvents];

	/// <summary>
	/// Gets the received <see cref="DataNode.Changed"/> events.
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
	/// Gets the received <see cref="DataNode.ChangedAsync"/> events.
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
	/// Gets the received <see cref="ViewerDataNode.Changed"/> events,
	/// respectively the <see cref="DataNode.ViewerChanged"/> events.
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
	/// Gets the received <see cref="ViewerDataNode.ChangedAsync"/> events,
	/// respectively the <see cref="DataNode.ViewerChangedAsync"/> events.
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
	/// Clears the lists of received events.
	/// </summary>
	/// <param name="recursive">
	/// <c>true</c> to clear expected events recursively;<br/>
	/// otherwise <c>false</c>.
	/// </param>
	public void ResetExpectedEvents(bool recursive = true)
	{
		mExpectedChangedEvents.Clear();
		mExpectedChangedAsyncEvents.Clear();
		mExpectedViewerChangedEvents.Clear();
		mExpectedViewerChangedAsyncEvents.Clear();

		if (!recursive)
			return;

		foreach (ExpectedDataNode child in Children)
		{
			child.ResetExpectedEvents();
		}

		foreach (IExpectedUntypedDataValue value in Values)
		{
			value.ResetExpectedEvents();
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

		foreach (ExpectedDataNode child in Children)
		{
			child.ResetReceivedEvents();
		}

		foreach (IExpectedUntypedDataValue value in Values)
		{
			value.ResetReceivedEvents();
		}
	}

	private void ChangedHandler(object sender, DataNodeChangedEventArgs e)
	{
		lock (mReceivedChangedEvents)
		{
			mReceivedChangedEvents.Add(
				new ReceivedChangedEventItem(
					nameof(DataNode.Changed),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ChangedAsyncHandler(object sender, DataNodeChangedEventArgs e)
	{
		lock (mReceivedChangedAsyncEvents)
		{
			mReceivedChangedAsyncEvents.Add(
				new ReceivedChangedEventItem(
					nameof(DataNode.ChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ViewerChangedHandler(object sender, ViewerDataNodeChangedEventArgs e)
	{
		lock (mReceivedViewerChangedEvents)
		{
			mReceivedViewerChangedEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(DataNode.ViewerChanged),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	private void ViewerChangedAsyncHandler(object sender, ViewerDataNodeChangedEventArgs e)
	{
		lock (mReceivedViewerChangedAsyncEvents)
		{
			mReceivedViewerChangedAsyncEvents.Add(
				new ReceivedViewerChangedEventItem(
					nameof(DataNode.ViewerChangedAsync),
					sender,
					e,
					SynchronizationContext.Current));
		}
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Releases the specified semaphore catching a possibly thrown <see cref="ObjectDisposedException"/>
	/// (this is necessary if an asynchronous task does not finish in time, so the semaphore has already been
	/// disposed by the initiating test).
	/// </summary>
	/// <param name="semaphore">Semaphore to release.</param>
	private static void ReleaseSemaphore(SemaphoreSlim semaphore)
	{
		try
		{
			semaphore.Release();
		}
		catch (ObjectDisposedException)
		{
			// swallow...
		}
	}

	/// <summary>
	/// Sets the parent node for all specified items.
	/// </summary>
	/// <param name="items">Items to set the parent for.</param>
	private void SetParentToThis(IEnumerable<IExpectedItem> items)
	{
		if (items == null)
			return;

		foreach (IExpectedItem item in items)
		{
			item.Parent = this;
		}
	}

	/// <summary>
	/// Resets the parent node for all specified items.
	/// </summary>
	/// <param name="items">Items to reset the parent for.</param>
	private static void ResetParent(IEnumerable<IExpectedItem> items)
	{
		if (items == null)
			return;

		foreach (IExpectedItem item in items)
		{
			item.Parent = null;
		}
	}

	/// <summary>
	/// Splits the specified path into a list of node/value names (without any validation).
	/// </summary>
	/// <param name="path">Path to tokenize.</param>
	/// <returns>List of names extracted from the path.</returns>
	private static List<string> TokenizePath(string path)
	{
		bool escape = false;
		var token = new StringBuilder();
		var tokens = new List<string>();
		foreach (char c in path)
		{
			if (escape)
			{
				token.Append(c);
				continue;
			}

			if (c == '/')
			{
				if (token.Length > 0) tokens.Add(token.ToString());
				token.Clear();
				continue;
			}

			if (c == '\\')
			{
				escape = true;
				continue;
			}

			token.Append(c);
		}

		if (token.Length > 0) tokens.Add(token.ToString());
		return tokens;
	}

	/// <summary>
	/// Makes the current node and all nodes up to the root node persistent.
	/// </summary>
	internal void MakePersistent()
	{
		// make all nodes up to the root persistent
		Parent?.MakePersistent();

		// make current node persistent
		DataNodePropertiesInternal newProperties = mProperties | DataNodePropertiesInternal.Persistent;
		if (newProperties == mProperties) return;
		DataNodePropertiesInternal oldProperties = mProperties;
		mProperties = newProperties;

		// abort if there is no actual data node, yet
		if (ActualDataNode == null)
			return;

		// add expected regular events
		// ------------------------------------------------------------------------------------

		DataNodeChangedFlags changedFlags = DataNodeChangedFlags.Properties | (DataNodeChangedFlags)((~mProperties & oldProperties) | (mProperties & ~oldProperties));

		// expect 'Changed' event to be raised
		mExpectedChangedEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataNode.Changed),
				ActualDataNode,
				new ExpectedDataNodeChangedEventArgs(
					ActualDataNode,
					new ExpectedDataNodeSnapshot(Name, Path, mProperties),
					changedFlags)));

		// expect 'ChangedAsync' event to be raised
		mExpectedChangedAsyncEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataNode.ChangedAsync),
				ActualDataNode,
				new ExpectedDataNodeChangedEventArgs(
					ActualDataNode,
					new ExpectedDataNodeSnapshot(Name, Path, mProperties),
					changedFlags)));

		// add expected viewer events
		// ------------------------------------------------------------------------------------
		ViewerDataNodeChangedFlags viewerChangedFlags = ViewerDataNodeChangedFlags.Properties | (ViewerDataNodeChangedFlags)((~mProperties & oldProperties) | (mProperties & ~oldProperties));

		// expect 'ViewerChanged' event to be raised
		mExpectedViewerChangedEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataNode.ViewerChanged),
				ActualDataNode.ViewerWrapper,
				new ExpectedViewerDataNodeChangedEventArgs(
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
					viewerChangedFlags)));

		// expect 'ViewerChangedAsync' event to be raised
		mExpectedViewerChangedAsyncEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataNode.ViewerChangedAsync),
				ActualDataNode.ViewerWrapper,
				new ExpectedViewerDataNodeChangedEventArgs(
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
					viewerChangedFlags)));
	}

	/// <summary>
	/// Raises events informing about a change to the <see cref="Path"/> property.
	/// </summary>
	internal void NotifyPathChanged()
	{
		// add expected regular events
		// ------------------------------------------------------------------------------------

		const DataNodeChangedFlags changedFlags = DataNodeChangedFlags.Path;

		// expect 'Changed' event to be raised
		mExpectedChangedEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataNode.Changed),
				ActualDataNode,
				new ExpectedDataNodeChangedEventArgs(
					ActualDataNode,
					new ExpectedDataNodeSnapshot(Name, Path, mProperties),
					changedFlags)));

		// expect 'ChangedAsync' event to be raised
		mExpectedChangedAsyncEvents.Add(
			new ExpectedChangedEventItem(
				nameof(DataNode.ChangedAsync),
				ActualDataNode,
				new ExpectedDataNodeChangedEventArgs(
					ActualDataNode,
					new ExpectedDataNodeSnapshot(Name, Path, mProperties),
					changedFlags)));

		// add expected viewer events
		// ------------------------------------------------------------------------------------
		const ViewerDataNodeChangedFlags viewerChangedFlags = ViewerDataNodeChangedFlags.Path;

		// expect 'ViewerChanged' event to be raised
		mExpectedViewerChangedEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataNode.ViewerChanged),
				ActualDataNode.ViewerWrapper,
				new ExpectedViewerDataNodeChangedEventArgs(
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
					viewerChangedFlags)));

		// expect 'ViewerChangedAsync' event to be raised
		mExpectedViewerChangedAsyncEvents.Add(
			new ExpectedViewerChangedEventItem(
				nameof(DataNode.ViewerChangedAsync),
				ActualDataNode.ViewerWrapper,
				new ExpectedViewerDataNodeChangedEventArgs(
					ActualDataNode.ViewerWrapper,
					new ExpectedViewerDataNodeSnapshot(Name, Path, mProperties),
					viewerChangedFlags)));

		// let child nodes add their events
		// ------------------------------------------------------------------------------------
		foreach (ExpectedDataNode dataNode in Children)
		{
			dataNode.NotifyPathChanged();
		}
	}

	#endregion
}
