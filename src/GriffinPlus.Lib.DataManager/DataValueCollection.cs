///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
/// A collection containing values in a data node.
/// </summary>
[InternalObjectSerializer(1)]
public partial class DataValueCollection :
	IEnumerable<IUntypedDataValue>,
	IInternalObjectSerializer
{
	private readonly DataNode                               mNode;   // node containing the collection
	private readonly List<IUntypedDataValueInternal>        mBuffer; // list storing the nodes
	private          IEnumerable<IUntypedDataValueInternal> RegularValuesOnly => mBuffer.Where(x => !x.IsDummyUnsynced);

	#region Changed / ChangedAsync

	internal const string ChangedEventName = "Changed";

	/// <summary>
	/// Occurs when a data value is added to or removed from the collection.<br/>
	/// The first invocation of the event handler notifies about the initial set of values in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataValueCollectionChangedEventArgs> Changed
	{
		add
		{
			lock (mNode.DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<DataValueCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					SynchronizationContext.Current ?? mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataValueCollectionChangedEventArgs(mNode, RegularValuesOnly));
			}
		}

		remove => EventManager<DataValueCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a data value is added to or removed from the collection.<br/>
	/// The first invocation of the event handler notifies about the initial set of values in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	public event EventHandler<DataValueCollectionChangedEventArgs> ChangedAsync
	{
		add
		{
			lock (mNode.DataTreeManager.Sync)
			{
				EventManager<DataValueCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ChangedEventName,
					value,
					mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new DataValueCollectionChangedEventArgs(mNode, RegularValuesOnly));
			}
		}

		remove => EventManager<DataValueCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ChangedEventName,
			value);
	}

	#endregion

	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataValueCollection"/> class.
	/// </summary>
	/// <param name="node">Data node the value collection resides in.</param>
	internal DataValueCollection(DataNode node)
	{
		mBuffer = [];
		mNode = node;
	}

	#endregion

	#region Serialization using Griffin+ Serializer

	/// <summary>
	/// Deserializes an instance of the <see cref="DataValueCollection"/> class from a serializer archive.
	/// </summary>
	/// <param name="archive">Serializer archive containing the serialized collection.</param>
	/// <exception cref="VersionNotSupportedException">Serializer version is not supported.</exception>
	internal DataValueCollection(DeserializationArchive archive)
	{
		if (archive.Version == 1)
		{
			// get the node the collection belongs to from the archive context
			if (archive.Context is not DataDeserializationContext context)
				throw new SerializationException($"The serialization archive does not contain a {nameof(DataDeserializationContext)}.");

			mNode = context.ParentNode;

			// read number of data values
			int count = archive.ReadInt32();

			// read data values
			mBuffer = new List<IUntypedDataValueInternal>(count);
			for (int i = 0; i < count; i++)
			{
				var dataValue = archive.ReadObject(archive.Context) as IUntypedDataValueInternal;
				mBuffer.Add(dataValue);
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
	/// Serializes the current object to a serializer archive.
	/// </summary>
	/// <param name="archive">Archive to serialize the current object to.</param>
	/// <exception cref="VersionNotSupportedException">Requested serializer version is not supported.</exception>
	void IInternalObjectSerializer.Serialize(SerializationArchive archive)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			if (archive.Version == 1)
			{
				// determine the number of values to serialize
				int count = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
				foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
				{
					// only regular persistent values should be serialized
					if ((dataValue.PropertiesUnsynced & DataValuePropertiesInternal.Persistent) != 0 &&
					    (dataValue.PropertiesUnsynced & DataValuePropertiesInternal.Dummy) == 0)
					{
						count++;
					}
				}

				// write number of values
				archive.Write(count);

				// write the values
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
				// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
				foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
				foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
				{
					// only regular persistent values should be serialized
					if ((dataValue.PropertiesUnsynced & DataValuePropertiesInternal.Persistent) != 0 &&
					    (dataValue.PropertiesUnsynced & DataValuePropertiesInternal.Dummy) == 0)
					{
						archive.Write(dataValue);
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
	/// Gets the number of values in the collection.
	/// </summary>
	public int Count
	{
		get
		{
			lock (mNode.DataTreeManager.Sync)
			{
				return RegularValuesOnly.Count();
			}
		}
	}

	/// <summary>
	/// Gets the number of data values in the collection
	/// (considers regular and dummy values, for internal use only, not synchronized).
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
	/// Gets the internal list of child data values (considers regular and dummy values).
	/// </summary>
	internal IReadOnlyList<IUntypedDataValueInternal> InternalBuffer => mBuffer;

	#endregion

	#region Add(...) / AddDynamically(...)

	/// <summary>
	/// The <see cref="Add{T}(string,DataValueProperties,T)"/> method.
	/// </summary>
	private static readonly MethodInfo sGenericAddMethod =
		typeof(DataValueCollection)
			.GetMethods()
			.Single(
				x => x.Name == nameof(Add) &&
				     x.IsGenericMethodDefinition &&
				     x.GetParameters().Length == 3 &&
				     x.GetParameters()[0].ParameterType == typeof(string) &&
				     x.GetParameters()[1].ParameterType == typeof(DataValueProperties) &&
				     x.GetParameters()[2].ParameterType.IsGenericParameter &&
				     x.GetParameters()[2].ParameterType.GenericParameterPosition == 0);

	/// <summary>
	/// Adds a data value to the collection
	/// (for cases in which the type of the value is known at compile time).
	/// </summary>
	/// <typeparam name="T">Type of the value.</typeparam>
	/// <param name="name">Name of the data value.</param>
	/// <param name="properties">Initial properties of the data value.</param>
	/// <param name="value">Initial value of the data value (can be of a type deriving from <typeparamref name="T"/>).</param>
	/// <returns>The added data value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for value names.
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataValueExistsAlreadyException">Data value with the specified name exists already.</exception>
	public DataValue<T> Add<T>(string name, DataValueProperties properties, T value)
	{
		// ensure that the specified name is valid
		if (name == null) throw new ArgumentNullException(nameof(name));
		if (!PathHelpers.IsValidName(name.AsSpan()))
			throw new ArgumentException($"The specified name ({name}) does not comply with the rules for node names.", nameof(name));

		// ensure that the data value properties are valid
		if ((properties & ~DataValueProperties.All) != 0)
			throw new ArgumentException($"The specified properties (0x{properties:X}) contain unsupported flags ((0x{properties & ~DataValueProperties.All:X})).", nameof(properties));

		// copy the initial value of the data value to add to keep the spent time in the lock below as short as possible
		T copy = Serializer.CopySerializableObject(value);

		lock (mNode.DataTreeManager.Sync)
		{
			// check whether a data value with the specified name is already in the collection
			IUntypedDataValueInternal dataValue = FindUnsynced(name);

			// add a new data value, if the collection does not contain a data value with the specified name
			if (dataValue == null)
				return AddInternalUnsynced(name.AsSpan(), (DataValuePropertiesInternal)properties, copy);

			// data value exists

			// abort if the data value is regular
			if (!dataValue.IsDummyUnsynced)
				throw new DataValueExistsAlreadyException($"A value with the specified name '{name}' does already exist in the collection.");

			// data value is dummy

			if (dataValue.Type != typeof(T))
			{
				// type of the data value differs from the requested type
				// => invalidate data value references (the data value will be removed automatically) and
				//    add a new data value with the requested type
				mNode.DataTreeManager.UnregisterDataValueReferencesUnsynced(dataValue, true);
				Debug.Assert(!mBuffer.Any(other => ReferenceEquals(dataValue, other)));
				Debug.Assert((dataValue.PropertiesUnsynced & DataValuePropertiesInternal.Detached) != 0);
				return AddInternalUnsynced(name.AsSpan(), (DataValuePropertiesInternal)properties, copy);
			}

			// data value is dummy and of the expected type

			// set value and properties of the data value
			// (regularizes the data value automatically)
			dataValue.SetUnsynced(
				copy,
				(DataValuePropertiesInternal)properties,
				DataValuePropertiesInternal.All);

			return (DataValue<T>)dataValue;
		}
	}

	/// <summary>
	/// Adds a data value to the collection
	/// (for cases in which the type of the value is not known at compile time).
	/// </summary>
	/// <param name="type">Type of the value.</param>
	/// <param name="name">Name of the data value.</param>
	/// <param name="properties">Initial properties of the data value.</param>
	/// <param name="value">Initial value of the data value (can be of a type deriving from <paramref name="type"/>).</param>
	/// <returns>The added data value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for value names.
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataValueExistsAlreadyException">Data value with the specified name exists already.</exception>
	public IUntypedDataValue AddDynamically(
		Type                type,
		string              name,
		DataValueProperties properties,
		object              value)
	{
		return AddDynamicallyInternal(type, name, properties, value);
	}

	/// <summary>
	/// Adds a data value with the specified type to the collection (for internal use only, synchronized)
	/// </summary>
	/// <param name="type">Type of the value.</param>
	/// <param name="name">Name of the data value.</param>
	/// <param name="properties">Initial properties of the data value.</param>
	/// <param name="value">Initial value of the data value (can be of a type deriving from <paramref name="type"/>).</param>
	/// <returns>The added data value.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">
	/// The specified <paramref name="name"/> does not comply with the rules for value names.
	/// -or-<br/>
	/// The specified <paramref name="properties"/> contain unsupported flags.
	/// </exception>
	/// <exception cref="DataValueExistsAlreadyException">Data value with the specified name exists already.</exception>
	internal IUntypedDataValueInternal AddDynamicallyInternal(
		Type                type,
		string              name,
		DataValueProperties properties,
		object              value)
	{
		MethodInfo generic = sGenericAddMethod.MakeGenericMethod(type);
		try
		{
			return (IUntypedDataValueInternal)generic.Invoke(this, [name, properties, value]);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException!;
		}
	}

	/// <summary>
	/// Adds a data value with the specified name to the collection and raises the appropriate events
	/// (does not check for duplicate names, for internal use only, not synchronized).
	/// </summary>
	/// <typeparam name="T">Type of the value.</typeparam>
	/// <param name="name">Name of the data value.</param>
	/// <param name="properties">Initial properties of the data value.</param>
	/// <param name="value">Initial value.</param>
	/// <returns>The added data value.</returns>
	internal DataValue<T> AddInternalUnsynced<T>(
		ReadOnlySpan<char>          name,
		DataValuePropertiesInternal properties,
		T                           value)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// make nodes regular and persistent up to the root node, if necessary...
		if ((properties & DataValuePropertiesInternal.Dummy) == 0)
		{
			mNode.Regularize();
			if ((properties & DataValuePropertiesInternal.Persistent) != 0)
			{
				mNode.MakePersistent();
			}
		}

		var dataValue = new DataValue<T>(mNode, name, properties, value); // value is not copied!
		mBuffer.Add(dataValue);

		// raise 'Changed' event, if the added value is regular
		if ((properties & DataValuePropertiesInternal.Dummy) == 0)
		{
			if (EventManager<DataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			{
				EventManager<DataValueCollectionChangedEventArgs>.FireEvent(
					this,
					ChangedEventName,
					this,
					new DataValueCollectionChangedEventArgs(CollectionChangedAction.Added, mNode, dataValue));
			}
		}

		// raise 'ViewerChanged' event
		if (EventManager<ViewerDataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataValueCollectionChangedEventArgs>.FireEvent(
				this,
				ViewerChangedEventName,
				this,
				new ViewerDataValueCollectionChangedEventArgs(CollectionChangedAction.Added, mNode, dataValue));
		}

		return dataValue;
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
			ClearUnsynced();
		}
	}

	/// <summary>
	/// Removes all regular values from the collection (for internal use only, not synchronized).
	/// </summary>
	private void ClearUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// remove values from the collection
		// (remove from last to first to avoid reorganizing the buffer unnecessarily)
		List<IUntypedDataInternal> unregisteredReferences = null;
		for (int i = mBuffer.Count - 1; i >= 0; i--)
		{
			IUntypedDataValueInternal dataValue = mBuffer[i];

			// skip value, if the value is dummy
			if (dataValue.IsDummyUnsynced)
				continue;

			// unregister data value references, if any
			// (does not remove the data value as it is regular...)
			IUntypedDataInternal[] references = mNode.DataTreeManager.UnregisterDataValueReferencesUnsynced(dataValue, true);
			if (references.Length > 0)
			{
				if (unregisteredReferences != null) unregisteredReferences.AddRange(references);
				else unregisteredReferences = [..references];
			}

			// remove value
			mBuffer.RemoveAt(i);

			// raise 'Changed' event
			if (EventManager<DataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			{
				EventManager<DataValueCollectionChangedEventArgs>.FireEvent(
					this,
					ChangedEventName,
					this,
					new DataValueCollectionChangedEventArgs(
						CollectionChangedAction.Removed,
						mNode,
						dataValue));
			}

			// raise 'ViewerChanged' event
			if (EventManager<ViewerDataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
			{
				EventManager<ViewerDataValueCollectionChangedEventArgs>.FireEvent(
					this,
					ViewerChangedEventName,
					this,
					new ViewerDataValueCollectionChangedEventArgs(
						CollectionChangedAction.Removed,
						mNode,
						dataValue));
			}

			// the removed value is detached from the tree now...
			dataValue.DetachFromDataTree();
		}

		// register previously removed data value references
		// (creates dummy values and binds them accordingly to the references)
		if (unregisteredReferences != null)
		{
			mNode.DataTreeManager.RegisterDataValueReferencesUnsynced([.. unregisteredReferences]);
		}
	}

	#endregion

	#region Contains(...)

	/// <summary>
	/// Checks whether the collection contains a data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to check for.</param>
	/// <returns>
	/// <c>true</c> if the specified data value exists;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	public bool Contains(string name)
	{
		lock (mNode.DataTreeManager.Sync)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
			foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (dataValue.NameUnsynced != name) continue;
				if (dataValue.IsDummyUnsynced) continue;
				return true;
			}
			return false;
		}
	}

	#endregion

	#region GetEnumerator()

	/// <summary>
	/// Returns an enumerator that can be used to iterate through the data value collection.
	/// </summary>
	/// <returns>An enumerator.</returns>
	/// <remarks>
	/// This method returns a thread-safe enumerator for the collection. The enumerator locks the entire tree when
	/// constructed and releases the lock when it gets disposed. To ensure the lock is released properly, call its
	/// <see cref="IDisposable.Dispose"/> method. Using a foreach statement calls the <see cref="IDisposable.Dispose"/>
	/// method automatically.
	/// </remarks>
	public IEnumerator<IUntypedDataValue> GetEnumerator()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return new MonitorSynchronizedEnumerator<IUntypedDataValue>(
				mBuffer.GetEnumerator(),
				mNode.DataTreeManager.Sync);
		}
	}

	/// <summary>
	/// Returns an enumerator that can be used to iterate through the data value collection.
	/// </summary>
	/// <returns>An enumerator.</returns>
	/// <remarks>
	/// This method returns a thread-safe enumerator for the collection. The enumerator locks the entire tree when
	/// constructed and releases the lock when it gets disposed. To ensure the lock is released properly, call its
	/// <see cref="IDisposable.Dispose"/> method. Using a foreach statement calls the <see cref="IDisposable.Dispose"/>
	/// method automatically.
	/// </remarks>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion

	#region GetByName(...)

	/// <summary>
	/// Gets the data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to get.</param>
	/// <returns>
	/// Requested data value;<br/>
	/// <c>null</c> if there is data value with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	public IUntypedDataValue GetByName(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name.AsSpan(), false);
		}
	}

	/// <summary>
	/// Gets the data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to get.</param>
	/// <typeparam name="T">Type of the data value's value (generic type argument of <see cref="DataValue{T}"/>).</typeparam>
	/// <returns>
	/// Requested data value;<br/>
	/// <c>null</c> if there is data value with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="DataTypeMismatchException">A data value with the expected name exists, but its type differs from <typeparamref name="T"/>.</exception>
	public DataValue<T> GetByName<T>(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			IUntypedDataValueInternal dataValue = GetByNameUnsynced(name.AsSpan(), false);
			if (dataValue.Type == typeof(T)) return (DataValue<T>)dataValue;
			string path = PathHelpers.AppendNameToPath(mNode.PathUnsynced, name.AsSpan());
			throw new DataTypeMismatchException($"Cannot get value at '{path}' due to a type mismatch (existing value type: {dataValue.Type.FullName}, trying to get type: {typeof(T).FullName}).");
		}
	}

	/// <summary>
	/// Gets the data value with the specified name (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the data value to get.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy nodes;<br/>
	/// <c>false</c> to consider regular nodes only.
	/// </param>
	/// <returns>
	/// Requested data value;<br/>
	/// <c>null</c> if there is no data value with the specified name.
	/// </returns>
	internal IUntypedDataValueInternal GetByNameUnsynced(ReadOnlySpan<char> name, bool considerDummies)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(name != null);

		if (considerDummies)
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
			foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (dataValue.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal)) return dataValue;
			}
		}
		else
		{
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
			foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				if (dataValue.IsDummyUnsynced) continue;
				if (dataValue.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal)) return dataValue;
			}
		}

		return null;
	}

	#endregion

	#region Remove(...) / RemoveAll(...)

	/// <summary>
	/// Removes the specified data value.
	/// </summary>
	/// <param name="dataValue">Data value to remove.</param>
	/// <return>
	/// <c>true</c> if the specified data value was removed;<br/>
	/// <c>false</c> if specified data value does not exist.
	/// </return>
	public bool Remove(IUntypedDataValue dataValue)
	{
		if (dataValue == null) throw new ArgumentNullException(nameof(dataValue));

		lock (mNode.DataTreeManager.Sync)
		{
			int index = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal other in mBuffer)
#else
			foreach (IUntypedDataValueInternal other in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				// consider regular values only
				if (other.IsDummyUnsynced) continue;
				if (ReferenceEquals(other, dataValue)) return RemoveByIndexInternalUnsynced(index);
				index++;
			}

			return false;
		}
	}

	/// <summary>
	/// Removes a data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to remove.</param>
	/// <return>
	/// <c>true</c> if the data value with the specified name was removed;<br/>
	/// <c>false</c> if the data value with the specified name does not exist.
	/// </return>
	public bool Remove(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			int index = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
			foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
			{
				// consider regular values only
				if (dataValue.IsDummyUnsynced) continue;
				if (dataValue.NameUnsynced == name) return RemoveByIndexInternalUnsynced(index);
				index++;
			}

			return false;
		}
	}

	/// <summary>
	/// Removes all data values that match the specified predicate.
	/// </summary>
	/// <param name="predicate">A predicate data values to remove must match.</param>
	/// <returns>Number of removed data values.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
	public int RemoveAll(Predicate<IUntypedDataValue> predicate)
	{
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		lock (mNode.DataTreeManager.Sync)
		{
			int count = 0;
			for (int i = mBuffer.Count - 1; i >= 0; i--)
			{
				IUntypedDataValueInternal dataValue = mBuffer[i];
				if (dataValue.IsDummyUnsynced) continue;
				if (!predicate(dataValue)) continue;
				RemoveByIndexInternalUnsynced(i);
				count++;
			}

			return count;
		}
	}

	/// <summary>
	/// Removes the specified data value (for internal use only, not synchronized).<br/>
	/// This method is able to remove dummy values.
	/// </summary>
	/// <param name="dataValue">Data value to remove.</param>
	/// <return>
	/// <c>true</c> if the specified data value was removed;<br/>
	/// <c>false</c> if specified data value does not exist.
	/// </return>
	internal bool RemoveUnsynced(IUntypedDataValueInternal dataValue)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert(dataValue != null);

		int index = 0;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach (IUntypedDataValueInternal other in mBuffer)
