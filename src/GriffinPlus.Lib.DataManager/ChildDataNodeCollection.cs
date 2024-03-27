///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;
using GriffinPlus.Lib.Serialization;
using GriffinPlus.Lib.Threading;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A collection containing child nodes of a data node.
/// </summary>
[InternalObjectSerializer(1)]
[DebuggerDisplay("Count: {" + nameof(Count) + "}")]
public sealed partial class ChildDataNodeCollection :
	IEnumerable<DataNode>,
	IInternalObjectSerializer
{
	private readonly DataNode       mNode;   // node the collection belongs to
	private readonly List<DataNode> mBuffer; // list storing the nodes

	#region Changed / ChangedAsync

	internal const string ChangedEventName = "Changed";

	/// <summary>
	/// Occurs when a node is added to or removed from the collection.<br/>
	/// The first invocation of the event handler notifies about the initial set of nodes in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataNodeCollectionChangedEventArgs> Changed
	{
		add
		{
			lock (mNode.DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<DataNodeCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					SynchronizationContext.Current ?? mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataNodeCollectionChangedEventArgs(mNode, RegularNodesOnlyUnsynced));
			}
		}

		remove => EventManager<DataNodeCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a node is added to or removed from the collection.<br/>
	/// The first invocation of the event handler notifies about the initial set of nodes in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataNodeCollectionChangedEventArgs> ChangedAsync
	{
		add
		{
			lock (mNode.DataTreeManager.Sync)
			{
				EventManager<DataNodeCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataNodeCollectionChangedEventArgs(mNode, RegularNodesOnlyUnsynced));
			}
		}

		remove => EventManager<DataNodeCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ChildDataNodeCollection"/> class.
	/// </summary>
	/// <param name="node">Data node the collection resides in.</param>
	internal ChildDataNodeCollection(DataNode node)
	{
		mBuffer = [];
		mNode = node;
	}

	#endregion

	#region Serialization using Griffin+ Serializer

	/// <summary>
	/// Initializes a new instance of the <see cref="ChildDataNodeCollection"/> class from a serializer archive
	/// (for serialization purposes only).
	/// </summary>
	/// <param name="archive">Serializer archive containing the serialized collection.</param>
	/// <exception cref="VersionNotSupportedException">Archive was created using a serializer of a greater version than supported.</exception>
	internal ChildDataNodeCollection(DeserializationArchive archive)
	{
		if (archive.Version == 1)
		{
			// get the node the collection belongs to from the archive context
			if (archive.Context is not DataDeserializationContext context)
				throw new SerializationException($"The serialization archive does not contain a {nameof(DataDeserializationContext)}.");

			mNode = context.ParentNode;

			// read number of child nodes
			int count = archive.ReadInt32();

			// read child nodes
			mBuffer = new List<DataNode>(count);
			for (int i = 0; i < count; i++)
			{
				var node = (DataNode)archive.ReadObject(context);
				mBuffer.Add(node);
			}
		}
		else
		{
			throw new VersionNotSupportedException(archive);
		}

		// ensure all members are initialized before leaving the constructor
		// (otherwise some other thread may find a partially initialized object)
		Thread.MemoryBarrier();
	}

	/// <summary>
	/// Serializes the current instance to a serializer archive.
	/// </summary>
	/// <param name="archive">Archive to serialize the current object to.</param>
	/// <exception cref="VersionNotSupportedException">Requested serializer version is greater than supported.</exception>
	void IInternalObjectSerializer.Serialize(SerializationArchive archive)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			if (archive.Version == 1)
			{
				// determine the number of nodes to serialize
				int count = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach (DataNode node in mBuffer)
#else
				foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
				{
					// only regular persistent nodes should be serialized
					if ((node.PropertiesUnsynced & DataNodePropertiesInternal.Persistent) != 0 &&
					    (node.PropertiesUnsynced & DataNodePropertiesInternal.Dummy) == 0)
					{
						count++;
					}
				}

				// write number of nodes
				archive.Write(count);

				// write the nodes
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
				// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
				foreach (DataNode node in mBuffer)
#else
				foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
				{
					// only regular persistent nodes should be serialized
					if ((node.PropertiesUnsynced & DataNodePropertiesInternal.Persistent) != 0 &&
					    (node.PropertiesUnsynced & DataNodePropertiesInternal.Dummy) == 0)
					{
						archive.Write(node);
					}
				}
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}
	}

	#endregion

	#region Count

	/// <summary>
	/// Gets the number of nodes in the collection.
	/// </summary>
	public int Count
	{
		get
		{
			lock (mNode.DataTreeManager.Sync)
			{
				return RegularNodesOnlyUnsynced.Count();
			}
		}
	}

	/// <summary>
	/// Gets the number of nodes in the collection
	/// (considers regular and dummy nodes, for internal use only, not synchronized).
	/// </summary>
	internal int CountUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mBuffer.Count;
		}
	}

	#endregion

	#region InternalBuffer

	/// <summary>
	/// Gets the internal list of child data nodes (considers regular and dummy nodes).
	/// </summary>
	internal IReadOnlyList<DataNode> InternalBuffer => mBuffer;

	#endregion

	#region RegularNodesOnlyUnsynced

	/// <summary>
	/// Gets all regular data nodes in the collection (for internal use only, not synchronized).
	/// </summary>
	private IEnumerable<DataNode> RegularNodesOnlyUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mBuffer.Where(x => !x.IsDummyUnsynced);
		}
	}

	#endregion

	#region Add(...)

	/// <summary>
	/// Adds a new child node with the specified name and the same properties as the node containing the current collection.
	/// </summary>
	/// <param name="name">Name of the child node to add.</param>
	/// <return>The added child node.</return>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="name"/> does not comply with the rules for node names.</exception>
	/// <exception cref="DataNodeExistsAlreadyException">Data node with the specified name exists already.</exception>
	public DataNode Add(string name)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			DataNodePropertiesInternal properties = mNode.PropertiesUnsynced & ~DataNodePropertiesInternal.AdministrativeProperties;
			return AddUnsynced(name.AsSpan(), properties);
		}
	}

	/// <summary>
	/// Adds a new child node with the specified name and properties.
	/// </summary>
	/// <param name="name">Name of the child node to add.</param>
	/// <param name="properties">Properties of the node to add.</param>
	/// <return>The added child node.</return>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for node names.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataNodeExistsAlreadyException">Data node with the specified name exists already.</exception>
	public DataNode Add(string name, DataNodeProperties properties)
	{
		if ((properties & ~DataNodeProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags ((0x{properties & ~DataNodeProperties.All:X})).", nameof(properties));

		lock (mNode.DataTreeManager.Sync)
		{
			return AddUnsynced(name.AsSpan(), (DataNodePropertiesInternal)properties);
		}
	}

	/// <summary>
	/// Adds a regular node with the specified name to the collection (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the node to add.</param>
	/// <param name="properties">
	/// Properties of the node to add
	/// (must not contain the <see cref="DataNodePropertiesInternal.Dummy"/> flag).
	/// </param>
	/// <returns>The added node.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for node names.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags or the <see cref="DataNodePropertiesInternal.Dummy"/> flag.
	/// </exception>
	/// <exception cref="DataNodeExistsAlreadyException">Data node with the specified name exists already.</exception>
	internal DataNode AddUnsynced(ReadOnlySpan<char> name, DataNodePropertiesInternal properties)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// ensure that the specified name is valid
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (!PathHelpers.IsValidName(name))
			throw new ArgumentException($"The specified name ({name.ToString()}) does not comply with the rules for node names.", nameof(name));

		// ensure that the properties of the node to add are valid
		if ((properties & ~DataNodePropertiesInternal.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags (0x{properties & ~DataNodePropertiesInternal.All:X}).", nameof(properties));

		// ensure that the dummy flag is not set
		if ((properties & DataNodePropertiesInternal.Dummy) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) must not contain the dummy flag (0x{DataNodePropertiesInternal.Dummy:X}).", nameof(properties));

		// check whether a node with the specified name is already in the collection
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (DataNode node in mBuffer)
#else
		foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			// proceed if the name does not match...
			if (!node.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal))
				continue;

			// node with the specified name exists

			// abort if the node is regular
			if (!node.IsDummyUnsynced)
				throw new DataNodeExistsAlreadyException($"A node with the specified name ({name.ToString()}) exists already.");

			// node is a dummy node
			// => regularize the existing dummy node and return it...
			node.PropertiesUnsynced = properties;
			return node;
		}

		// node with the specified name does not exist
		// => add new node and raise events...
		return AddInternalUnsynced(name, properties);
	}

	/// <summary>
	/// Adds a regular node with the specified name to the collection
	/// (does not perform any checks, for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the node to add.</param>
	/// <param name="properties">
	/// Properties of the node to add
	/// (must not contain the <see cref="DataNodePropertiesInternal.Dummy"/> flag).
	/// </param>
	/// <returns>The added node.</returns>
	internal DataNode AddInternalUnsynced(ReadOnlySpan<char> name, DataNodePropertiesInternal properties)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(PathHelpers.IsValidName(name));
		Debug.Assert((properties & ~DataNodePropertiesInternal.All) == 0, $"The specified properties (0x{properties:X}) contain unsupported flags (0x{properties & ~DataNodePropertiesInternal.All:X}).");
		Debug.Assert((properties & DataNodePropertiesInternal.Dummy) == 0, $"The specified properties (0x{properties:X}) must not contain the dummy flag (0x{DataNodePropertiesInternal.Dummy:X}).");
#if DEBUG
		string nameString = name.ToString();
		Debug.Assert(mBuffer.All(x => x.NameUnsynced != nameString), $"The collection must not contain a node with the specified name ({name.ToString()}).");
#endif

		// regularize current node, if necessary
		if (mNode.IsDummyUnsynced) mNode.RegularizeUnsynced();

		// add node to the collection
		var newNode = new DataNode(mNode, name, properties);
		mBuffer.Add(newNode);

		// the added node is always regular

		// raise 'Changed' event and 'ChangedAsync' event
		if (EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
		{
			EventManager<DataNodeCollectionChangedEventArgs>.FireEvent(
				this,
				ChangedEventName,
				this,
				new DataNodeCollectionChangedEventArgs(
					CollectionChangedAction.Added,
					mNode,
					newNode));
		}

		// raise 'ViewerChanged' event and 'ViewerChangedAsync' event
		if (EventManager<ViewerDataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataNodeCollectionChangedEventArgs>.FireEvent(
				this,
				ViewerChangedEventName,
				ViewerWrapper,
				new ViewerDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.Added,
					mNode.ViewerWrapper,
					newNode.ViewerWrapper));
		}

		return newNode;
	}

	#endregion

	#region Clear()

	/// <summary>
	/// Clears the collection.
	/// </summary>
	public void Clear()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			// unregister data value references below the current node
			// (removes dummy nodes/values associated with these references as well)
			IUntypedDataInternal[] references = mNode.DataTreeManager.UnregisterDataValueReferencesBelowNodeUnsynced(mNode, true);

			// remove nodes from the collection
			// (remove from last to first to avoid reorganizing the buffer unnecessarily)
			for (int i = mBuffer.Count - 1; i >= 0; i--)
			{
				DataNode node = mBuffer[i];

				// skip node, if the node is dummy
				if (node.IsDummyUnsynced) continue;

				// remove data node
				mBuffer.RemoveAt(i);

				// the removed node is always regular...

				// raise 'Changed' event and 'ChangedAsync' event
				if (EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
				{
					EventManager<DataNodeCollectionChangedEventArgs>.FireEvent(
						this,
						ChangedEventName,
						this,
						new DataNodeCollectionChangedEventArgs(
							CollectionChangedAction.Removed,
							mNode,
							node));
				}

				// raise 'ViewerChanged' event and 'ViewerChangedAsync' event
				if (EventManager<ViewerDataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
				{
					EventManager<ViewerDataNodeCollectionChangedEventArgs>.FireEvent(
						this,
						ViewerChangedEventName,
						ViewerWrapper,
						new ViewerDataNodeCollectionChangedEventArgs(
							CollectionChangedAction.Removed,
							mNode.ViewerWrapper,
							node.ViewerWrapper));
				}

				// the removed node becomes the root node of its subtree
				node.BecomeRootNodeUnsynced();
			}

			// register previously removed data value references
			// (creates dummy nodes/values and binds them accordingly to the references)
			mNode.DataTreeManager.RegisterDataValueReferencesUnsynced(references);
		}
	}

	#endregion

	#region Contains(...)

	/// <summary>
	/// Checks whether a node with the specified name is in the collection.
	/// </summary>
	/// <param name="name">Name of the node to check for.</param>
	/// <returns>
	/// <c>true</c> if the collection contains a node with the specified name;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	public bool Contains(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name, false) != null;
		}
	}

	#endregion

	#region GetByName(...)

	/// <summary>
	/// Gets the data node with the specified name.
	/// </summary>
	/// <param name="name">Name of the data node to get.</param>
	/// <returns>
	/// Requested data node;<br/>
	/// <c>null</c> if there is no node with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	public DataNode GetByName(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name, false);
		}
	}

	/// <summary>
	/// Gets the data node with the specified name (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the data node to get (must not be <c>null</c>).</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy nodes;<br/>
	/// <c>false</c> to consider regular nodes only.
	/// </param>
	/// <returns>
	/// Requested data node;<br/>
	/// <c>null</c> if there is no node with the specified name.
	/// </returns>
	internal DataNode GetByNameUnsynced(string name, bool considerDummies = true)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(name != null);

		if (considerDummies)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (DataNode node in mBuffer)
