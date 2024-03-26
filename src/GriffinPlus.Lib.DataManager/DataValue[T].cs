///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;
using GriffinPlus.Lib.Events;
using GriffinPlus.Lib.Logging;
using GriffinPlus.Lib.Serialization;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A data value associated with a data node.
/// </summary>
[InternalObjectSerializer(1)]
[DebuggerDisplay("Name: {" + nameof(Name) + "}, Type: {" + nameof(Type) + ".FullName}, Properties: {" + nameof(Properties) + "}, Timestamp: {" + nameof(Timestamp) + "}, Value: {" + nameof(Value) + "}, Path: {" + nameof(Path) + "}")]
public partial class DataValue<T> : IUntypedDataValueInternal, IInternalObjectSerializer
{
	#region Class Variables

	private static readonly LogWriter sLog = LogWriter.Get<DataValue<T>>();

	#endregion

	#region UntypedChanged / UntypedChangedAsync

	internal const string UntypedChangedEventName = "UntypedChanged";

	/// <inheritdoc/>
	public event EventHandler<UntypedDataValueChangedEventArgs> UntypedChanged
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event of the data value
			{
				EventManager<UntypedDataValueChangedEventArgs>.RegisterEventHandler(
					this,
					UntypedChangedEventName,
					value,
					SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedDataValueChangedEventArgs(this, DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<UntypedDataValueChangedEventArgs>.UnregisterEventHandler(
			this,
			UntypedChangedEventName,
			value);
	}

	/// <inheritdoc/>
	public event EventHandler<UntypedDataValueChangedEventArgs> UntypedChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event of the data value
			{
				EventManager<UntypedDataValueChangedEventArgs>.RegisterEventHandler(
					this,
					UntypedChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedDataValueChangedEventArgs(this, DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<UntypedDataValueChangedEventArgs>.UnregisterEventHandler(
			this,
			UntypedChangedEventName,
			value);
	}

	#endregion

	#region Changed / ChangedAsync

	internal const string ChangedEventName = "Changed";

	/// <inheritdoc cref="UntypedChanged"/>
	public event EventHandler<DataValueChangedEventArgs<T>> Changed
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event of the data value
			{
				EventManager<DataValueChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					SynchronizationContext.Current ?? DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataValueChangedEventArgs<T>(this, DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<DataValueChangedEventArgs<T>>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	/// <inheritdoc cref="IUntypedDataValue.UntypedChangedAsync"/>
	public event EventHandler<DataValueChangedEventArgs<T>> ChangedAsync
	{
		add
		{
			lock (DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event of the data value
			{
				EventManager<DataValueChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataValueChangedEventArgs<T>(this, DataValueChangedFlags.All | DataValueChangedFlags.InitialUpdate));
			}
		}

		remove => EventManager<DataValueChangedEventArgs<T>>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	#endregion

	#region ChangedInternal

	internal const string ChangedInternalEventName = "ChangedInternal";

	/// <summary>
	/// Occurs when something in the current data value changes (for internal use only).<br/>
	/// Handlers are always invoked synchronously from within the locked data tree.
	/// </summary>
	internal event Action<DataValue<T>, DataValueChangedFlagsInternal> ChangedInternal
	{
		add
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

			GenericWeakEventManager<DataValue<T>, DataValueChangedFlagsInternal>.RegisterEventHandler(
				this,
				ChangedInternalEventName,
				value,
				null,
				false);
		}

		remove => GenericWeakEventManager<DataValue<T>, DataValueChangedFlagsInternal>.UnregisterEventHandler(
			this,
			ChangedInternalEventName,
			value);
	}

	#endregion

	#region Raising Events

	/// <summary>
	/// Raises the <see cref="Changed"/> event, the <see cref="ChangedInternal"/> event and the <see cref="ChangedAsync"/> event.
	/// </summary>
	/// <param name="changedFlags">Flags indicating what parts of the data value have changed.</param>
	private void RaiseChangedEvents(DataValueChangedFlagsInternal changedFlags)
	{
		// raise 'ChangedInternal' event
		if (GenericWeakEventManager<DataValue<T>, DataValueChangedFlagsInternal>.IsHandlerRegistered(this, ChangedInternalEventName))
		{
			GenericWeakEventManager<DataValue<T>, DataValueChangedFlagsInternal>.FireEvent(
				this,
				ChangedInternalEventName,
				this,
				changedFlags);
		}

		// raise 'Changed' event and 'ChangedAsync' event
		if (EventManager<DataValueChangedEventArgs<T>>.IsHandlerRegistered(this, ChangedEventName))
		{
			EventManager<DataValueChangedEventArgs<T>>.FireEvent(
				this,
				ChangedEventName,
				this,
				new DataValueChangedEventArgs<T>(
					this,
					(DataValueChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllUserFlags)));
		}

		// raise 'UntypedChanged' event and 'UntypedChangedAsync' event
		if (EventManager<UntypedDataValueChangedEventArgs>.IsHandlerRegistered(this, UntypedChangedEventName))
		{
			EventManager<UntypedDataValueChangedEventArgs>.FireEvent(
				this,
				UntypedChangedEventName,
				this,
				new UntypedDataValueChangedEventArgs(
					this,
					(DataValueChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllUserFlags)));
		}

		// raise 'ViewerChanged' event and 'ViewerChangedAsync' event
		if (EventManager<ViewerDataValueChangedEventArgs<T>>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataValueChangedEventArgs<T>>.FireEvent(
				this,
				ViewerChangedEventName,
				ViewerWrapper,
				new ViewerDataValueChangedEventArgs<T>(
					this,
					(ViewerDataValueChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllViewerFlags)));
		}

		// raise 'ViewerUntypedChanged' event and 'ViewerUntypedChangedAsync' event
		// ReSharper disable once InvertIf
		if (EventManager<UntypedViewerDataValueChangedEventArgs>.IsHandlerRegistered(this, ViewerUntypedChangedEventName))
		{
			EventManager<UntypedViewerDataValueChangedEventArgs>.FireEvent(
				this,
				ViewerUntypedChangedEventName,
				ViewerWrapper,
				new UntypedViewerDataValueChangedEventArgs(
					this,
					(ViewerDataValueChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllViewerFlags)));
		}
	}

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValue{T}"/> class.
	/// </summary>
	/// <param name="parentNode"><see cref="DataNode"/> the value belongs to.</param>
	/// <param name="name">Name of the data value</param>
	/// <param name="properties">Properties of the data value.</param>
	/// <param name="value">The value of the data value.</param>
	/// <param name="timestamp">UTC-Timestamp of the data value (<c>null</c>> to use <see cref="DateTime.UtcNow"/>).</param>
	internal DataValue(
		DataNode                    parentNode,
		ReadOnlySpan<char>          name,
		DataValuePropertiesInternal properties,
		T                           value,
		DateTime?                   timestamp = null)
	{
		// tree sync should be locked here...
		Debug.Assert(parentNode != null, "The parent node is not specified.");
		Debug.Assert(parentNode.DataTreeManager != null, "The data tree manager is not set on the parent node.");
		Debug.Assert(Monitor.IsEntered(parentNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		mParentNode = parentNode;
		mName = name.ToString();
		mPath = PathHelpers.AppendNameToPath(mParentNode.PathUnsynced, name);
		mTimestamp = timestamp ?? DateTime.UtcNow;
		mProperties = properties;
		mValue = value;

		DataTreeManager = mParentNode.DataTreeManager;
	}

	#endregion

	#region Serialization using Griffin+ Serializer

	/// <summary>
	/// Initializes a new data value object from a serializer archive (for serialization purposes only).
	/// </summary>
	/// <param name="archive">Serializer archive to initialize a new data value object from.</param>
	/// <exception cref="VersionNotSupportedException">Archive was created using a serializer of a greater version than supported.</exception>
	internal DataValue(DeserializationArchive archive)
	{
		if (archive.Version == 1)
		{
			// archive should contain the node the value belongs to...
			if (archive.Context is not DataDeserializationContext context)
				throw new SerializationException($"The serialization archive does not contain a {nameof(DataDeserializationContext)}.");

			mParentNode = context.ParentNode;
			DataTreeManager = mParentNode.DataTreeManager;

			// read data value
			mName = archive.ReadString();
			mTimestamp = archive.ReadDateTime();
			mProperties = archive.ReadEnum<DataValuePropertiesInternal>();
			mValue = (T)archive.ReadObject();

			// set path appropriately
			mPath = PathHelpers.AppendNameToPath(mParentNode.PathUnsynced, Name.AsSpan());
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
			if (archive.Version == 1)
			{
				archive.Write(mName);
				archive.Write(mTimestamp);
				archive.Write(mProperties);
				archive.Write(mValue);
			}
			else
			{
				throw new VersionNotSupportedException(archive);
			}
		}
	}

	#endregion

	#region DataTreeManager

	/// <inheritdoc/>
	public DataTreeManager DataTreeManager { get; private set; }

	#endregion

	#region Name

	private readonly string mName;

	/// <inheritdoc/>
	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	public string Name => mName;

	/// <summary>
	/// Gets the name of the data value (for internal use only, not synchronized)
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWhenPossible
	internal string NameUnsynced => mName;

	/// <summary>
	/// Gets the name of the data value (for internal use only, not synchronized)
	/// </summary>
	// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
	string IUntypedDataValueInternal.NameUnsynced => mName;

	#endregion

	#region Type

	/// <inheritdoc/>
	public Type Type => typeof(T);

	#endregion

	#region Value

	private T mValue;

	/// <summary>
	/// Gets or sets the value of the data value.
	/// </summary>
	/// <exception cref="DataValueDoesNotExistException">
	/// The data value is a dummy data value, i.e. it does not exist actually (thrown by getter only).
	/// </exception>
	/// <exception cref="SerializationException">Serializing the specified <paramref name="value"/> failed.</exception>
	/// <remarks>
	/// This property returns a copy of the object stored in the data value. Since the latest value is returned,
	/// the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="Changed"/><br/>
	/// - <see cref="ChangedAsync"/><br/>
	/// - <see cref="UntypedChanged"/><br/>
	/// - <see cref="UntypedChangedAsync"/><br/>
	/// If the data value is dummy and persistent, setting this property the first time makes the containing node
	/// and its parent nodes up to the root node persistent as well to ensure that the value is included when
	/// persisting.
	/// </remarks>
	public T Value
	{
		get
		{
			T result;

			lock (DataTreeManager.Sync)
			{
				// ensure the data value is regular
				if (IsDummyUnsynced)
					throw new DataValueDoesNotExistException($"Referenced data value (path: {mPath}) does not exist.");

				// an object within the data manger is never passed out, so it cannot be modified
				// => take its reference out of the lock to make the locked section as short as possible
				result = mValue;
			}

			// return a copy of the internal value
			return DataTreeManager.Serializer.CopySerializableValue(result);
		}

		set
		{
			// create a copy of the value that can be stored in the data manager
			T newValue = DataTreeManager.Serializer.CopySerializableValue(value);

			lock (DataTreeManager.Sync)
			{
				ValueUnsynced = newValue;
			}
		}
	}

	/// <inheritdoc/>
	object IUntypedDataValue.Value
	{
		get => Value;
		set => Value = (T)value;
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.ValueUnsynced"/>
	internal T ValueUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mValue;
		}
		set
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

			// remove dummy flag, if necessary
			// -------------------------------------------------------------------------------------------------
			bool wasDummyValue = false;
			if (IsDummyUnsynced)
			{
				// regularize parent nodes and make them persistent,
				// if the current value is persistent
				if (mParentNode != null)
				{
					mParentNode.RegularizeUnsynced();
					if ((mProperties & DataValuePropertiesInternal.Persistent) != 0)
					{
						mParentNode.MakePersistentUnsynced();
					}
				}

				// remove the dummy flag
				mProperties &= ~DataValuePropertiesInternal.Dummy;
				wasDummyValue = true;
			}

			// set value and timestamp
			// -------------------------------------------------------------------------------------------------
			T oldValue = mValue;
			mValue = value;
			mTimestamp = DateTime.UtcNow;

			// raise events
			// --------------------------------------------------------------------------------------------------------------------
			RaiseChangedEvents(
				DataValueChangedFlagsInternal.Timestamp |
				(wasDummyValue || !Equals(oldValue, mValue) ? DataValueChangedFlagsInternal.Value : DataValueChangedFlagsInternal.None) |
				(wasDummyValue ? DataValueChangedFlagsInternal.IsDummy : 0));
		}
	}

	/// <inheritdoc/>
	object IUntypedDataValueInternal.ValueUnsynced => ValueUnsynced;

	#endregion

	#region Timestamp

	private DateTime mTimestamp;

	/// <inheritdoc/>
	public DateTime Timestamp
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return mTimestamp;
			}
		}
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.TimestampUnsynced"/>
	internal DateTime TimestampUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mTimestamp;
		}
		set
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			mTimestamp = value;
		}
	}

	/// <inheritdoc/>
	DateTime IUntypedDataValueInternal.TimestampUnsynced => TimestampUnsynced;

	#endregion

	#region Properties

	private DataValuePropertiesInternal mProperties;

	/// <inheritdoc/>
	public DataValueProperties Properties
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return (DataValueProperties)(PropertiesUnsynced & DataValuePropertiesInternal.UserProperties);
			}
		}

		set
		{
			// ensure that only valid property flags are specified
			if ((value & ~DataValueProperties.All) != 0)
				throw new ArgumentException($"The specified properties (0x{value:X}) contain unsupported flags ((0x{value & ~DataValueProperties.All:X})).", nameof(value));

			lock (DataTreeManager.Sync)
			{
				// set user flags only and keep administrative flags
				PropertiesUnsynced = (mProperties & ~DataValuePropertiesInternal.UserProperties) | (DataValuePropertiesInternal)value;
			}
		}
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.PropertiesUnsynced"/>
	internal DataValuePropertiesInternal PropertiesUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mProperties;
		}

		set
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			Debug.Assert((value & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{value:X}) are not supported.");

			// regularize nodes up to the root node if the data value becomes regular
			if ((mProperties & DataValuePropertiesInternal.Dummy) != 0 && (value & DataValuePropertiesInternal.Dummy) == 0)
				mParentNode?.RegularizeUnsynced();

			// make all nodes on the path to this data value persistent as well, if the data value is regular
			// (for dummy values this is done as soon as the data value is regularized by setting its value later on)
			if ((value & DataValuePropertiesInternal.Dummy) == 0 && (value & DataValuePropertiesInternal.Persistent) != 0)
				mParentNode?.MakePersistentUnsynced();

			// abort if the properties did not change
			if (mProperties == value)
				return;

			// change properties
			DataValuePropertiesInternal oldProperties = mProperties;
			DataValuePropertiesInternal oldUserProperties = mProperties & DataValuePropertiesInternal.UserProperties;
			mProperties = value;
			DataValuePropertiesInternal newProperties = mProperties;
			DataValuePropertiesInternal newUserProperties = mProperties & DataValuePropertiesInternal.UserProperties;

			// determine flags reflecting the property changes
			var changedFlags = DataValueChangedFlagsInternal.None;
			// add changed flags for all properties (user flags + administrative flags)
			changedFlags |= (DataValueChangedFlagsInternal)((~oldProperties & newProperties) | (oldProperties & ~newProperties));
			// add changed flag for the 'Properties' property, if user properties have changed
			changedFlags |= ((~oldUserProperties & newUserProperties) | (oldUserProperties & ~newUserProperties)) != 0
				                ? DataValueChangedFlagsInternal.Properties
				                : DataValueChangedFlagsInternal.None;
			// add changed flag for the value if the data value has switched from dummy to regular or vice versa
			changedFlags |= (oldProperties & DataValuePropertiesInternal.Dummy) != (newProperties & DataValuePropertiesInternal.Dummy)
				                ? DataValueChangedFlagsInternal.Value
				                : DataValueChangedFlagsInternal.None;

			// raise events
			RaiseChangedEvents(changedFlags);
		}
	}

	/// <inheritdoc/>
	DataValuePropertiesInternal IUntypedDataValueInternal.PropertiesUnsynced
	{
		get => PropertiesUnsynced;
		set => PropertiesUnsynced = value;
	}

	#endregion

	#region IsPersistent

	/// <inheritdoc/>
	public bool IsPersistent
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return (mProperties & DataValuePropertiesInternal.Persistent) != 0;
			}
		}

		set
		{
			lock (DataTreeManager.Sync)
			{
				PropertiesUnsynced = value
					                     ? mProperties | DataValuePropertiesInternal.Persistent
					                     : mProperties & ~DataValuePropertiesInternal.Persistent;
			}
		}
	}

	#endregion

	#region IsDetached

	/// <inheritdoc/>
	public bool IsDetached
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return (mProperties & DataValuePropertiesInternal.Detached) != 0;
			}
		}
	}

	#endregion

	#region IsDummy

	/// <summary>
	/// Gets a value indicating whether the current data value is dummy (for internal use only, not synchronized).
	/// </summary>
	bool IUntypedDataValueInternal.IsDummyUnsynced => IsDummyUnsynced;

	/// <inheritdoc cref="IUntypedDataValueInternal.IsDummyUnsynced"/>
	internal bool IsDummyUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return (mProperties & DataValuePropertiesInternal.Dummy) != 0;
		}
	}

	#endregion

	#region Path

	private string mPath;

	/// <inheritdoc/>
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

	/// <inheritdoc cref="IUntypedDataValueInternal.PathUnsynced"/>
	internal string PathUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mPath;
		}
	}

	/// <inheritdoc/>
	string IUntypedDataValueInternal.PathUnsynced => PathUnsynced;

	#endregion

	#region ParentNode

	private DataNode mParentNode;

	/// <inheritdoc/>
	public DataNode ParentNode
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return mParentNode;
			}
		}
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.ParentNodeUnsynced"/>
	internal DataNode ParentNodeUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mParentNode;
		}
	}

	/// <inheritdoc/>
	DataNode IUntypedDataValueInternal.ParentNodeUnsynced => ParentNodeUnsynced;

	#endregion

	#region RootNode

	/// <inheritdoc/>
	public DataNode RootNode
	{
		get
		{
			lock (DataTreeManager.Sync)
			{
				return RootNodeUnsynced;
			}
		}
	}

	/// <inheritdoc cref="IUntypedDataValueInternal.RootNodeUnsynced"/>
	internal DataNode RootNodeUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
			if (mParentNode == null) return null;
			DataNode node = mParentNode;
			while (node.ParentUnsynced != null)
			{
				node = node.ParentUnsynced;
			}
			return node;
		}
	}

	/// <inheritdoc/>
	DataNode IUntypedDataValueInternal.RootNodeUnsynced => RootNodeUnsynced;

	#endregion

	#region Executing Multiple Operations Atomically

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <param name="action">Action to perform within the locked section.</param>
	public void ExecuteAtomically(DataValueAction<T> action)
	{
		lock (DataTreeManager.Sync)
		{
			action(this);
		}
	}

	/// <summary>
	/// Locks the entire data tree and executes the specified action within the lock.
	/// </summary>
	/// <typeparam name="TState">Some custom object passed to the action.</typeparam>
	/// <param name="action">Action to perform within the locked section.</param>
	/// <param name="state">Some state object passed to the action.</param>
	public void ExecuteAtomically<TState>(DataValueAction<T, TState> action, TState state)
	{
		lock (DataTreeManager.Sync)
		{
			action(this, state);
		}
	}

	/// <inheritdoc/>
	void IUntypedDataValue.ExecuteAtomically(UntypedDataValueAction action)
	{
		lock (DataTreeManager.Sync)
		{
			action(this);
		}
	}

	/// <inheritdoc/>
	void IUntypedDataValue.ExecuteAtomically<TState>(UntypedDataValueAction<TState> action, TState state)
	{
		lock (DataTreeManager.Sync)
		{
			action(this, state);
		}
	}

	#endregion

	#region Setting Value and Value Properties In An Atomic Operation

	/// <inheritdoc cref="IUntypedDataValue.Set(object, DataValueProperties)"/>
	public void Set(T value, DataValueProperties properties)
	{
		Set(value, properties, DataValueProperties.All);
	}

	/// <inheritdoc/>
	void IUntypedDataValue.Set(object value, DataValueProperties properties)
	{
		Set((T)value, properties); // can throw InvalidCastException
	}

	/// <inheritdoc cref="IUntypedDataValue.Set(object, DataValueProperties, DataValueProperties)"/>
	public void Set(T value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		// ensure that only valid property flags are specified
		if ((propertiesToSet & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{propertiesToSet:X}) contain unsupported flags ((0x{propertiesToSet & ~DataValueProperties.All:X})).", nameof(propertiesToSet));
		if ((propertiesToClear & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{propertiesToClear:X}) contain unsupported flags ((0x{propertiesToClear & ~DataValueProperties.All:X})).", nameof(propertiesToClear));

		// create a copy of the value to set
		T newValue = DataTreeManager.Serializer.CopySerializableValue(value);

		lock (DataTreeManager.Sync)
		{
			// set user properties, but preserve administrative flags
			DataValuePropertiesInternal internalPropertiesToSet = (DataValuePropertiesInternal)propertiesToSet | (mProperties & DataValuePropertiesInternal.AdministrativeProperties);
			DataValuePropertiesInternal internalPropertiesToClear = (DataValuePropertiesInternal)propertiesToClear | DataValuePropertiesInternal.AdministrativeProperties;
			SetUnsynced(newValue, internalPropertiesToSet, internalPropertiesToClear);
		}
	}

	/// <inheritdoc/>
	void IUntypedDataValue.Set(object value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		Set((T)value, propertiesToSet, propertiesToClear); // can throw InvalidCastException
	}

	/// <summary>
	/// Sets the value and the data value properties of the current data value in an atomic operation
	/// (for internal use only, not synchronized, does not copy the value, raises events).
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="propertiesToSet">Data value property flags to set.</param>
	/// <param name="propertiesToClear">Data value property flags to clear.</param>
	/// <remarks>
	/// This method supports setting and clearing administrative flags.<br/>
	/// If <paramref name="propertiesToSet"/> contains the <see cref="DataValuePropertiesInternal.Persistent"/> flag, it
	/// makes all nodes up to the root node persistent as well to ensure the current value is included when
	/// persisting the data tree.<br/>
	/// If <paramref name="propertiesToSet"/> and <paramref name="propertiesToClear"/> specify the same flag, the
	/// flag is effectively set.
	/// </remarks>
	internal void SetUnsynced(T value, DataValuePropertiesInternal propertiesToSet, DataValuePropertiesInternal propertiesToClear)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// determine the effective properties
		// (remove the 'Dummy' flag as the data value will become regular after setting its value - unless propertiesToSet specifies the flag)
		DataValuePropertiesInternal newProperties = (mProperties & ~DataValuePropertiesInternal.Dummy & ~propertiesToClear) | propertiesToSet;

		// regularize parent nodes and make them persistent, if necessary
		if ((newProperties & DataValuePropertiesInternal.Dummy) == 0)
		{
			mParentNode?.RegularizeUnsynced();
			if ((newProperties & DataValuePropertiesInternal.Persistent) != 0)
				mParentNode?.MakePersistentUnsynced();
		}

		// set value and timestamp, change properties and raise events, if necessary
		// ----------------------------------------------------------------------------------------------------------------
		T oldValue = mValue;
		mValue = (newProperties & DataValuePropertiesInternal.Dummy) != 0 ? default : value;
		mTimestamp = DateTime.UtcNow;
		DataValuePropertiesInternal oldProperties = mProperties;
		DataValuePropertiesInternal oldUserProperties = mProperties & DataValuePropertiesInternal.UserProperties;
		mProperties = newProperties;
		DataValuePropertiesInternal newUserProperties = mProperties & DataValuePropertiesInternal.UserProperties;
		var changedFlags = DataValueChangedFlagsInternal.Timestamp;
		// add changed flags for all properties (user flags + administrative flags)
		changedFlags |= (DataValueChangedFlagsInternal)((~oldProperties & newProperties) | (oldProperties & ~newProperties));
		// add changed flag for the 'Properties' property, if user properties have changed
		changedFlags |= ((~oldUserProperties & newUserProperties) | (oldUserProperties & ~newUserProperties)) != 0
			                ? DataValueChangedFlagsInternal.Properties
			                : DataValueChangedFlagsInternal.None;
		// add changed flag for the value if the data value has switched from dummy to regular or vice versa or if its value has changed
		changedFlags |= (oldProperties & DataValuePropertiesInternal.Dummy) != (newProperties & DataValuePropertiesInternal.Dummy) || !Equals(oldValue, mValue)
			                ? DataValueChangedFlagsInternal.Value
			                : DataValueChangedFlagsInternal.None;
		// raise events
		RaiseChangedEvents(changedFlags);
	}

	/// <inheritdoc/>
	void IUntypedDataValueInternal.SetUnsynced(object value, DataValuePropertiesInternal propertiesToSet, DataValuePropertiesInternal propertiesToClear)
	{
		SetUnsynced((T)value, propertiesToSet, propertiesToClear); // can throw InvalidCastException
	}

	#endregion

	#region Removing the Data Value

	/// <inheritdoc/>
	public void Remove()
	{
		lock (DataTreeManager.Sync)
		{
			mParentNode?.Values.Remove(this);
		}
	}

	/// <inheritdoc/>
	void IUntypedDataValueInternal.RemoveUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		mParentNode?.Values.RemoveUnsynced(this);
	}

	#endregion

	#region Other Internal Helper Methods

	/// <inheritdoc/>
	void IUntypedDataValueInternal.DetachFromDataTreeUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert((mProperties & DataValuePropertiesInternal.Detached) == 0, "The data value is already detached.");
		PropertiesUnsynced = mProperties | DataValuePropertiesInternal.Detached; // set flags and raise events
		mParentNode = null;
	}

	/// <inheritdoc/>
	void IUntypedDataValueInternal.UpdatePathUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// re-create the path from the path of the parent node and the name of the data value
		string path = PathHelpers.AppendNameToPath(mParentNode.PathUnsynced, Name.AsSpan());
		bool changed = mPath != path;
		mPath = path;

		// raise events
		if (changed) RaiseChangedEvents(DataValueChangedFlagsInternal.Path);
	}

	/// <inheritdoc/>
	void IUntypedDataValueInternal.UpdateDataTreeManagerUnsynced(DataTreeManager dataTreeManager)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		DataTreeManager = dataTreeManager;
	}

	/// <inheritdoc/>
	void IUntypedDataValueInternal.CopyValuesUnsynced(DataNode destinationNode)
	{
		Debug.Assert(Monitor.IsEntered(DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(Monitor.IsEntered(destinationNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the node is a dummy node
		if (IsDummyUnsynced)
			return;

		IUntypedDataValueInternal destinationDataValue = destinationNode.Values.FindUnsynced(mName.AsSpan());
		if (destinationDataValue != null)
		{
			// destination data value exists already
			Debug.Assert(destinationDataValue.IsDummyUnsynced);
			if (destinationDataValue.Type == typeof(T))
			{
				// type matches expected type
				// => set properties and value
				lock (destinationDataValue.DataTreeManager.Sync)
				{
					// share the value object with the copied data value
					// (it is immutable within the data manager and copied when passed out)
					destinationDataValue.SetUnsynced(
						mValue,
						mProperties & DataValuePropertiesInternal.UserProperties,
						DataValuePropertiesInternal.All);
				}
			}
			else
			{
				// type does not match the expected type
				// => use type of the data value to copy and log error...
				sLog.Write(
					LogLevel.Error,
					"Detected an inconsistent data value type during a subtree copy (destination path: {0}, type of existing data value: {1}, type of data value to copy: {2})." + Environment.NewLine +
					"The source data value was copied to the path of the destination data value. Any existing data value references become invalid and cannot be used any further!",
					destinationDataValue.PathUnsynced,
					destinationDataValue.Type.FullName,
					typeof(T).FullName);

				// remove all data value references from the data value to remove
				DataTreeManager.InvalidateReferencesForDataValueUnsynced(destinationDataValue.PathUnsynced, true);

				// remove the data value
				destinationDataValue.RemoveUnsynced();
				Debug.Assert(destinationDataValue.ParentNode == null);

				// add value
				destinationNode.Values.AddInternalUnsynced(
					mName.AsSpan(),
					mProperties & DataValuePropertiesInternal.UserProperties,
					mValue);
			}
		}
		else
		{
			// destination data value does not exist, yet
			destinationNode.Values.AddInternalUnsynced(
				mName.AsSpan(),
				mProperties & DataValuePropertiesInternal.UserProperties,
				mValue);
		}
	}

	#endregion
}