#else
		foreach (IUntypedDataValueInternal other in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			// consider regular and dummy values
			if (ReferenceEquals(other, dataValue)) return RemoveByIndexInternalUnsynced(index);
			index++;
		}

		return false;
	}

	/// <summary>
	/// Removes the data value at the specified index from the collection and raises the appropriate events
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="index">Index of the data value to remove.</param>
	/// <returns>
	/// <c>true</c> if the data value was successfully removed;<br/>
	/// otherwise <c>false</c>.
	/// </returns>
	private bool RemoveByIndexInternalUnsynced(int index)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		// abort if the index is negative
		if (index < 0)
			return false;

		IUntypedDataValueInternal dataValue = mBuffer[index];

		// unregister data value references, if any
		IUntypedDataInternal[] references = mNode.DataTreeManager.UnregisterDataValueReferencesUnsynced(dataValue, true);

		if (dataValue.IsDummyUnsynced)
		{
			// unregistering removed the data value as it is dummy
			// => nothing to do here...
			Debug.Assert(!mBuffer.Any(otherDataValue => ReferenceEquals(dataValue, otherDataValue)));
		}
		else
		{
			// unregistering did not remove the data value as it is regular
			// => remove data value from the collection and detach it from the data tree
			mBuffer.RemoveAt(index);
			dataValue.DetachFromDataTree();
		}

		// register previously removed data value references
		mNode.DataTreeManager.RegisterDataValueReferencesUnsynced(references);

		// raise 'Changed' event, if the removed data value was regular
		if (!dataValue.IsDummyUnsynced)
		{
			if (EventManager<DataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ChangedEventName))
			{
				EventManager<DataValueCollectionChangedEventArgs>.FireEvent(
					this,
					ChangedEventName,
					this,
					new DataValueCollectionChangedEventArgs(
						CollectionChangedAction.Removed,
						mNode,
						dataValue));
			}
		}

		// raise 'ViewerChanged' event
		if (EventManager<ViewerDataValueCollectionChangedEventArgs>.IsHandlerRegistered(this, ViewerChangedEventName))
		{
			EventManager<ViewerDataValueCollectionChangedEventArgs>.FireEvent(
				this,
				ViewerChangedEventName,
				this,
				new ViewerDataValueCollectionChangedEventArgs(
					CollectionChangedAction.Removed,
					mNode,
					dataValue));
		}

		return true;
	}

	#endregion

	#region ToArray()

	/// <summary>
	/// Gets all data values in the collection as an array.
	/// </summary>
	/// <returns>An array containing all data values that are currently in the collection.</returns>
	public IUntypedDataValue[] ToArray()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return [.. RegularValuesOnly];
		}
	}

	#endregion

	#region Internal Helpers

	/// <summary>
	/// Finds the first occurrence of a value with the specified name (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the value to find.</param>
	/// <returns>
	/// Value with the specified name;<br/>
	/// <c>null</c> if no value with the specified name exists in the collection.
	/// </returns>
	internal IUntypedDataValueInternal FindUnsynced(string name)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
		foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			if (dataValue.NameUnsynced == name)
				return dataValue;
		}

		return null;
	}

	/// <summary>
	/// Finds the first occurrence of a value with the specified name (for internal use only, not synchronized).
	/// </summary>
	/// <param name="name">Name of the value to find.</param>
	/// <returns>Value with the specified name, null if no value with the specified name exists in the collection.</returns>
	internal IUntypedDataValueInternal FindUnsynced(ReadOnlySpan<char> name)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
		foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			if (dataValue.NameUnsynced.AsSpan().Equals(name, StringComparison.Ordinal))
				return dataValue;
		}

		return null;
	}

	/// <summary>
	/// Reinitializes the path variable (for internal use only, not synchronized).
	/// </summary>
	internal void ReinitializePathsRecursivelyUnsynced()
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		mBuffer.ForEach(dataValue => dataValue.UpdatePath());
	}

	/// <summary>
	/// Updates the data tree manager of all data values.
	/// </summary>
	/// <param name="dataTreeManager">The data tree manager to use.</param>
	internal void UpdateDataTreeManagerRecursivelyUnsynced(DataTreeManager dataTreeManager)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
		foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			dataValue.UpdateDataTreeManager(dataTreeManager);
		}
	}

	/// <summary>
	/// Copies all data values from the current data value collection to the data value collection of the specified node
	/// (for internal use only, not synchronized).
	/// </summary>
	internal void CopyValuesUnsynced(DataNode node)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
		foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			dataValue.CopyValuesUnsynced(node);
		}
	}

	/// <summary>
	/// Changes properties of all data values in the collection (for internal use only, not synchronized).
	/// </summary>
	/// <param name="valuePropertiesToSet">Data value properties to set.</param>
	/// <param name="valuePropertiesToClear">Data value properties to clear.</param>
	/// <remarks>
	/// If a flag is specified both 'set' and 'cleared', it will be finally set.
	/// </remarks>
	internal void SetPropertiesRecursivelyUnsynced(
		DataValuePropertiesInternal valuePropertiesToSet,
		DataValuePropertiesInternal valuePropertiesToClear)
	{
		Debug.Assert(Monitor.IsEntered(mNode.DataTreeManager.Sync), "The tree synchronization object is not locked.");
		Debug.Assert((valuePropertiesToSet & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToSet:X}) are not supported.");
		Debug.Assert((valuePropertiesToClear & ~DataValuePropertiesInternal.All) == 0, $"The specified property flags (0x{valuePropertiesToClear:X}) are not supported.");

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461 || NET48
		foreach (IUntypedDataValueInternal dataValue in mBuffer)
#else
		foreach (IUntypedDataValueInternal dataValue in CollectionsMarshal.AsSpan(mBuffer))
#endif
		{
			dataValue.PropertiesUnsynced = (dataValue.PropertiesUnsynced & ~valuePropertiesToClear) | valuePropertiesToSet;
		}
	}

	#endregion
}