#else
			foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (node.NameUnsynced == name) return node;
			}
		}
		else
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (DataNode node in mBuffer)
#else
			foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (node.IsDummyUnsynced) continue;
				if (node.NameUnsynced == name) return node;
			}
		}

		return null;
	}

	/// <summary>
	/// Gets the data node with the specified name (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the data node to get (must not be <c>null</c>).</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy nodes;<br/>
	/// <c>false</c> to consider regular nodes only.
	/// </param>
	/// <returns>
	/// Requested data node;<br/>
	/// <c>null</c> if there is no node with the specified name.
	/// </returns>
	internal DataNode GetByNameUnsynced(ReadOnlySpan<char> name, bool considerDummies = true)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(name != null);

		if (considerDummies)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (DataNode node in mBuffer)
#else
			foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (node.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal)) return node;
			}
		}
		else
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (DataNode node in mBuffer)
#else
			foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (node.IsDummyUnsynced) continue;
				if (node.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal)) return node;
			}
		}

		return null;
	}

	#endregion

	#region GetEnumerator()

	/// <summary>
	/// Gets an enumerator that can be used to iterate through the data node collection.
	/// </summary>
	/// <returns>An enumerator.</returns>
	/// <remarks>
	/// This method returns a thread-safe enumerator for the collection. The enumerator locks the entire tree when
	/// constructed and releases the lock when it gets disposed. To ensure the lock is released properly, call its
	/// <see cref="IDisposable.Dispose"/> method. Using a foreach statement calls the <see cref="IDisposable.Dispose"/>
	/// method automatically.
	/// </remarks>
	public IEnumerator<DataNode> GetEnumerator()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return new MonitorSynchronizedEnumerator<DataNode>(
				RegularNodesOnlyUnsynced.GetEnumerator(),
				mNode.DataTreeManager.Sync);
		}
	}

	/// <inheritdoc cref="GetEnumerator"/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion

	#region GetNewNodeName(...)

	/// <summary>
	/// Gets a name for a node that does not exist, yet.
	/// </summary>
	/// <param name="baseName">
	/// Base name of the child node to add (is suffixed using a hash followed by an incrementing number, if necessary).
	/// </param>
	/// <returns>
	/// The base name, if no node with the specified name is in the collection;<br/>
	/// A name consisting of the base name and a hash followed by an increasing number (e.g. "MyNode #2"),
	/// if a node with the specified base name exists already.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="baseName"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="baseName"/> does not comply with the rules for node names.</exception>
	public string GetNewNodeName(string baseName)
	{
		if (baseName == null) throw new ArgumentNullException(nameof(baseName));

		// check whether the specified name is valid
		if (!PathHelpers.IsValidName(baseName.AsSpan()))
			throw new ArgumentException($"The specified name ({baseName}) does not comply with the rules for node names.", nameof(baseName));

		lock (mNode.DataTreeManager.Sync)
		{
			// find a node name that does not exist, yet - or is dummy...
			// => use the base name and append a hash plus an increasing number, if necessary
			string name = baseName;
			for (int i = 2;; i++)
			{
				DataNode node = GetByNameUnsynced(name, true);
				if (node == null || node.IsDummyUnsynced) return name;
				name = $"{baseName} #{i}";
			}
		}
	}

	#endregion

	#region Remove(...) / RemoveAll(...)

	/// <summary>
	/// Removes the specified node.
	/// </summary>
	/// <param name="node">Node to remove.</param>
	/// <return>
	/// <c>true</c> if the specified node was removed;<br/>
	/// <c>false</c> if specified node does not exist in the collection.
	/// </return>
	/// <exception cref="ArgumentNullException"><paramref name="node"/> is <c>null</c>.</exception>
	public bool Remove(DataNode node)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));

		lock (mNode.DataTreeManager.Sync)
		{
			return RemoveUnsynced(node, false);
		}
	}

	/// <summary>
	/// Removes the node with the specified name.
	/// </summary>
	/// <param name="name">Name of the node to remove.</param>
	/// <return>
	/// <c>true</c> if the node with the specified name was removed;<br/>
	/// <c>false</c> if the node with the specified name does not exist.
	/// </return>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	public bool Remove(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			int index = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (DataNode node in mBuffer)
