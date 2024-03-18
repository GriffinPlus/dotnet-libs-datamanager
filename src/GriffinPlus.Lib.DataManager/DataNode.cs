///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;
using GriffinPlus.Lib.Serialization;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A node in the data tree.
/// </summary>
[InternalObjectSerializer(1)]
[DebuggerDisplay("DataNode => Name: {" + nameof(Name) + "}, Properties: {" + nameof(Properties) + "}, Path: {" + nameof(Path) + "}")]
public partial class DataNode : IInternalObjectSerializer
{
	#region Changed

	internal const string ChangedEventName = "Changed";

	/// <summary>
	/// Occurs when a property of the current node changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the node.<br/>
	/// Subsequent invocations notify about changes to the node.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataNodeChangedEventArgs> Changed
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<DataNodeChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					SynchronizationContext.Current ?? SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataNodeChangedEventArgs(this, DataNodeChangedFlags.All | DataNodeChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<DataNodeChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a property of the current node changes.<br/>
	/// The first invocation of the event handler notifies about the initial state of the node.<br/>
	/// Subsequent invocations notify about changes to the node.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataNodeChangedEventArgs> ChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<DataNodeChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataNodeChangedEventArgs(this, DataNodeChangedFlags.All | DataNodeChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<DataNodeChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataNode"/> class (for a root node).
	/// </summary>
	/// <param name="name">Name of the node.</param>
	/// <param name="properties">Properties specifying the behaviour of the node.</param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use
	/// (<c>null</c> to use the default host specified by <see cref="DataManager.DefaultDataTreeManagerHost"/>).
	/// </param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use the default serializer specified by <see cref="DataManager.DefaultSerializer"/>).
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="name"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for node names.
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	public DataNode(
		string                 name,
		DataNodeProperties     properties,
		DataTreeManagerHost    dataTreeManagerHost = null,
		IDataManagerSerializer serializer          = null)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		if ((properties & ~DataNodeProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags (0x{properties & ~DataNodeProperties.All:X}).", nameof(properties));

		dataTreeManagerHost ??= DataManager.DefaultDataTreeManagerHost;
		serializer ??= DataManager.DefaultSerializer;

		// the new node becomes the root node of a data tree
		// => register a new data tree manager for it...
		DataTreeManager = dataTreeManagerHost.CreateDataTreeManager(this, serializer);

		// check whether the specified name is valid
		if (!PathHelpers.IsValidName(name.AsSpan()))
			throw new ArgumentException($"The specified name ({name}) does not comply with the rules for node names.", nameof(name));

		mName = name;
		mProperties = (DataNodePropertiesInternal)properties;
		mParent = null;
		mPath = PathHelpers.RootPath;
		Children = new ChildDataNodeCollection(this);
		Values = new DataValueCollection(this);

		// ensure all members are initialized before leaving the constructor
		// (otherwise some other thread may find a partially initialized object)
		Thread.MemoryBarrier();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataNode"/> class specifying the node behaviour
	/// (for a child node, for internal use only, not synchronized).
	/// </summary>
	/// <param name="parent">The parent node.</param>
	/// <param name="name">Name of the node.</param>
	/// <param name="properties">Properties specifying the behaviour of the node.</param>
	internal DataNode(
		DataNode                   parent,
		ReadOnlySpan<char>         name,
		DataNodePropertiesInternal properties)
	{
		Debug.Assert(Monitor.IsEntered(parent.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(PathHelpers.IsValidName(name), $"The specified name ({name.ToString()}) does not comply with the rules for node names.");

		DataTreeManager = parent.DataTreeManager;

		mName = name.ToString();
		mProperties = properties;
		mParent = parent;
		mPath = PathHelpers.AppendNameToPath(parent.PathUnsynced, name);
		Children = new ChildDataNodeCollection(this);
		Values = new DataValueCollection(this);
	}

	#endregion

	#region Serialization using Griffin+ Serializer

	/// <summary>
	/// Initializes a new <see cref="DataNode"/> instance from a serializer archive (for internal use only).
	/// </summary>
	/// <param name="archive">Serializer archive to initialize a new <see cref="DataNode"/> instance from.</param>
	/// <exception cref="VersionNotSupportedException">Archive was created using a serializer of a greater version than supported.</exception>
	internal DataNode(DeserializationArchive archive)
	{
		if (archive.Version == 1)
		{
			if (archive.Context is not DataDeserializationContext context)
				throw new SerializationException($"The serialization archive does not contain a {nameof(DataDeserializationContext)}.");

			mParent = context.ParentNode;
			mName = archive.ReadString();
			mProperties = archive.ReadEnum<DataNodePropertiesInternal>();

			if (mParent != null)
			{
				// child node
				DataTreeManager = context.ParentNode.DataTreeManager;
				mPath = PathHelpers.AppendNameToPath(mParent.mPath, mName.AsSpan());
			}
			else
			{
				// root node
				DataTreeManager = new DataTreeManager(context.DataTreeManagerHost, this, context.Serializer);
				mPath = PathHelpers.RootPath;
			}

			var newContext = new DataDeserializationContext(this, context.DataTreeManagerHost, context.Serializer);
			Children = (ChildDataNodeCollection)archive.ReadObject(newContext);
			Values = (DataValueCollection)archive.ReadObject(newContext);
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
	/// Serializes the current object to a serializer archive.
	/// </summary>
	/// <param name="archive">Archive to serialize the current object to.</param>
	/// <exception cref="VersionNotSupportedException">Requested serializer version is greater than supported.</exception>
	void IInternalObjectSerializer.Serialize(SerializationArchive archive)
	{
		lock (DataTreeManager.Sync)
		{
			// serialize the current node and its child nodes and values
			if (archive.Version == 1)
			{
				archive.Write(mName);
				archive.Write(mProperties);
				archive.Write(Children);
				archive.Write(Values);
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}
	}

	#endregion

	#region Serialization to/from Stream

	/// <summary>
	/// Serializes the current node and its child nodes and values to the specified stream using
	/// the serializer associated with the data tree.
	/// </summary>
	/// <param name="stream">The stream to write the node and its child nodes and values to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
	/// <exception cref="SerializationException">Serializing object failed. See exception text and inner exception for details.</exception>
	public void ToStream(Stream stream)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		DataTreeManager.Serializer.Serialize(this, stream);
	}

	/// <summary>
	/// Deserializes a node and its child nodes and values, i.e. a data tree, from the specified stream.
	/// </summary>
	/// <param name="stream">
	/// Stream containing a node that has been serialized using the specified <paramref name="serializer"/>.
	/// </param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use (<c>null</c> to use <see cref="DataManager.DefaultDataTreeManagerHost"/>).
	/// </param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use <see cref="DataManager.DefaultSerializer"/>).
	/// </param>
	/// <returns>The deserialized data tree.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
	public static DataNode FromStream(
		Stream                 stream,
		DataTreeManagerHost    dataTreeManagerHost = null,
		IDataManagerSerializer serializer          = null)
	{
		if (stream == null) throw new ArgumentNullException(nameof(stream));
		serializer ??= DataManager.DefaultSerializer;
		dataTreeManagerHost ??= DataManager.DefaultDataTreeManagerHost;
		return serializer.Deserialize(stream, dataTreeManagerHost);
	}

	#endregion

	#region Serialization to/from File

	/// <summary>
	/// Serializes the current node and its child nodes and values, i.e. the data tree,
	/// to the specified stream using the serializer associated with the data tree.
	/// </summary>
	/// <param name="fileName">Name of the file to write the node to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
	/// <exception cref="SerializationException">Serializing object failed. See exception text and inner exception for details.</exception>
	public void Save(string fileName)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));
		DataTreeManager.Serializer.WriteToFile(this, fileName);
	}

	/// <summary>
	/// Deserializes a node and its child nodes and values, i.e. a data tree, from the specified file.
	/// </summary>
	/// <param name="fileName">Name of the file to read the specified file from.</param>
	/// <param name="serializer">
	/// Serializer to use for serializing, deserializing and copying data in the data tree
	/// (<c>null</c> to use <see cref="DataManager.DefaultSerializer"/>).
	/// </param>
	/// <param name="dataTreeManagerHost">
	/// Data tree manager host to use (<c>null</c> to use <see cref="DataManager.DefaultDataTreeManagerHost"/>).
	/// </param>
	/// <returns>Root node of the loaded data tree.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is <c>null</c>.</exception>
	/// <exception cref="FileNotFoundException"><paramref name="fileName"/> does not exist.</exception>
	/// <exception cref="SerializationException">Deserializing object failed. See exception text and inner exception for details.</exception>
	public static DataNode FromFile(
		string                 fileName,
		IDataManagerSerializer serializer          = null,
		DataTreeManagerHost    dataTreeManagerHost = null)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));
		serializer ??= DataManager.DefaultSerializer;
		dataTreeManagerHost ??= DataManager.DefaultDataTreeManagerHost;
		return serializer.ReadFromFile(fileName, dataTreeManagerHost);
	}

	#endregion

	#region DataTreeManager

	/// <summary>
	/// Gets the data tree manager.
	/// </summary>
	public DataTreeManager DataTreeManager { get; private set; }

	#endregion

	#region Name

	private string mName;

	/// <summary>
	/// Gets or sets the name of the node.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException"><paramref name="value"/> does not comply with the rules for node names.</exception>
	/// <exception cref="DataNodeExistsAlreadyException">A node with the specified name does exist already at the current level.</exception>
	public string Name
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return mName;
			}
		}

		set
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			// check whether the specified name is valid
			if (!PathHelpers.IsValidName(value.AsSpan()))
				throw new ArgumentException($"The specified name ({value}) does not comply with the rules for node names.", nameof(value));

			lock (DataTreeManager.Sync)
			{
				// make the node regular, if it is a dummy node before proceeding
				// (the ViewerDataNode class uses this property as well, so this node can also be dummy...)
				if (IsDummyUnsynced) Regularize();

				// abort if the name did not change
				if (mName == value)
					return;

				// ensure there is no node with the same name in the parent's child node collection
				if (mParent?.Children.GetByNameUnsynced(value) != null)
					throw new DataNodeExistsAlreadyException($"A node with the same name ({value}) does already exist at this level.");

				// remove data value references from data values below that node
				IUntypedDataInternal[] references = DataTreeManager.UnregisterDataValueReferencesBelowNodeUnsynced(this, true);

				// change the name and the resulting path and raise the appropriate events
				mName = value;
				mPath = mParent != null ? PathHelpers.AppendNameToPath(mParent.PathUnsynced, mName.AsSpan()) : PathHelpers.RootPath;
				if (EventManager<DataNodeChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
				{
					EventManager<DataNodeChangedEventArgs>.FireEvent(
						this,
						ChangedEventName,
						this,
						new DataNodeChangedEventArgs(this, DataNodeChangedFlags.Name | DataNodeChangedFlags.Path));
				}
				if (EventManager<ViewerDataNodeChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
				{
					EventManager<ViewerDataNodeChangedEventArgs>.FireEvent(
						this,
						ViewerChangedEventName,
						this,
						new ViewerDataNodeChangedEventArgs(this, ViewerDataNodeChangedFlags.Name | ViewerDataNodeChangedFlags.Path));
				}

				// change the path of all child nodes and data values
				Children.ReinitializePathsRecursivelyUnsynced();
				Values.ReinitializePathsRecursivelyUnsynced();

				// update data value reference objects to point to the proper data values
				DataTreeManager.RegisterDataValueReferencesUnsynced(references);
			}
		}
	}

	/// <summary>
	/// Gets the name of the current node (for internal use only, not synchronized)
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
	internal string NameUnsynced => mName;

	#endregion

	#region Properties

	private DataNodePropertiesInternal mProperties;

	/// <summary>
	/// Gets or sets the properties of the node.
	/// </summary>
	/// <exception cref="ArgumentException">The specified properties contain unsupported flags.</exception>
	/// <remarks>
	/// If the current node is dummy, setting this property regularizes the node.<br/>
	/// Setting the <see cref="DataNodeProperties.Persistent"/> flag makes all nodes up to the root node persistent
	/// as well to ensure that the node is included when persisting the data tree.
	/// </remarks>
	public DataNodeProperties Properties
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return (DataNodeProperties)(mProperties & DataNodePropertiesInternal.UserProperties);
			}
		}

		set
		{
			// ensure that only valid property flags are specified
			if ((value & ~DataNodeProperties.All) != 0)
				throw new ArgumentException($"The specified properties (0x{value:X}) contain unsupported flags (0x{value & ~DataNodeProperties.All:X}).", nameof(value));

			lock (DataTreeManager.Sync)
			{
				// set user flags only and keep administrative flags except the dummy flag to regularize the node, if necessary
				PropertiesUnsynced = (mProperties & ~DataNodePropertiesInternal.UserProperties & ~DataNodePropertiesInternal.Dummy) |
				                     (DataNodePropertiesInternal)value;
			}
		}
	}

	/// <summary>
	/// Gets the properties of the current node (for internal use only, not synchronized).
	/// </summary>
	/// <remarks>
	/// This property supports setting and clearing administrative flags.<br/>
	/// If the current node is regular, setting the <see cref="DataNodeProperties.Persistent"/> flag makes all
	/// nodes up to the root node persistent as well to ensure that the node is included when persisting the
	/// data tree.<br/>
	/// If the current node is dummy, its parents are not touched. In this case the nodes are made persistent
	/// when the node is regularized later on.
	/// </remarks>
	internal DataNodePropertiesInternal PropertiesUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mProperties;
		}

		set
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			Debug.Assert((value & ~DataNodePropertiesInternal.All) == 0, $"The specified properties (0x{value:X}) contain unsupported flags ((0x{value & ~DataNodePropertiesInternal.All:X})).");

			// regularize nodes up to the root node if the node becomes regular
			if ((mProperties & DataNodePropertiesInternal.Dummy) != 0 && (value & DataNodePropertiesInternal.Dummy) == 0)
			{
				mParent?.Regularize();
			}

			// make all nodes on the path to this node persistent as well, if the value is regular
			// (for dummy nodes this is done as soon as the node becomes regular)
			if ((value & DataNodePropertiesInternal.Dummy) == 0 && (value & DataNodePropertiesInternal.Persistent) != 0)
			{
				mParent?.MakePersistent();
			}

			// abort if the properties did not change
			if (mProperties == value)
				return;

			// change properties
			DataNodePropertiesInternal oldProperties = mProperties;
			DataNodePropertiesInternal oldUserProperties = mProperties & DataNodePropertiesInternal.UserProperties;
			mProperties = value;

			// abort if no event handler is registered
			if (!EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
				return;

			// determine flags reflecting the property changes
			DataNodePropertiesInternal newProperties = mProperties;
			DataNodePropertiesInternal newUserProperties = mProperties & DataNodePropertiesInternal.UserProperties;
			var changedFlags = DataNodeChangedFlags.None;
			// add changed flags for all properties (user flags + administrative flags)
			changedFlags |= (DataNodeChangedFlags)((~oldProperties & newProperties) | (oldProperties & ~newProperties));
			// add changed flag for the 'Properties' property, if user properties have changed
			changedFlags |= ((~oldUserProperties & newUserProperties) | (oldUserProperties & ~newUserProperties)) != 0
				                ? DataNodeChangedFlags.Properties
				                : DataNodeChangedFlags.None;

			// raise event
			EventManager<DataNodeChangedEventArgs>.FireEvent(
				this,
				ChangedEventName,
				this,
				new DataNodeChangedEventArgs(this, changedFlags));
		}
	}

	#endregion

	#region IsPersistent

	/// <summary>
	/// Gets or sets a value indicating whether the current node is serialized when persisting.
	/// </summary>
	/// <remarks>
	/// If the current node is dummy, setting this property regularizes the node.<br/>
	/// Setting this property to <c>true</c> makes all nodes up to the root node persistent as well to ensure the
	/// current node value is included when persisting the data tree.
	/// </remarks>
	public bool IsPersistent
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return (mProperties & DataNodePropertiesInternal.Persistent) != 0;
			}
		}

		set
		{
			lock (DataTreeManager.Sync)
			{
				DataNodePropertiesInternal newProperties = value
					                                           ? mProperties | DataNodePropertiesInternal.Persistent
					                                           : mProperties & ~DataNodePropertiesInternal.Persistent;
				newProperties &= ~DataNodePropertiesInternal.Dummy; // regularize the node, since it has been touched by the user
				PropertiesUnsynced = newProperties;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the current node is serialized when persisting
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <remarks>
	/// If the current node is dummy, setting this property regularizes the node.<br/>
	/// Setting this property to <c>true</c> makes all nodes up to the root node persistent as well to ensure the
	/// current node value is included when persisting the data tree.
	/// </remarks>
	internal bool IsPersistentUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return (mProperties & DataNodePropertiesInternal.Persistent) != 0;
		}
	}

	#endregion

	#region IsDummy

	/// <summary>
	/// Gets a value indicating whether the current node is dummy (for internal use only, not synchronized).
	/// </summary>
	internal bool IsDummyUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return (mProperties & DataNodePropertiesInternal.Dummy) != 0;
		}
	}

	#endregion

	#region Parent

	private DataNode mParent;

	/// <summary>
	/// Gets the parent of the current node.
	/// </summary>
	/// <value>
	/// The parent node;<br/>
	/// <c>null</c> if the node is the root node.
	/// </value>
	public DataNode Parent
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return mParent;
			}
		}
	}

	/// <summary>
	/// Gets the parent of the current node (for internal use only, not synchronized).
	/// </summary>
	/// <value>
	/// The parent node;<br/>
	/// <c>null</c> if the node is the root node.
	/// </value>
	internal DataNode ParentUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mParent;
		}
	}

	#endregion

	#region Path

	private string mPath;

	/// <summary>
	/// Gets the path of the current node in the data tree.
	/// </summary>
	public string Path
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return mPath;
			}
		}
	}

	/// <summary>
	/// Gets the path of the current node in the data tree (for internal use only, not synchronized)
	/// </summary>
	public string PathUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mPath;
		}
	}

	#endregion

	#region Children

	/// <summary>
	/// Gets the collection of child nodes associated with the current node.
	/// </summary>
	public ChildDataNodeCollection Children { get; }

	#endregion

	#region Values

	/// <summary>
	/// Gets collection of values associated with the current node.
	/// </summary>
	public DataValueCollection Values { get; }

	#endregion

	#region Remove Node from the Tree

	/// <summary>
	/// Removes the current node from the data tree.
	/// </summary>
	/// <remarks>
	/// This method removes the current node from the data tree.
	/// The current node becomes the root node of its child nodes.
	/// </remarks>
	public void Remove()
	{
		lock (DataTreeManager.Sync)
		{
			// the current node gets its own data tree manager,
			// but the synchronization object remains the same to avoid synchronization issues
			mParent?.Children.Remove(this);
		}
	}

	#endregion

	#region Getting a Data Value Reference (Data<T>)

	/// <summary>
	/// Gets a data value reference (<see cref="Data{T}"/>) referencing the data value at the specified path.
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value stored in the addressed data value.
	/// Corresponds to the generic type argument of <see cref="DataValue{T}"/>.
	/// </typeparam>
	/// <param name="path">Path of the data value to reference.</param>
	/// <returns>A data value reference to the data value at the specified path.</returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.
	/// </exception>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	public Data<T> GetData<T>(string path)
	{
		return GetData<T>(path, DataValueProperties.None);
	}

	/// <summary>
	/// Gets a data value reference (<see cref="Data{T}"/>) referencing the data value at the specified path.
	/// </summary>
	/// <typeparam name="T">
	/// Type of the value stored in the addressed data value.
	/// Corresponds to the generic type argument of <see cref="DataValue{T}"/>.
	/// </typeparam>
	/// <param name="path">Path of the data value to reference.</param>
	/// <param name="properties">Properties to apply to the referenced data value, if it does not exist, yet.</param>
	/// <returns>A data value reference to the data value at the specified path.</returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/>properties contain unsupported flags.
	/// </exception>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	public Data<T> GetData<T>(string path, DataValueProperties properties)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// ensure that only valid property flags are specified
		if ((properties & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags ((0x{properties & ~DataValueProperties.All:X})).", nameof(properties));

		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// the path is syntactically ok
		// => get/create the addressed data value and put a data value reference on top of it...
		lock (DataTreeManager.Sync)
		{
			DataNode node = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;
			return new Data<T>(
				node.GetDataValueInternalUnsynced(
					ref pathParser,
					(DataValuePropertiesInternal)properties | DataValuePropertiesInternal.Dummy, // initial properties, if data value does not exist, yet
					default(T)));                                                                // initial value (ignored, since data value is created as a dummy)
		}
	}

	#endregion

	#region Getting a Data Value (DataValue<T>) with Path Creation

	/// <summary>
	/// Gets the data value (<see cref="DataValue{T}"/>) at the specified path.<br/>
	/// This method creates the path to the data value, if it does not exist, yet.
	/// </summary>
	/// <typeparam name="T">Type of the value stored in the data value.</typeparam>
	/// <param name="path">Path of the data value to get.</param>
	/// <param name="properties">Properties to apply to the data value, if it does not exist, yet.</param>
	/// <param name="value">Initial value of the data value, if it does not exist, yet.</param>
	/// <returns>The data value at the specified path.</returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	public DataValue<T> GetDataValue<T>(
		string              path,
		DataValueProperties properties,
		T                   value)
	{
		if (path == null) throw new ArgumentNullException(nameof(path));

		// ensure that only valid property flags are specified
		if ((properties & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags ((0x{properties & ~DataValueProperties.All:X})).", nameof(properties));

		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// the path is syntactically ok
		// => get requested data value and create path to it, if necessary
		lock (DataTreeManager.Sync)
		{
			DataNode node = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;
			return node.GetDataValueInternalUnsynced(
				ref pathParser,
				(DataValuePropertiesInternal)properties, // initial properties, if data value does not exist, yet
				value);                                  // initial value, if the data value does not exist, yet
		}
	}

	/// <summary>
	/// Gets the data value (<see cref="DataValue{T}"/>) at the specified path.<br/>
	/// This method creates the path, if it does not exist, yet.<br/>
	/// For internal use only, not synchronized.
	/// </summary>
	/// <typeparam name="T">Type of the value stored in the data value.</typeparam>
	/// <param name="pathParser">Path parser set up to parse the path to the requested data value.</param>
	/// <param name="initialProperties">Properties to apply to the data value, if it does not exist, yet.</param>
	/// <param name="initialValue">The initial value of the data value, if it does not exist, yet.</param>
	/// <returns>The data value at the specified path.</returns>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	internal DataValue<T> GetDataValueInternalUnsynced<T>(
		ref PathParser              pathParser,
		DataValuePropertiesInternal initialProperties,
		T                           initialValue)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;
		bool dummyNodesAdded = false;
		try
		{
			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool isLastToken))
			{
				if (!isLastToken)
				{
					// a data node
					// => get existing node, create a dummy node if necessary
					DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);
					if (nextDataNode == null)
					{
						nextDataNode = currentDataNode.Children.AddDummyNodeUnsynced(token);
						dummyNodesAdded = true;
					}
					currentDataNode = nextDataNode;
					continue;
				}

				// add data value, if there is no data value with the specified name
				IUntypedDataValueInternal untypedDataValue = currentDataNode.Values.FindUnsynced(token);
				if (untypedDataValue == null)
				{
					if ((initialProperties & DataValuePropertiesInternal.Dummy) != 0)
					{
						// add a new dummy data value
						return currentDataNode.Values.AddInternalUnsynced<T>(
							token,
							initialProperties,
							default);
					}

					// add a new regular data value
					return currentDataNode.Values.AddInternalUnsynced(
						token,
						initialProperties,
						DataTreeManager.Serializer.CopySerializableValue(initialValue)); // would be better to do outside the lock
				}

				// data value exists already

				// ensure that the type of the data value is as expected
				if (untypedDataValue.Type != typeof(T))
					throw new DataTypeMismatchException($"Cannot get value at '{pathParser.Path.ToString()}' due to a type mismatch (existing value type: {untypedDataValue.Type.FullName}, trying to bind type: {typeof(T).FullName}).");

				// type is as expected
				// => cast to DataValue<T> to avoid boxing of values
				var dataValue = (DataValue<T>)untypedDataValue;

				// abort if the data value is regular
				if (!dataValue.IsDummyUnsynced)
					return dataValue;

				// existing data value is dummy
				// => set value and/or properties
				if ((initialProperties & DataValuePropertiesInternal.Dummy) != 0)
				{
					// data value should stay dummy
					// => set properties only
					dataValue.PropertiesUnsynced = initialProperties;
				}
				else
				{
					// data value should become regular
					// => set value and properties
					dataValue.SetUnsynced(
						DataTreeManager.Serializer.CopySerializableValue(initialValue), // TODO: Would be better to do outside the lock...
						initialProperties,
						DataValuePropertiesInternal.All);
				}

				// return the data value
				return dataValue;
			}
		}
		catch (Exception)
		{
			// remove added dummy nodes that are not needed any more, if any
			if (dummyNodesAdded) currentDataNode.CleanupDummiesUnsynced();
			throw;
		}

		Debug.Fail("Should never occur.");
		return null;
	}

	#endregion

	#region Getting a Data Value (DataValue<T>) without Path Creation

	/// <summary>
	/// Gets the data value (<see cref="DataValue{T}"/>) at the specified path.<br/>
	/// This method does not create the path to the data value, if it does not exist, yet.
	/// </summary>
	/// <typeparam name="T">Type of the value stored in the data value.</typeparam>
	/// <param name="path">Path of the data value to get.</param>
	/// <returns>
	/// The data value at the specified path;<br/>
	/// <c>null</c> if the data value does not exist.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.
	/// </exception>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	public DataValue<T> GetDataValue<T>(string path)
	{
		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// the path is syntactically ok
		// => get requested data value
		lock (DataTreeManager.Sync)
		{
			DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;

			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool isLastToken))
			{
				if (!isLastToken)
				{
					// a data node
					// => get existing node
					DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);
					if (nextDataNode == null) return null;
					if (nextDataNode.IsDummyUnsynced) return null;
					currentDataNode = nextDataNode;
					continue;
				}

				// get data value
				// => abort, if the data value does not exist or is dummy
				IUntypedDataValueInternal dataValue = currentDataNode.Values.FindUnsynced(token);
				if (dataValue == null) return null;
				if (dataValue.IsDummyUnsynced) return null;

				// data value exists and is regular

				// return data value if the type of the data value is as expected
				if (dataValue.Type == typeof(T))
					return dataValue as DataValue<T>;

				// the type of the data value is not as expected
				throw new DataTypeMismatchException($"Cannot get value at '{path}' due to a type mismatch (existing value type: {dataValue.Type.FullName}, trying to bind type: {typeof(T).FullName}).");
			}
		}

		Debug.Fail("Should never occur.");
		return null;
	}

	#endregion

	#region Setting a Data Value (DataValue<T>)

	/// <summary>
	/// Sets the data value (<see cref="DataValue{T}"/>) at the specified path.<br/>
	/// This method creates the path to the data value, if it does not exist, yet.
	/// </summary>
	/// <typeparam name="T">Type of the value stored in the data value.</typeparam>
	/// <param name="path">Path of the data value to set.</param>
	/// <param name="properties">Properties to apply to the data value.</param>
	/// <param name="value">Value of the data value to set.</param>
	/// <returns>The data value at the specified path.</returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataTypeMismatchException">
	/// A data value exists at the specified path, but it's type of different from <typeparamref name="T"/>.
	/// </exception>
	public DataValue<T> SetValue<T>(string path, T value, DataValueProperties properties)
	{
		// ensure that only valid property flags are specified
		if ((properties & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags ((0x{properties & ~DataValueProperties.All:X})).", nameof(properties));

		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// copy value to store in the data tree
		T valueCopy = DataTreeManager.Serializer.CopySerializableValue(value);

		// the path is syntactically ok
		// => get requested data value and create path to it, if necessary
		lock (DataTreeManager.Sync)
		{
			DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;

			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool isLastToken))
			{
				if (!isLastToken)
				{
					// a data node
					DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);

					if (nextDataNode != null)
					{
						// data node exists already
						// => regularize node, if necessary, and make it persistent, if the data value is persistent
						DataNodePropertiesInternal newProperties = nextDataNode.PropertiesUnsynced;
						newProperties &= DataNodePropertiesInternal.Dummy;
						newProperties |= (properties & DataValueProperties.Persistent) != 0
							                 ? DataNodePropertiesInternal.Persistent
							                 : DataNodePropertiesInternal.None;
						if (nextDataNode.PropertiesUnsynced != newProperties)
							nextDataNode.PropertiesUnsynced = newProperties;
					}
					else
					{
						// data node does not exist, yet
						// => add a new regular data node, make it persistent, if the data value is persistent
						DataNodePropertiesInternal nodeProperties = ((DataValuePropertiesInternal)properties & DataValuePropertiesInternal.Persistent) != 0
							                                            ? DataNodePropertiesInternal.Persistent
							                                            : DataNodePropertiesInternal.None;
						nextDataNode = currentDataNode.Children.AddInternalUnsynced(token, nodeProperties);
					}

					currentDataNode = nextDataNode;
					continue;
				}

				// add data value if there is no data value with the specified name
				IUntypedDataValueInternal untypedDataValue = currentDataNode.Values.FindUnsynced(token);
				if (untypedDataValue == null)
					return currentDataNode.Values.AddInternalUnsynced(token, (DataValuePropertiesInternal)properties, valueCopy);

				// data value exists already

				if (untypedDataValue.IsDummyUnsynced)
				{
					// data value is dummy
					if (untypedDataValue.Type != typeof(T))
					{
						// the type of the data value is not as expected
						// => remove dummy data value and replace it with the new regular one...

						// remove all data value references from the data value to remove
						DataTreeManager.InvalidateReferencesForDataValueUnsynced(untypedDataValue.PathUnsynced, false);

						// remove the data value
						DataNode parentNode = untypedDataValue.ParentNodeUnsynced;
						parentNode.Values.RemoveUnsynced(untypedDataValue);
						Debug.Assert(untypedDataValue.ParentNodeUnsynced == null);

						// add value
						return currentDataNode.Values.AddInternalUnsynced(
							mName.AsSpan(),
							(DataValuePropertiesInternal)properties,
							valueCopy);
					}
				}
				else
				{
					// data value is regular
					// => ensure that the type of the data value is as expected
					if (untypedDataValue.Type != typeof(T))
						throw new DataTypeMismatchException($"Cannot set value at '{path}' due to a type mismatch (existing value type: {untypedDataValue.Type.FullName}, trying to bind type: {typeof(T).FullName}).");
				}

				// data value is of the expected type

				// set value and properties
				var dataValue = (DataValue<T>)untypedDataValue;
				dataValue.SetUnsynced(
					valueCopy,
					(DataValuePropertiesInternal)properties,
					DataValuePropertiesInternal.All);

				return dataValue;
			}
		}

		Debug.Fail("Should never occur.");
		return null;
	}

	#endregion

	#region Getting a Data Value (IDataValue) without Path Creation

	/// <summary>
	/// Gets the untyped data value (<see cref="IUntypedDataValue"/>) at the specified path.<br/>
	/// This method does not create the path to the data value, if it does not exist, yet.
	/// </summary>
	/// <param name="path">Path of the data value to get.</param>
	/// <returns>
	/// The data value at the specified path;<br/>
	/// <c>null</c> if the data value does not exist.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.
	/// </exception>
	public IUntypedDataValue GetDataValueUntyped(string path)
	{
		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// the path is syntactically ok
		// => get requested data value
		lock (DataTreeManager.Sync)
		{
			DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;

			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool isLastToken))
			{
				if (!isLastToken)
				{
					// a data node in the path
					DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);
					if (nextDataNode == null) return null;
					if (nextDataNode.IsDummyUnsynced) return null;
					currentDataNode = nextDataNode;
					continue;
				}

				// a data value in the path
				IUntypedDataValueInternal dataValue = currentDataNode.Values.FindUnsynced(token);
				if (dataValue == null) return null;
				return dataValue.IsDummyUnsynced ? null : dataValue;
			}
		}

		Debug.Fail("Should never occur.");
		return null;
	}

	/// <summary>
	/// Gets the untyped data value (<see cref="IUntypedDataValue"/>) at the specified path (for internal use only, not synchronized).<br/>
	/// This method does not create the path to the data value, if it does not exist, yet.
	/// </summary>
	/// <param name="path">Path of the data value to get.</param>
	/// <returns>
	/// The data value at the specified path;<br/>
	/// <c>null</c> if the data value does not exist.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// The specified <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.
	/// </exception>
	internal IUntypedDataValueInternal GetDataValueUntypedUnsynced(ReadOnlySpan<char> path)
	{
		// parse and validate the path
		var pathParser = PathParser.Create(path, true); // throws ArgumentException on validation errors

		DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;

		while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool isLastToken))
		{
			if (!isLastToken)
			{
				// a data node in the path
				DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);
				if (nextDataNode == null) return null;
				if (nextDataNode.IsDummyUnsynced) return null;
				currentDataNode = nextDataNode;
				continue;
			}

			// a data value in the path
			IUntypedDataValueInternal dataValue = currentDataNode.Values.FindUnsynced(token);
			if (dataValue == null) return null;
			return dataValue.IsDummyUnsynced ? null : dataValue;
		}

		Debug.Fail("Should never occur.");
		return null;
	}

	#endregion

	#region Getting a Child Data Node (DataNode) without Path Creation

	/// <summary>
	/// Gets the data node (<see cref="DataNode"/>) at the specified path.<br/>
	/// This method does not create the path to the data node, if it does not exist, yet.
	/// </summary>
	/// <param name="path">Path of the data node to get.</param>
	/// <returns>
	/// The data node at the specified path;<br/>
	/// <c>null</c> if the node does not exist, yet.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="path"/> is <c>null</c>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.
	/// </exception>
	public DataNode GetDataNode(string path)
	{
		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		lock (DataTreeManager.Sync)
		{
			DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;
			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool _))
			{
				DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);
				if (nextDataNode == null) return null;
				if (nextDataNode.IsDummyUnsynced) return null;
				currentDataNode = nextDataNode;
			}

			return currentDataNode;
		}
	}

	#endregion

	#region Getting a Child Data Node (DataNode) with Path Creation

	/// <summary>
	/// Gets the data node (<see cref="DataNode"/>) at the specified path.<br/>
	/// This method creates the path to the addressed data node, if it does not exist, yet.
	/// </summary>
	/// <param name="path">Path of the data node to get.</param>
	/// <param name="properties">Properties to apply to the data node, if it does not exist, yet.</param>
	/// <returns>The data node at the specified path.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="path"/> is malformed.<br/>
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	public DataNode GetDataNode(string path, DataNodeProperties properties)
	{
		// ensure that only valid property flags are specified
		if ((properties & ~DataNodeProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags (0x{properties & ~DataNodeProperties.All:X}).", nameof(properties));

		// parse and validate the path
		var pathParser = PathParser.Create(path.AsSpan(), true); // throws ArgumentException on validation errors

		// the path is syntactically ok
		// => get requested data node and create path to it, if necessary
		lock (DataTreeManager.Sync)
		{
			DataNode currentDataNode = pathParser.IsAbsolutePath ? DataTreeManager.RootNode : this;

			while (pathParser.GetNext(out ReadOnlySpan<char> token, out bool _))
			{
				DataNode nextDataNode = currentDataNode.Children.GetByNameUnsynced(token);

				if (nextDataNode != null)
				{
					// node exists already
					// => regularize if necessary
					if (nextDataNode.IsDummyUnsynced)
						nextDataNode.PropertiesUnsynced = (DataNodePropertiesInternal)properties;
				}
				else
				{
					// node does not exist, yet
					// => add it...
					nextDataNode = currentDataNode.Children.AddInternalUnsynced(token, (DataNodePropertiesInternal)properties);
				}

				currentDataNode = nextDataNode;
			}

			return currentDataNode;
		}
	}

	#endregion

	#region Copying the current node (including its child nodes and values)

	private static readonly Regex sBaseNameRegex = new(@"[\ ]#[0-9]+$", RegexOptions.Compiled);

	/// <summary>
	/// Copies the current node (including its child nodes and values) beneath the specified node.
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
	/// <exception cref="DataNodeExistsAlreadyException">
	/// There is already a data node with the name of the current node and <paramref name="renameIfNecessary"/> is <c>false</c>.
	/// </exception>
	/// <remarks>
	/// This method inserts a copy of the current node (including its child nodes and values) beneath the specified node
	/// and optionally renames the copied node, if a node with the same name exists already by suffixing the name with
	/// a hashtag and an incrementing number (i.e. #2, #3, #4, etc.).
	/// </remarks>
	public DataNode Copy(DataNode destinationNode, bool renameIfNecessary = false)
	{
		lock (DataTreeManager.Sync)
		lock (destinationNode.DataTreeManager.Sync)
		{
			return CopyUnsynced(destinationNode, renameIfNecessary);
		}
	}

	/// <summary>
	/// Copies the current node (including its child nodes and values) beneath the specified node
	/// (for internal use only, not synchronized).
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
	private DataNode CopyUnsynced(DataNode destinationNode, bool renameIfNecessary = false)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(Monitor.IsEntered(destinationNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		if (destinationNode == null) throw new ArgumentNullException(nameof(destinationNode));

		// ensure the current node is not a dummy node
		if ((mProperties & DataNodePropertiesInternal.Dummy) != 0)
			throw new InvalidOperationException("The data node is dummy and can therefore not be copied.");

		// check whether there is a regular node with the same name in the destination node's child collection
		// and whether a copy of the current node has to be renamed to resolve the conflict
		bool renameRequired = false;
		DataNode nodeInDestinationTree = destinationNode.Children.GetByNameUnsynced(mName);
		if (nodeInDestinationTree is { IsDummyUnsynced: false })
		{
			// existing node is a regular node
			// => abort, if renaming is not desired, otherwise remember to rename the node properly
			if (!renameIfNecessary) throw new DataNodeExistsAlreadyException("A node with the same name does already exist at this level.");
			renameRequired = true;
		}

		if (renameRequired)
		{
			// a regular node with the same name is already a child node of the destination node
			// => adjust name of the current node to resolve the conflict with the existing node beneath the destination node
			string baseName = sBaseNameRegex.Replace(mName, "");
			for (int i = 2;; i++)
			{
				string adjustedName = $"{baseName} #{i}";
				DataNode node = destinationNode.Children.GetByNameUnsynced(adjustedName);
				if (node != null)
				{
					// node exists already
					// => proceed with the next name to try if the existing node is regular
					if (!node.IsDummyUnsynced) continue;

					// the existing destination node is dummy
					// => remove data value references from the destination subtree
					IUntypedDataInternal[] references = DataTreeManager.UnregisterDataValueReferencesBelowNodeUnsynced(node, true);
					Debug.Assert(node.Children.CountUnsynced == 0);
					Debug.Assert(node.Values.CountUnsynced == 0);

					// regularize the dummy destination node and set its properties to the properties of the current node,
					// then copy its child nodes and values beneath this node
					node.PropertiesUnsynced = mProperties & DataNodePropertiesInternal.UserProperties;
					Children.CopyNodesUnsynced(node);
					Values.CopyValuesUnsynced(node);

					// add previously removed data value references, if any
					DataTreeManager.RegisterDataValueReferencesUnsynced(references);
					return node;
				}

				// node does not exist at all
				// => add a copy of the current node to the destination node's child collection,
				//    then copy its child nodes and values beneath this node
				node = destinationNode.Children.AddInternalUnsynced(adjustedName.AsSpan(), mProperties & DataNodePropertiesInternal.UserProperties);
				Children.CopyNodesUnsynced(node);
				Values.CopyValuesUnsynced(node);
				return node;
			}
		}

		// renaming is not required

		if (nodeInDestinationTree != null)
		{
			// node in destination tree exists already
			// => it is a dummy node, otherwise renaming would have been handled above...
			Debug.Assert(nodeInDestinationTree.IsDummyUnsynced);

			// the existing node is dummy
			// => remove data value references from the destination subtree
			IUntypedDataInternal[] references = DataTreeManager.UnregisterDataValueReferencesBelowNodeUnsynced(nodeInDestinationTree, true);
			Debug.Assert(nodeInDestinationTree.Children.CountUnsynced == 0);
			Debug.Assert(nodeInDestinationTree.Values.CountUnsynced == 0);

			// regularize the dummy destination node and set its properties to the properties of the current node,
			// then copy its child nodes and values beneath this node
			nodeInDestinationTree.PropertiesUnsynced = mProperties & DataNodePropertiesInternal.UserProperties;
			Children.CopyNodesUnsynced(nodeInDestinationTree);
			Values.CopyValuesUnsynced(nodeInDestinationTree);

			// add previously removed data value references, if any
			DataTreeManager.RegisterDataValueReferencesUnsynced(references);

			return nodeInDestinationTree;
		}

		// node in destination tree does not exist, yet
		// => create copy of the current node and all child nodes and values
		DataNode newNode = destinationNode.Children.AddInternalUnsynced(mName.AsSpan(), mProperties & DataNodePropertiesInternal.UserProperties);
		Children.CopyNodesUnsynced(newNode);
		Values.CopyValuesUnsynced(newNode);
		return newNode;
	}

	/// <summary>
	/// Copies the current node (including its child nodes and values) beneath the specified node
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="destinationNode">Node to copy the current node beneath.</param>
	internal void CopyNodesUnsynced(DataNode destinationNode)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(Monitor.IsEntered(destinationNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if this is a dummy node
		if (IsDummyUnsynced)
			return;

		// current node is regular
		// => start copying...
		DataNode dataNode = destinationNode.Children.GetByNameUnsynced(mName);
		if (dataNode != null)
		{
			// target node exists
			// => set its properties appropriately (regularizes automatically, if necessary)
			dataNode.PropertiesUnsynced = mProperties & DataNodePropertiesInternal.UserProperties;
		}
		else
		{
			// target node does not exist
			// => create it...
			dataNode = destinationNode.Children.AddInternalUnsynced(mName.AsSpan(), mProperties & DataNodePropertiesInternal.UserProperties);
		}

		Children.CopyNodesUnsynced(dataNode);
		Values.CopyValuesUnsynced(dataNode);
	}

	#endregion

	#region Executing Multiple Operations Atomically

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock
	/// (do not perform excessive calculations in the callback method and do not block, since the entire
	/// data tree is locked and blocking might result in a deadlock!!!)
	/// </summary>
	/// <param name="action">Method to call within the locked section.</param>
	public void ExecuteAtomically(DataNodeAction action)
	{
		lock (DataTreeManager.Sync)
		{
			action(this);
		}
	}

	/// <summary>
	/// Locks the entire data tree and calls the specified action method within the lock
	/// (do not perform excessive calculations in the callback method and do not block, since the entire
	/// data tree is locked and blocking might result in a deadlock!!!)
	/// </summary>
	/// <param name="action">Method to call within the locked section.</param>
	/// <param name="state">Some state object to pass to the action.</param>
	public void ExecuteAtomically<T>(DataNodeAction<T> action, T state)
	{
		lock (DataTreeManager.Sync)
		{
			action(this, state);
		}
	}

	#endregion

	#region Setting Data Node/Value Properties Recursively

	/// <summary>
	/// Changes the properties of the current node, its child nodes and/or data values recursively.
	/// </summary>
	/// <param name="nodePropertiesToSet">Data node properties to set.</param>
	/// <param name="nodePropertiesToClear">Data node properties to clear.</param>
	/// <param name="valuePropertiesToSet">Data value properties to set.</param>
	/// <param name="valuePropertiesToClear">Data value properties to clear.</param>
	/// <param name="topDown">
	/// <c>true</c> to traverse the tree top-down;<br/>
	/// <c>false</c> to traverse the tree bottom-up.
	/// </param>
	/// <exception cref="ArgumentException">The specified properties contain unsupported flags.</exception>
	/// <remarks>
	/// This method changes the properties of data nodes and data values recursively either in
	/// top-down or bottom-up fashion. If a flag is specified both 'set' and 'cleared', it will
	/// be finally set.
	/// </remarks>
	public void SetPropertiesRecursively(
		DataNodeProperties  nodePropertiesToSet,
		DataNodeProperties  nodePropertiesToClear,
		DataValueProperties valuePropertiesToSet,
		DataValueProperties valuePropertiesToClear,
		bool                topDown = true)
	{
		// ensure that only valid property flags are specified
		if ((nodePropertiesToSet & ~DataNodeProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{nodePropertiesToSet:X}) contain unsupported flags (0x{nodePropertiesToSet & ~DataNodeProperties.All:X}).", nameof(nodePropertiesToSet));
		if ((nodePropertiesToClear & ~DataNodeProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{nodePropertiesToClear:X}) contain unsupported flags (0x{nodePropertiesToClear & ~DataNodeProperties.All:X}).", nameof(nodePropertiesToClear));
		if ((valuePropertiesToSet & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{valuePropertiesToSet:X}) contain unsupported flags (0x{valuePropertiesToSet & ~DataValueProperties.All:X}).", nameof(valuePropertiesToSet));
		if ((valuePropertiesToClear & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{valuePropertiesToClear:X}) contain unsupported flags (0x{valuePropertiesToClear & ~DataValueProperties.All:X}).", nameof(valuePropertiesToClear));

		lock (DataTreeManager.Sync)
		{
			SetPropertiesRecursivelyUnsynced(
				(DataNodePropertiesInternal)nodePropertiesToSet,
				(DataNodePropertiesInternal)nodePropertiesToClear,
				(DataValuePropertiesInternal)valuePropertiesToSet,
				(DataValuePropertiesInternal)valuePropertiesToClear,
				topDown);
		}
	}

	/// <summary>
	/// Changes the properties of the current node, its child nodes and/or data values recursively
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
	/// This method changes the properties of data nodes and data values recursively either in
	/// top-down or bottom-up fashion. If a flag is specified both 'set' and 'cleared', it will
	/// be finally set.
	/// </remarks>
	internal void SetPropertiesRecursivelyUnsynced(
		DataNodePropertiesInternal  nodePropertiesToSet,
		DataNodePropertiesInternal  nodePropertiesToClear,
		DataValuePropertiesInternal valuePropertiesToSet,
		DataValuePropertiesInternal valuePropertiesToClear,
		bool                        topDown)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert((nodePropertiesToSet & ~DataNodePropertiesInternal.All) == 0, $"The specified property flags (0x{nodePropertiesToSet:X}) are not supported.");
		Debug.Assert((nodePropertiesToClear & ~DataNodePropertiesInternal.All) == 0, $"The specified property flags (0x{nodePropertiesToClear:X}) are not supported.");
		Debug.Assert((valuePropertiesToSet & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToSet:X}) are not supported.");
		Debug.Assert((valuePropertiesToClear & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToClear:X}) are not supported.");

		// determine effective property flags
		DataNodePropertiesInternal properties = (mProperties & ~nodePropertiesToClear) | nodePropertiesToSet;

		// set properties of the current node (top-down mode)
		if (topDown) PropertiesUnsynced = properties;

		// set properties of child nodes
		Children.SetPropertiesRecursivelyUnsynced(
			nodePropertiesToSet,
			nodePropertiesToClear,
			valuePropertiesToSet,
			valuePropertiesToClear,
			topDown);

		// set properties of data values
		Values.SetPropertiesRecursivelyUnsynced(valuePropertiesToSet, valuePropertiesToClear);

		// abort, if top-down mode...
		if (topDown) return;

		// set properties of the current node (bottom-up mode)
		PropertiesUnsynced = properties;
	}

	#endregion

	#region Removing a Set of Data Values

	/// <summary>
	/// Removes data nodes by their path from the data tree.
	/// </summary>
	/// <param name="paths">
	/// Paths to the data nodes to remove
	/// (supports absolute and relative paths, may also be <c>null</c>).
	/// </param>
	/// <returns>Number of data nodes actually removed.</returns>
	/// <exception cref="ArgumentException">At least one of the specified <paramref name="paths"/> is malformed.</exception>
	public int RemoveDataNodes(IEnumerable<string> paths)
	{
		// abort, if the enumeration is null
		if (paths == null) return 0;

		// parse and validate the paths
		// (throws ArgumentException if validation fails)
		string[] pathsArray = paths as string[] ?? paths.ToArray();
		foreach (string path in pathsArray) PathParser.Validate(path.AsSpan());

		// try to remove the specified data nodes
		int count = 0;
		ExecuteAtomically(
			currentNode =>
			{
				foreach (string path in pathsArray)
				{
					if (path == null) continue;
					DataNode node = currentNode.GetDataNode(path); // handles absolute and relative paths
					if (node == null) continue;
					node.Remove();
					count++;
				}
			});

		return count;
	}

	/// <summary>
	/// Removes data values by their path from the data tree.
	/// </summary>
	/// <param name="paths">
	/// Paths to the data values to remove
	/// (supports absolute and relative paths, may also be <c>null</c>).
	/// </param>
	/// <returns>Number of data values actually removed.</returns>
	/// <exception cref="ArgumentException">At least one of the specified <paramref name="paths"/> is malformed.</exception>
	public int RemoveDataValues(IEnumerable<string> paths)
	{
		// abort, if the enumeration is null
		if (paths == null) return 0;

		// parse and validate the paths
		// (throws ArgumentException if validation fails)
		string[] pathsArray = paths as string[] ?? paths.ToArray();
		foreach (string path in pathsArray) PathParser.Validate(path.AsSpan());

		// try to remove the specified data values
		int count = 0;
		ExecuteAtomically(
			currentNode =>
			{
				foreach (string path in pathsArray)
				{
					if (path == null) continue;
					IUntypedDataValueInternal dataValue = currentNode.GetDataValueUntypedUnsynced(path.AsSpan()); // handles absolute and relative paths
					if (dataValue == null) continue;
					dataValue.Remove();
					count++;
				}
			});

		return count;
	}

	#endregion

	#region Internal Helpers

	/// <summary>
	/// Converts the current node and all nodes up to the root node to a regular nodes, if these nodes are dummy nodes
	/// (for internal use only, not synchronized).
	/// </summary>
	internal void Regularize()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the current node is a regular node,
		// since nodes at an even higher level are regular nodes as well...
		if (!IsDummyUnsynced)
			return;

		// current node is a dummy node, perhaps its parent is one as well
		// => regularize its parent first...
		mParent?.Regularize();

		// regularize the current node now...
		DataNodePropertiesInternal newProperties = mProperties & ~DataNodePropertiesInternal.Dummy;
		mProperties = newProperties;

		// abort if no event handler is registered
		if (!EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			return;

		// raise event
		EventManager<ViewerDataNodeChangedEventArgs>.FireEvent(
			this,
			ChangedEventName,
			ViewerWrapper,
			new ViewerDataNodeChangedEventArgs(this, ViewerDataNodeChangedFlags.IsDummy));
	}

	/// <summary>
	/// Makes the current node and all nodes up to the root node persistent (for internal use only, not synchronized).
	/// </summary>
	internal void MakePersistent()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// make all nodes up to the root persistent
		mParent?.MakePersistent();

		// make current node persistent
		DataNodePropertiesInternal newProperties = mProperties | DataNodePropertiesInternal.Persistent;
		if (newProperties == mProperties) return;
		DataNodePropertiesInternal oldProperties = mProperties;
		mProperties = newProperties;

		// abort if no event handler is registered
		if (!EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			return;

		// raise event
		DataNodeChangedFlags changedFlags = DataNodeChangedFlags.Properties | (DataNodeChangedFlags)((~mProperties & oldProperties) | (mProperties & ~oldProperties));
		EventManager<DataNodeChangedEventArgs>.FireEvent(
			this,
			ChangedEventName,
			this,
			new DataNodeChangedEventArgs(this, changedFlags));
	}

	/// <summary>
	/// Resets the parent node reference and makes the current node the root node of all nodes and values below it
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <remarks>
	/// The synchronization object and the serializer remains the same to avoid synchronization issues,
	/// but a new data tree manager is created.
	/// </remarks>
	internal void BecomeRootNode()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mParent = null;
		UpdateDataTreeManagerRecursivelyUnsynced(new DataTreeManager(DataTreeManager, this));
		ReinitializePathsRecursivelyUnsynced();
	}

	/// <summary>
	/// Updates the data tree manager of all data nodes and data values recursively
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="dataTreeManager">The data tree manager to use.</param>
	internal void UpdateDataTreeManagerRecursivelyUnsynced(DataTreeManager dataTreeManager)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(ReferenceEquals(DataTreeManager.Sync, dataTreeManager.Sync), "The tree synchronization object should remain the same.");
		Debug.Assert(ReferenceEquals(DataTreeManager.Serializer, dataTreeManager.Serializer), "The tree serializer should remain the same.");

		DataTreeManager = dataTreeManager;
		Children.UpdateDataTreeManagerRecursivelyUnsynced(dataTreeManager);
		Values.UpdateDataTreeManagerRecursivelyUnsynced(dataTreeManager);
	}

	/// <summary>
	/// Cleans up dummy nodes and values from the current node up to the root node (for internal use only, not synchronized).<br/>
	/// This is done, if dummy nodes are not needed any more to keep up the data tree for data value references (<see cref="Data{T}"/>).
	/// </summary>
	internal void CleanupDummiesUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the current node is a regular node
		// => all nodes up to the root are regular as well...
		if (!IsDummyUnsynced)
			return;

		// abort if the current node has child nodes or values
		// => the current node must not be removed from the tree
		if (Children.CountUnsynced != 0 || Values.CountUnsynced != 0)
			return;

		// remove the current node from the parent node
		// and let the parent node cleanup itself...
		DataNode parent = mParent;
		if (parent == null) return;
		parent.Children.RemoveUnsynced(this, true);
		parent.CleanupDummiesUnsynced();
	}

	/// <summary>
	/// Reinitializes the path recursively (for internal use only, not synchronized).
	/// </summary>
	internal void ReinitializePathsRecursivelyUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mPath = mParent != null
			        ? PathHelpers.AppendNameToPath(mParent.PathUnsynced, mName.AsSpan())
			        : PathHelpers.RootPath;

		// raise event
		if (EventManager<DataNodeCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
		{
			EventManager<DataNodeChangedEventArgs>.FireEvent(
				this,
				ChangedEventName,
				this,
				new DataNodeChangedEventArgs(this, DataNodeChangedFlags.Path));
		}

		// proceed changing the path of child nodes and data values...
		Children.ReinitializePathsRecursivelyUnsynced();
		Values.ReinitializePathsRecursivelyUnsynced();
	}

	#endregion
}
