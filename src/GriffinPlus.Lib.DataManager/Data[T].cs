///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Threading;

using GriffinPlus.Lib.Events;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// A reference to a data value at a certain path in the data tree.
/// It tracks the data value in the data tree and provides notifications you can register for to stay informed about changes.
/// </summary>
/// <typeparam name="T">
/// Type of the referenced value (corresponds to the generic type argument of <see cref="DataValue{T}"/>).
/// </typeparam>
public sealed class Data<T> : IUntypedDataInternal, IDisposable
{
	#region Changed / ChangedAsync / UntypedChanged / UntypedChangedAsync

	private const string ChangedEventName        = "Changed";
	private const string UntypedChangedEventName = "UntypedChanged";

	/// <inheritdoc cref="IUntypedData.UntypedChanged"/>
	public event EventHandler<DataChangedEventArgs<T>> Changed
	{
		add
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed)
					throw new ObjectDisposedException(null);

				EventManager<DataChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					SynchronizationContext.Current ?? RootNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataChangedEventArgs<T>(this, DataChangedFlags.All | DataChangedFlags.InitialUpdate));
			}
		}

		remove
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				EventManager<DataChangedEventArgs<T>>.UnregisterEventHandler(this, ChangedEventName, value);
			}
		}
	}

	/// <inheritdoc cref="IUntypedData.UntypedChangedAsync"/>
	public event EventHandler<DataChangedEventArgs<T>> ChangedAsync
	{
		add
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed)
					throw new ObjectDisposedException(null);

				EventManager<DataChangedEventArgs<T>>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					RootNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataChangedEventArgs<T>(this, DataChangedFlags.All | DataChangedFlags.InitialUpdate));
			}
		}

		remove
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				EventManager<DataChangedEventArgs<T>>.UnregisterEventHandler(this, ChangedEventName, value);
			}
		}
	}

	/// <inheritdoc/>
	public event EventHandler<UntypedDataChangedEventArgs> UntypedChanged
	{
		add
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed)
					throw new ObjectDisposedException(null);

				EventManager<UntypedDataChangedEventArgs>.RegisterEventHandler(
					this,
					UntypedChangedEventName,
					value,
					SynchronizationContext.Current ?? RootNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedDataChangedEventArgs(this, DataChangedFlags.All | DataChangedFlags.InitialUpdate));
			}
		}

		remove
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				EventManager<UntypedDataChangedEventArgs>.UnregisterEventHandler(this, UntypedChangedEventName, value);
			}
		}
	}

	/// <inheritdoc/>
	public event EventHandler<UntypedDataChangedEventArgs> UntypedChangedAsync
	{
		add
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed)
					throw new ObjectDisposedException(null);

				EventManager<UntypedDataChangedEventArgs>.RegisterEventHandler(
					this,
					UntypedChangedEventName,
					value,
					RootNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new UntypedDataChangedEventArgs(this, DataChangedFlags.All | DataChangedFlags.InitialUpdate));
			}
		}

		remove
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				EventManager<UntypedDataChangedEventArgs>.UnregisterEventHandler(this, UntypedChangedEventName, value);
			}
		}
	}

	/// <summary>
	/// Event handler updating the data value reference as soon as the referenced data value changes.
	/// </summary>
	/// <param name="dataValue">The referenced data value.</param>
	/// <param name="changedFlags">Changed flags indicating what has changed.</param>
	private void EH_ChangedInternal(DataValue<T> dataValue, DataValueChangedFlagsInternal changedFlags)
	{
		Debug.Assert(!mDisposed, "Disposed data value references should not get notified any more.");
		Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(ReferenceEquals(mDataValue, dataValue), "The notifying data value should be the same as the referenced data value.");

		// update mirrored data
		mTimestamp = mDataValue.TimestampUnsynced;
		mProperties = mDataValue.PropertiesUnsynced;
		mValue = mDataValue.ValueUnsynced;

		// raise the 'Changed' event and 'ChangedAsync' event, if necessary
		if (EventManager<DataChangedEventArgs<T>>.IsHandlerRegistered(this, ChangedEventName))
		{
			EventManager<DataChangedEventArgs<T>>.FireEvent(
				this,
				ChangedEventName,
				this,
				new DataChangedEventArgs<T>(
					this,
					(DataChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllUserFlags)));
		}

		// raise the 'UntypedChanged' event and 'UntypedChangedAsync' event, if necessary
		if (EventManager<UntypedDataChangedEventArgs>.IsHandlerRegistered(this, UntypedChangedEventName))
		{
			EventManager<UntypedDataChangedEventArgs>.FireEvent(
				this,
				UntypedChangedEventName,
				this,
				new UntypedDataChangedEventArgs(
					this,
					(DataChangedFlags)(changedFlags & DataValueChangedFlagsInternal.AllUserFlags)));
		}
	}

	#endregion

	#region Construction and Disposal

	private bool mDisposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="Data{T}"/> class.
	/// </summary>
	/// <param name="dataValue">Data value to reference.</param>
	internal Data(DataValue<T> dataValue)
	{
		Debug.Assert(Monitor.IsEntered(dataValue.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(dataValue != null, "The data value is not specified.");
		Debug.Assert(dataValue.ParentNodeUnsynced != null, "The data value to reference has been detached from the data tree.");

		Name = dataValue.NameUnsynced;         // immutable
		Path = dataValue.PathUnsynced;         // immutable
		RootNode = dataValue.RootNodeUnsynced; // immutable

		mDataValue = dataValue;
		mTimestamp = dataValue.TimestampUnsynced;
		mProperties = dataValue.PropertiesUnsynced;
		mValue = dataValue.ValueUnsynced;

		// register for change notifications from the data value
		mDataValue.ChangedInternal += EH_ChangedInternal;

		// register the Data<T> object with the data tree manager that keeps track of the Data<T> object
		RootNode.DataTreeManager.RegisterDataValueReferenceUnsynced(this);
	}

	/// <summary>
	/// Disposes the current data value reference effectively cutting the link to the referenced data value.
	/// </summary>
	public void Dispose()
	{
		lock (RootNode.DataTreeManager.Sync)
		{
			// abort if the object is already disposed
			if (mDisposed) return;

			// unregister direct 'Changed' event from referenced data value
			// (might not have been registered, but trying to unregister them does no harm)
			if (mDataValue != null) mDataValue.ChangedInternal -= EH_ChangedInternal;

			// unlink from referenced data value
			RootNode.DataTreeManager.UnregisterDataValueReferenceUnsynced(this);
			mDataValue = null;

			// mark object as disposed to avoid trying to tear down the reference multiple times
			mDisposed = true;
		}
	}

	#endregion

	#region DataValue

	private DataValue<T> mDataValue;

	/// <summary>
	/// Gets the referenced data value (<c>null</c> if the link between the data value reference and its data tree is broken).<br/>
	/// For internal use only, not synchronized.
	/// </summary>
	internal DataValue<T> DataValueUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mDataValue;
		}
	}

	/// <inheritdoc/>
	IUntypedDataValueInternal IUntypedDataInternal.DataValueUnsynced => DataValueUnsynced;

	#endregion

	#region RootNode

	/// <summary>
	/// Gets the root node of the tree the referenced data value resides in.
	/// </summary>
	public DataNode RootNode { get; }

	#endregion

	#region Path

	/// <inheritdoc/>
	public string Path { get; }

	#endregion

	#region Name

	/// <inheritdoc/>
	public string Name { get; }

	#endregion

	#region Type

	/// <inheritdoc/>
	public Type Type => typeof(T);

	#endregion

	#region Timestamp

	private DateTime mTimestamp;

	/// <inheritdoc/>
	public DateTime Timestamp
	{
		get
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);

				// ensure the data value is regular
				// (dummy values are considered non-existing)
				if ((mProperties & DataValuePropertiesInternal.Dummy) != 0)
					throw new DataValueDoesNotExistException($"Referenced data value (path: {Path}) does not exist, yet.");

				return mTimestamp;
			}
		}
	}

	/// <inheritdoc/>
	DateTime IUntypedDataInternal.TimestampUnsynced => TimestampUnsynced;

	/// <inheritdoc cref="IUntypedDataInternal.TimestampUnsynced"/>
	internal DateTime TimestampUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mTimestamp;
		}
	}

	#endregion

	#region Properties

	private DataValuePropertiesInternal mProperties;

	/// <inheritdoc/>
	public DataValueProperties Properties
	{
		get
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);

				return (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);
			}
		}
		set
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if ((value & ~DataValueProperties.All) != 0)
					throw new ArgumentException($"The specified properties (0x{value:X}) contain unsupported flags ((0x{value & ~DataValueProperties.All:X})).", nameof(value));

				if (mDisposed) throw new ObjectDisposedException(null);
				if (mDataValue == null) throw new DataValueReferenceBrokenException();

				// set properties in the data value
				// (fires the 'ChangedInternal' event that updates mProperties)
				Debug.Assert(mProperties == mDataValue.PropertiesUnsynced);
				mDataValue.PropertiesUnsynced = (mProperties & ~DataValuePropertiesInternal.UserProperties) |
				                                ((DataValuePropertiesInternal)value & DataValuePropertiesInternal.UserProperties);
			}
		}
	}

	/// <inheritdoc/>
	DataValuePropertiesInternal IUntypedDataInternal.PropertiesUnsynced => PropertiesUnsynced;

	/// <inheritdoc cref="IUntypedDataInternal.PropertiesUnsynced"/>
	internal DataValuePropertiesInternal PropertiesUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mProperties;
		}
	}

	#endregion

	#region IsPersistent

	/// <inheritdoc/>
	public bool IsPersistent
	{
		get
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				return (mProperties & DataValuePropertiesInternal.Persistent) != 0;
			}
		}
		set
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				if (mDataValue == null) throw new DataValueReferenceBrokenException();

				// set properties in the data value
				// (fires events that updates mProperties)
				Debug.Assert(mProperties == mDataValue.PropertiesUnsynced);
				mDataValue.PropertiesUnsynced = value
					                                ? mProperties | DataValuePropertiesInternal.Persistent
					                                : mProperties & ~DataValuePropertiesInternal.Persistent;
			}
		}
	}

	#endregion

	#region IsHealthy

	/// <summary>
	/// Gets a value indicating whether the data value reference is healthy,
	/// i.e. it is successfully bound to its data value and tracking its changes.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	public bool IsHealthy
	{
		get
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				return mDataValue != null;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the data value reference is healthy,
	/// i.e. it is successfully bound to its data value and tracking its changes (for internal use only).
	/// </summary>
	internal bool IsHealthyUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mDataValue != null;
		}
	}

	#endregion

	#region Value

	private T mValue; // stores the same internal instance as the referenced DataValue<T> instance

	/// <summary>
	/// Gets or sets the value of the referenced data value.
	/// </summary>
	/// <exception cref="DataValueDoesNotExistException">
	/// The referenced data value does not exist, yet (thrown by getter only).
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken (thrown by setter only).
	/// </exception>
	/// <exception cref="InvalidCastException">
	/// <paramref name="value"/> is not assignable to the actual value type.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// This property returns a copy of the object stored in the referenced data value. Since the latest value is
	/// returned, the returned object might differ from the 'new value' passed to a handler of the following events:<br/>
	/// - <see cref="Changed"/><br/>
	/// - <see cref="ChangedAsync"/><br/>
	/// - <see cref="UntypedChanged"/><br/>
	/// - <see cref="UntypedChangedAsync"/><br/>
	/// If the referenced data value does not exist and should be persistent, setting this property the first time
	/// makes the containing node and all nodes up to the root node persistent as well to ensure that the value is
	/// included when persisting.
	/// </remarks>
	public T Value
	{
		get
		{
			T result;

			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);

				// ensure the data value is regular
				// (dummy values are considered non-existing)
				if ((mProperties & DataValuePropertiesInternal.Dummy) != 0)
					throw new DataValueDoesNotExistException($"Referenced data value (path: {Path}) does not exist, yet.");

				// an object within the data manager is never passed out, so it cannot be modified
				// => take its reference out of the lock to make the locked section as short as possible
				result = mValue;
			}

			// return a copy of the internal value
			return RootNode.DataTreeManager.Serializer.CopySerializableValue(result);
		}

		set
		{
			// create a copy of the value that can be stored in the data manager
			T newValue = RootNode.DataTreeManager.Serializer.CopySerializableValue(value);

			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				if (mDataValue == null) throw new DataValueReferenceBrokenException();

				mDataValue.ValueUnsynced = newValue; // fires 'ChangedInternal' event that updates mValue
			}
		}
	}

	/// <inheritdoc/>
	object IUntypedData.Value
	{
		get => Value;
		set => Value = (T)value; // can throw InvalidCastException
	}

	/// <inheritdoc/>
	object IUntypedDataInternal.ValueUnsynced => ValueUnsynced;

	/// <summary>
	/// Gets the timestamp of the referenced data value, i.e. the date/time the value was set the last time
	/// (for internal use only, not synchronized).
	/// </summary>
	internal T ValueUnsynced
	{
		get
		{
			Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
			return mValue;
		}
	}

	#endregion

	#region HasValue

	/// <summary>
	/// Gets a value indicating whether <see cref="Value"/> provides a valid value.
	/// </summary>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	public bool HasValue
	{
		get
		{
			lock (RootNode.DataTreeManager.Sync)
			{
				if (mDisposed) throw new ObjectDisposedException(null);
				return (mProperties & DataValuePropertiesInternal.Dummy) == 0;
			}
		}
	}

	#endregion

	#region Setting Value and Value Properties In An Atomic Operation

	/// <summary>
	/// Sets the value and the data value properties of the referenced data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="properties">New data value properties.</param>
	/// <exception cref="ArgumentException">
	/// The specified property flags are not supported.
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// If <paramref name="properties"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it makes
	/// all nodes up to the root node persistent as well to ensure the referenced data value is included when
	/// persisting the data tree.
	/// </remarks>
	public void Set(T value, DataValueProperties properties)
	{
		Set(value, properties, DataValueProperties.All);
	}

	/// <summary>
	/// Sets the value and the data value properties of the referenced data value in an atomic operation.
	/// </summary>
	/// <param name="value">New value.</param>
	/// <param name="propertiesToSet">Data value property flags to set.</param>
	/// <param name="propertiesToClear">Data value property flags to clear.</param>
	/// <exception cref="ArgumentException">
	/// The specified property flags are not supported.
	/// </exception>
	/// <exception cref="DataValueReferenceBrokenException">
	/// The link between the data value reference and its data value is broken.
	/// </exception>
	/// <exception cref="ObjectDisposedException">
	/// The object has been disposed.
	/// </exception>
	/// <exception cref="SerializationException">
	/// Serializing the specified <paramref name="value"/> failed.
	/// </exception>
	/// <remarks>
	/// If <paramref name="propertiesToSet"/> contains the <see cref="DataValueProperties.Persistent"/> flag, it
	/// makes all nodes up to the root node persistent as well to ensure the referenced data value is included when
	/// persisting the data tree.<br/>
	/// If <paramref name="propertiesToSet"/> and <paramref name="propertiesToClear"/> specify the same flag, the flag is
	/// effectively set.
	/// </remarks>
	public void Set(T value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		// ensure that only valid property flags are specified
		if ((propertiesToSet & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{propertiesToSet:X}) contain unsupported flags ((0x{propertiesToSet & ~DataValueProperties.All:X})).", nameof(propertiesToSet));
		if ((propertiesToClear & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{propertiesToClear:X}) contain unsupported flags ((0x{propertiesToClear & ~DataValueProperties.All:X})).", nameof(propertiesToClear));

		// create a copy of the value to set
		T newValue = RootNode.DataTreeManager.Serializer.CopySerializableValue(value);

		lock (RootNode.DataTreeManager.Sync)
		{
			if (mDisposed) throw new ObjectDisposedException(null);
			if (mDataValue == null) throw new DataValueReferenceBrokenException();

			// set value and user properties, but preserve administrative flags
			// (fires 'ChangedInternal' event that updates mTimestamp, mProperties and mValue)
			mDataValue.SetUnsynced(
				newValue,
				(DataValuePropertiesInternal)propertiesToSet,
				(DataValuePropertiesInternal)propertiesToClear);
		}
	}

	/// <inheritdoc/>
	void IUntypedData.Set(object value, DataValueProperties properties)
	{
		Set((T)value, properties); // can throw InvalidCastException
	}

	/// <inheritdoc/>
	void IUntypedData.Set(object value, DataValueProperties propertiesToSet, DataValueProperties propertiesToClear)
	{
		Set((T)value, propertiesToSet, propertiesToClear); // can throw InvalidCastException
	}

	#endregion

	#region Implementation of IDataInternal

	private bool mSuppressedNotHealthyNotification;

	/// <inheritdoc/>
	IUntypedDataValueInternal IUntypedDataInternal.UpdateDataValueReferenceUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		DataValue<T> oldDataValue = mDataValue;
		DataValue<T> newDataValue;
		try
		{
			// get new dummy data value at the monitored path, if necessary
			DataValuePropertiesInternal properties = mProperties | DataValuePropertiesInternal.Dummy;
			var pathParser = PathParser.Create(Path.AsSpan(), false);
			newDataValue = RootNode.GetDataValueInternalUnsynced<T>(ref pathParser, properties, default);
		}
		catch (DataTypeMismatchException)
		{
			// a data value exists, but the type differs!

			// notify clients, if the data value reference was invalidated before,
			// but no event was raised...
			if (mSuppressedNotHealthyNotification)
			{
				Debug.Assert(mDataValue == null);
				RaiseChangedEvents(DataChangedFlags.IsHealthy);
				mSuppressedNotHealthyNotification = false;
			}

			// invalidate current data value reference and notify registered clients about it...
			if (mDataValue == null) return null;
			mDataValue.ChangedInternal -= EH_ChangedInternal;
			mDataValue = null;
			RaiseChangedEvents(DataChangedFlags.IsHealthy);
			return oldDataValue;
		}

		// abort, if the data value did not change...
		if (ReferenceEquals(newDataValue, mDataValue))
			return mDataValue;

		// unregister 'ChangedInternal' event from the old data value
		if (mDataValue != null) mDataValue.ChangedInternal -= EH_ChangedInternal;

		// bind the new data value to the reference
		mDataValue = newDataValue;
		mDataValue.ChangedInternal += EH_ChangedInternal;
		mSuppressedNotHealthyNotification = false;

		// notify registered clients of the change
		RaiseChangedEvents(DataChangedFlags.All | DataChangedFlags.InitialUpdate);

		return oldDataValue;
	}

	/// <inheritdoc/>
	IUntypedDataValueInternal IUntypedDataInternal.InvalidateDataValueReferenceUnsynced(bool suppressChangedEvent)
	{
		Debug.Assert(Monitor.IsEntered(RootNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the reference is not bound to a data value
		if (mDataValue == null)
			return null;

		// unregister 'ChangedInternal' event from referenced data value
		mDataValue.ChangedInternal -= EH_ChangedInternal;

		// notify registered clients of the change, if appropriate
		mSuppressedNotHealthyNotification = suppressChangedEvent;
		if (!suppressChangedEvent) RaiseChangedEvents(DataChangedFlags.IsHealthy);

		// return the previously referenced data value
		IUntypedDataValueInternal oldDataValue = mDataValue;
		mDataValue = null;
		return oldDataValue;
	}

	/// <summary>
	/// Raises the <see cref="Changed"/> event and the <see cref="UntypedChanged"/> event
	/// notifying of the specified changes.
	/// </summary>
	/// <param name="changedFlags">Flags indicating the changes.</param>
	private void RaiseChangedEvents(DataChangedFlags changedFlags)
	{
		// raise 'Changed' event and 'ChangedAsync' event
		if (EventManager<DataChangedEventArgs<T>>.IsHandlerRegistered(this, ChangedEventName))
		{
			EventManager<DataChangedEventArgs<T>>.FireEvent(
				this,
				ChangedEventName,
				mDataValue,
				new DataChangedEventArgs<T>(this, changedFlags));
		}

		// raise 'UntypedChanged' event and 'UntypedChangedAsync' event
		if (EventManager<UntypedDataChangedEventArgs>.IsHandlerRegistered(this, UntypedChangedEventName))
		{
			EventManager<UntypedDataChangedEventArgs>.FireEvent(
				this,
				UntypedChangedEventName,
				mDataValue,
				new UntypedDataChangedEventArgs(this, changedFlags));
		}
	}

	#endregion
}