#else
			foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				// consider regular nodes only
				if (node.IsDummyUnsynced) continue;
				if (node.NameUnsynced == name) return RemoveByIndexInternalUnsynced(index);
				index++;
			}

			return false;
		}
	}

	/// <summary>
	/// Removes all nodes that match the specified predicate.
	/// </summary>
	/// <param name="predicate">A predicate nodes to remove must match.</param>
	/// <returns>Number of removed nodes.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
	public int RemoveAll(Predicate<DataNode> predicate)
	{
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		lock (mNode.DataTreeManager.Sync)
		{
			int count = 0;
			for (int i = mBuffer.Count - 1; i >= 0; i--)
			{
				DataNode node = mBuffer[i];
				if (node.IsDummyUnsynced || !predicate(node)) continue;
				RemoveByIndexInternalUnsynced(i);
				count++;
			}

			return count;
		}
	}

	/// <summary>
	/// Removes the specified node (for internal use, not synchronized).<br/>
	/// This method is able to remove dummy nodes.
	/// </summary>
	/// <param name="node">Node to remove.</param>
	/// <param name="allowDummy">
	/// <c>true</c> to allow removing the node if it is regular or dummy;<br/>
	/// <c>false</c> to allow removing the node if it is regular only.
	/// </param>
	/// <returns>
	/// <c>true</c> if the specified node was removed;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	internal bool RemoveUnsynced(DataNode node, bool allowDummy)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the node to remove is null
		if (node == null)
			return false;

		// abort if dummy nodes are not allowed and the node is dummy
		if (!allowDummy && node.IsDummyUnsynced)
			return false;

		int index = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (DataNode other in mBuffer)
