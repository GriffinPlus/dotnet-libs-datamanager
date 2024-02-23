///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Accessor for the internal state of <see cref="DataValueCollection"/> (for viewers).
/// </summary>
/// <remarks>
/// This accessor can be used to gain access to dummy values in the data tree,
/// which is important for viewers to show the correct state of the data tree - with regular and dummy data.
/// Modifying operations do not affect dummy nodes as dummy nodes are managed by the data tree manager.
/// </remarks>
public sealed class ViewerDataValueCollection : IEnumerable<IUntypedViewerDataValue>
{
	/// <summary>
	/// Initializes a new <see cref="ViewerDataValueCollection"/> wrapping a <see cref="DataValueCollection"/>.
	/// </summary>
	/// <param name="collection">The <see cref="DataValueCollection"/> to wrap.</param>
	internal ViewerDataValueCollection(DataValueCollection collection)
	{
		WrappedCollection = collection;
	}

	/// <inheritdoc cref="DataValueCollection.ViewerChanged"/>
	public event EventHandler<ViewerDataValueCollectionChangedEventArgs> Changed
	{
		add => WrappedCollection.ViewerChanged += value;
		remove => WrappedCollection.ViewerChanged -= value;
	}

	/// <inheritdoc cref="DataValueCollection.ViewerChangedAsync"/>
	public event EventHandler<ViewerDataValueCollectionChangedEventArgs> ChangedAsync
	{
		add => WrappedCollection.ViewerChangedAsync += value;
		remove => WrappedCollection.ViewerChangedAsync -= value;
	}

	/// <summary>
	/// Gets the wrapped collection.
	/// </summary>
	internal DataValueCollection WrappedCollection { get; }

	/// <inheritdoc cref="DataValueCollection.ViewerCount"/>
	public int Count => WrappedCollection.ViewerCount;

	/// <inheritdoc cref="DataValueCollection.Add{T}(string,DataValueProperties, T)"/>
	public IUntypedViewerDataValue Add<T>(
		string              name,
		DataValueProperties properties,
		T                   value)
	{
		return WrappedCollection.Add(name, properties, value).ViewerWrapper;
	}

	/// <inheritdoc cref="DataValueCollection.ViewerAddDynamically(Type,string,DataValueProperties,object)"/>
	public IUntypedViewerDataValue AddDynamically(
		Type                type,
		string              name,
		DataValueProperties properties,
		object              value)
	{
		return WrappedCollection.ViewerAddDynamically(type, name, properties, value);
	}

	/// <inheritdoc cref="DataValueCollection.Clear()"/>
	public void Clear()
	{
		WrappedCollection.Clear();
	}

	/// <inheritdoc cref="DataValueCollection.ViewerContains(string,bool)"/>
	public bool Contains(string name, bool considerDummies)
	{
		return WrappedCollection.ViewerContains(name, considerDummies);
	}

	/// <inheritdoc cref="DataValueCollection.ViewerGetByName(string,bool)"/>
	public IUntypedViewerDataValue GetByName(string name, bool considerDummies)
	{
		return WrappedCollection.ViewerGetByName(name, considerDummies);
	}

	/// <inheritdoc cref="DataValueCollection.ViewerGetEnumerator()"/>
	public IEnumerator<IUntypedViewerDataValue> GetEnumerator()
	{
		return WrappedCollection.ViewerGetEnumerator();
	}

	/// <inheritdoc cref="DataValueCollection.ViewerGetEnumerator()"/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return WrappedCollection.ViewerGetEnumerator();
	}

	/// <inheritdoc cref="DataValueCollection.Remove(IUntypedDataValue)"/>
	public bool Remove(IUntypedViewerDataValue value)
	{
		return WrappedCollection.ViewerRemove(value);
	}

	/// <inheritdoc cref="DataValueCollection.Remove(string)"/>
	public bool Remove(string name)
	{
		return WrappedCollection.Remove(name);
	}

	/// <inheritdoc cref="DataValueCollection.RemoveAll(Predicate{IUntypedDataValue})"/>
	public int RemoveAll(Predicate<IUntypedViewerDataValue> predicate)
	{
		return WrappedCollection.ViewerRemoveAll(predicate);
	}

	/// <inheritdoc cref="DataValueCollection.ViewerToArray(bool)"/>
	public IUntypedViewerDataValue[] ToArray(bool considerDummies)
	{
		return WrappedCollection.ViewerToArray(considerDummies);
	}
}
