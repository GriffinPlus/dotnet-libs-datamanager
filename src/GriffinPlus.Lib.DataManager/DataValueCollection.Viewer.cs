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
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace GriffinPlus.Lib.DataManager;

partial class DataValueCollection
{
	#region ViewerChanged / ViewerChangedAsync

	internal const string ViewerChangedEventName = "ViewerChanged";

	/// <summary>
	/// Occurs when a data value is added to or removed from the collection (considers regular and dummy values).<br/>
	/// The first invocation of the event handler notifies about the initial set of values in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will be scheduled using the <see cref="SynchronizationContext"/> of the registering thread, if available.<br/>
	/// If the registering thread does not have a synchronization context, event invocations will be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataValueCollectionChangedEventArgs> ViewerChanged
	{
		add
		{
			lock (mNode.DataTreeManager.Sync) // ensures that nothing can get in between getting the initial state and registering the event that keeps track of changes
			{
				EventManager<ViewerDataValueCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					SynchronizationContext.Current ?? mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataValueCollectionChangedEventArgs(mNode, mBuffer));
			}
		}

		remove => EventManager<ViewerDataValueCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	/// <summary>
	/// Occurs when a data value is added to or removed from the collection (considers regular and dummy values).<br/>
	/// The first invocation of the event handler notifies about the initial set of values in the collection.<br/>
	/// Subsequent invocations notify about changes to the collection.<br/>
	/// Event invocations will always be scheduled on the data tree manager thread.
	/// </summary>
	internal event EventHandler<ViewerDataValueCollectionChangedEventArgs> ViewerChangedAsync
	{
		add
		{
			lock (mNode.DataTreeManager.Sync)
			{
				EventManager<ViewerDataValueCollectionChangedEventArgs>.RegisterEventHandler(
					this,
					ViewerChangedEventName,
					value,
					mNode.DataTreeManager.Host.SynchronizationContext,
					true,
					true,
					this,
					new ViewerDataValueCollectionChangedEventArgs(mNode, mBuffer));
			}
		}

		remove => EventManager<ViewerDataValueCollectionChangedEventArgs>.UnregisterEventHandler(
			this,
			ViewerChangedEventName,
			value);
	}

	#endregion

	#region ViewerWrapper

	private ViewerDataValueCollection mViewerWrapper;

	/// <summary>
	/// Gets a <see cref="ViewerDataValueCollection"/> wrapping the current collection for a viewer.
	/// </summary>
	/// <returns>The wrapper collection.</returns>
	internal ViewerDataValueCollection ViewerWrapper
	{
		get
		{
			if (mViewerWrapper != null)
				return mViewerWrapper;

			lock (mNode.DataTreeManager.Sync)
			{
				mViewerWrapper ??= new ViewerDataValueCollection(this);
				return mViewerWrapper;
			}
		}
	}

	#endregion

	#region ViewerCount

	/// <summary>
	/// Gets the number of values in the collection (considers regular and dummy values).
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

	#region ViewerAddDynamically(...)

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
	internal IUntypedViewerDataValue ViewerAddDynamically(
		Type                type,
		string              name,
		DataValueProperties properties,
		object              value)
	{
		return AddDynamicallyInternal(type, name, properties, value).ViewerWrapper;
	}

	#endregion

	#region ViewerContains(...)