#else
		foreach (DataNode other in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			if (ReferenceEquals(other, node))
				return RemoveByIndexInternalUnsynced(index);

			index++;
		}

		return false;
	}

	/// <summary>
	/// Removes the data node at the specified index from the collection and raises the appropriate events
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="index">Index of the node to remove.</param>
	/// <returns>
	/// <c>true</c> if the node was successfully removed;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	private bool RemoveByIndexInternalUnsynced(int index)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		DataNode node = mBuffer[index];

		// unregister data value references below the node to remove
		// (removes dummy nodes/values associated with these references as well)
		IUntypedDataInternal[] references = mNode.DataTreeManager.UnregisterDataValueReferencesBelowNodeUnsynced(node, true);

		// remove node from the collection
		mBuffer.RemoveAt(index);

		// raise 'Changed' event and 'ChangedAsync' event, if the removed node was regular
		if (!node.IsDummyUnsynced)
		{
			if (EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			{
				EventManager<DataNodeCollectionChangedEventArgs>.FireEvent(
					this,
					ChangedEventName,
					this,
					new DataNodeCollectionChangedEventArgs(
						CollectionChangedAction.Removed,
						mNode,
						node));
			}
		}

		// always raise 'ViewerChanged' event and 'ViewerChangedAsync' event
		if (EventManager<ViewerDataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataNodeCollectionChangedEventArgs>.FireEvent(
				this,
				ViewerChangedEventName,
				ViewerWrapper,
				new ViewerDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.Removed,
					mNode.ViewerWrapper,
					node.ViewerWrapper));
		}

		// the removed node becomes the root node of its subtree
		node.BecomeRootNodeUnsynced();

		// register previously removed data value references
		// (creates dummy nodes/values and binds them accordingly to the references)
		mNode.DataTreeManager.RegisterDataValueReferencesUnsynced(references);

		return true;
	}

	#endregion

	#region RequestItems() / RequestItemsAsync()

	/// <summary>
	/// Gets the data nodes in the collection asynchronously via the specified callback.<br/>
	/// The callback will be scheduled using the <see cref="SynchronizationContext"/> of the executing thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, the invocation will be scheduled on the data tree manager thread.<br/>
	/// This method is designed to work in conjunction with the <see cref="Changed"/> event.
	/// </summary>
	/// <param name="callback">Callback that will receive the data nodes that are currently in the collection.</param>
	/// <param name="state">Some context object to pass to the <paramref name="callback"/> method.</param>
	public void RequestItems(RequestDataNodesCallback callback, object state = null)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			SynchronizationContext synchronizationContext = SynchronizationContext.Current ?? mNode.DataTreeManager.Host.SynchronizationContext;
			DataNode[] nodes = RegularNodesOnlyUnsynced.ToArray();
			synchronizationContext.Post(_ => callback(this, nodes, state), null);
		}
	}

	/// <summary>
	/// Gets the data nodes in the collection asynchronously via the specified callback.<br/>
	/// The callback will be scheduled using the <see cref="SynchronizationContext"/> of the executing thread, if available.<br/>
	/// The invocation will always be scheduled on the data tree manager thread.<br/>
	/// This method is designed to work in conjunction with the <see cref="ChangedAsync"/> event.
	/// </summary>
	/// <param name="callback">Callback that will receive the data nodes that are currently in the collection.</param>
	/// <param name="state">Some context object to pass to the <paramref name="callback"/> method.</param>
	public void RequestItemsAsync(RequestDataNodesCallback callback, object state = null)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			SynchronizationContext synchronizationContext = mNode.DataTreeManager.Host.SynchronizationContext;
			DataNode[] nodes = RegularNodesOnlyUnsynced.ToArray();
			synchronizationContext.Post(_ => callback(this, nodes, state), null);
		}
	}

	#endregion

	#region ToArray()

	/// <summary>
	/// Gets all nodes in the collection as an array.
	/// </summary>
	/// <returns>An array containing all child nodes that are currently in the collection.</returns>
	public DataNode[] ToArray()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return [.. RegularNodesOnlyUnsynced];
		}
	}

	#endregion

	#region Internal Helpers

	/// <summary>
	/// Adds a dummy node (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the dummy node to add.</param>
	/// <param name="properties">Additional properties to set on the dummy node to add.</param>
	internal DataNode AddDummyNodeUnsynced(ReadOnlySpan<char> name, DataNodePropertiesInternal properties = DataNodePropertiesInternal.None)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		var node = new DataNode(mNode, name, properties | DataNodePropertiesInternal.Dummy);
		mBuffer.Add(node);

		// raise 'ViewerChanged' event and 'ViewerChangedAsync' event only
		// (the 'Changed' event and the 'ChangedAsync' event must be raised for regular nodes only)
		if (EventManager<ViewerDataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataNodeCollectionChangedEventArgs>.FireEvent(
				this,
				ViewerChangedEventName,
				ViewerWrapper,
				new ViewerDataNodeCollectionChangedEventArgs(
					CollectionChangedAction.Added,
					mNode.ViewerWrapper,
					node.ViewerWrapper));
		}

		return node;
	}

	/// <summary>
	/// Reinitializes the path of all data nodes and values recursively (for internal use only, not synchronized).
	/// </summary>
	internal void ReinitializePathsRecursivelyUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		mBuffer.ForEach(node => node.ReinitializePathsRecursivelyUnsynced());
	}

	/// <summary>
	/// Updates the data tree manager of all data nodes and data values recursively.
	/// </summary>
	/// <param name="dataTreeManager">The data tree manager to use.</param>
	internal void UpdateDataTreeManagerRecursivelyUnsynced(DataTreeManager dataTreeManager)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (DataNode node in mBuffer)
#else
		foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			node.UpdateDataTreeManagerRecursivelyUnsynced(dataTreeManager);
		}
	}

	/// <summary>
	/// Copies nodes from the current child node collection to the child node collection of the specified node
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="destinationNode">Node to copy nodes of the current collection to.</param>
	internal void CopyNodesUnsynced(DataNode destinationNode)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (DataNode node in mBuffer)
#else
		foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			node.CopyNodesUnsynced(destinationNode);
		}
	}

	/// <summary>
	/// Changes properties of all nodes in the collection and their data values recursively
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="nodePropertiesToSet">Data node properties to set.</param>
	/// <param name="nodePropertiesToClear">Data node properties to clear.</param>
	/// <param name="valuePropertiesToSet">Data value properties to set.</param>
	/// <param name="valuePropertiesToClear">Data value properties to clear.</param>
	/// <param name="topDown">
	/// <c>true</c> to modify the properties top-down,<br/>
	/// <c>false</c> to modify the properties bottom-up.
	/// </param>
	/// <remarks>
	/// If a flag is specified to be both 'set' and 'cleared', it will be finally set.
	/// </remarks>
	internal void SetPropertiesRecursivelyUnsynced(
		DataNodePropertiesInternal  nodePropertiesToSet,
		DataNodePropertiesInternal  nodePropertiesToClear,
		DataValuePropertiesInternal valuePropertiesToSet,
		DataValuePropertiesInternal valuePropertiesToClear,
		bool                        topDown)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert((nodePropertiesToSet & ~DataNodePropertiesInternal.All) == 0, $"The specified property flags (0x{nodePropertiesToSet:X}) are not supported.");
		Debug.Assert((nodePropertiesToClear & ~DataNodePropertiesInternal.All) == 0, $"The specified property flags (0x{nodePropertiesToClear:X}) are not supported.");
		Debug.Assert((valuePropertiesToSet & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToSet:X}) are not supported.");
		Debug.Assert((valuePropertiesToClear & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToClear:X}) are not supported.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (DataNode node in mBuffer)
#else
		foreach (DataNode node in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			node.SetPropertiesRecursivelyUnsynced(
				nodePropertiesToSet,
				nodePropertiesToClear,
				valuePropertiesToSet,
				valuePropertiesToClear,
				topDown);
		}
	}

	#endregion
}