	/// <summary>
	/// Checks whether the collection contains a data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to check for.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy values;<br/>
	/// <c>false</c> to consider regular values only.
	/// </param>
	/// <returns>
	/// <c>true</c> if the specified data value exists;
	/// <br/>otherwise <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	internal bool ViewerContains(string name, bool considerDummies)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach (IUntypedDataValueInternal dataValue in mBuffer)
			{
				if (dataValue.NameUnsynced != name) continue;
				if (!considerDummies && dataValue.IsDummyUnsynced) continue;
				return true;
			}
			return false;
		}
	}

	#endregion

	#region ViewerGetEnumerator()

	/// <summary>
	/// Returns an enumerator that can be used to iterate through the collection (considers regular and dummy values).
	/// </summary>
	/// <returns>An enumerator.</returns>
	/// <remarks>
	/// This method returns a thread-safe enumerator for the collection. The enumerator locks the entire tree when
	/// constructed and releases the lock when it gets disposed. To ensure the lock is released properly, call its
	/// <see cref="IDisposable.Dispose"/> method. Using a foreach statement calls the <see cref="IDisposable.Dispose"/>
	/// method automatically.
	/// </remarks>
	internal IEnumerator<IUntypedViewerDataValue> ViewerGetEnumerator()
	{
		lock (mNode.DataTreeManager.Sync)
		{
			return new MonitorSynchronizedEnumerator<IUntypedViewerDataValue>(
				mBuffer.Select(x => x.ViewerWrapper).GetEnumerator(),
				mNode.DataTreeManager.Sync);
		}
	}

	#endregion

	#region ViewerGetByName(...)

	/// <summary>
	/// Gets the data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to get.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy values;<br/>
	/// <c>false</c> to consider regular values only.
	/// </param>
	/// <returns>
	/// Requested data value;<br/>
	/// <c>null</c> if there is data value with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	internal IUntypedViewerDataValue ViewerGetByName(string name, bool considerDummies)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			return GetByNameUnsynced(name.AsSpan(), considerDummies)?.ViewerWrapper;
		}
	}

	/// <summary>
	/// Gets the data value with the specified name.
	/// </summary>
	/// <param name="name">Name of the data value to get.</param>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy values;<br/>
	/// <c>false</c> to consider regular values only.
	/// </param>
	/// <returns>
	/// Requested data value;<br/>
	/// <c>null</c> if there is data value with the specified name.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
	/// <exception cref="DataTypeMismatchException">A data value with the expected name exists, but its type differs from <typeparamref name="T"/>.</exception>
	internal ViewerDataValue<T> ViewerGetByName<T>(string name, bool considerDummies)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));

		lock (mNode.DataTreeManager.Sync)
		{
			IUntypedDataValueInternal dataValue = GetByNameUnsynced(name.AsSpan(), considerDummies);
			if (dataValue.Type == typeof(T)) return (ViewerDataValue<T>)dataValue.ViewerWrapper;
			string path = PathHelpers.AppendNameToPath(mNode.PathUnsynced, name.AsSpan());
			throw new DataTypeMismatchException($"Cannot get value at '{path}' due to a type mismatch (existing value type: {dataValue.Type.FullName}, trying to get type: {typeof(T).FullName}).");
		}
	}

	#endregion

	#region ViewerRemove(...) / ViewerRemoveAll(...)

	/// <summary>
	/// Removes the specified data value.
	/// </summary>
	/// <param name="dataValue">Data value to remove.</param>
	/// <return>
	/// <c>true</c> if the specified data value was removed;<br/>
	/// <c>false</c> if specified data value does not exist.
	/// </return>
	internal bool ViewerRemove(IUntypedViewerDataValue dataValue)
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
				// consider regular nodes only
				if (other.IsDummyUnsynced) continue;
				if (ReferenceEquals(other.ViewerWrapper, dataValue)) return RemoveByIndexInternalUnsynced(index);
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
	internal int ViewerRemoveAll(Predicate<IUntypedViewerDataValue> predicate)
	{
		if (predicate == null) throw new ArgumentNullException(nameof(predicate));

		lock (mNode.DataTreeManager.Sync)
		{
			int count = 0;
			for (int i = mBuffer.Count - 1; i >= 0; i--)
			{
				IUntypedDataValueInternal dataValue = mBuffer[i];
				if (dataValue.IsDummyUnsynced) continue;
				if (!predicate(dataValue.ViewerWrapper)) continue;
				RemoveByIndexInternalUnsynced(i);
				count++;
			}

			return count;
		}
	}

	#endregion

	#region ViewerToArray()

	/// <summary>
	/// Gets all data values in the collection as an array.
	/// </summary>
	/// <param name="considerDummies">
	/// <c>true</c> to consider regular and dummy values;<br/>
	/// <c>false</c> to consider regular values only.
	/// </param>
	/// <returns>An array containing all child nodes that are currently in the collection.</returns>
	internal IUntypedViewerDataValue[] ViewerToArray(bool considerDummies)
	{
		lock (mNode.DataTreeManager.Sync)
		{
			if (considerDummies) return [.. mBuffer.Select(x => x.ViewerWrapper)];
			return [.. RegularValuesOnly.Select(x => x.ViewerWrapper)];
		}
	}

	#endregion
}
