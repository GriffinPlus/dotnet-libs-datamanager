///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Accessor for the internal state of a <see cref="ChildDataNodeCollection"/> (for viewers).
/// </summary>
/// <remarks>
/// This accessor can be used to gain access to dummy nodes and values in the data tree,
/// which is important for viewers to show the correct state of the data tree - with regular and dummy data.
/// Modifying operations do not affect dummy nodes as dummy nodes are managed by the data tree manager.
/// </remarks>
[DebuggerDisplay("Count: {" + nameof(Count) + "}")]
public sealed class ViewerChildDataNodeCollection : IEnumerable<ViewerDataNode>
{
	/// <inheritdoc cref="ChildDataNodeCollection.ViewerChanged"/>
	public event EventHandler<ViewerDataNodeCollectionChangedEventArgs> Changed
	{
		add => WrappedCollection.ViewerChanged += value;
		remove => WrappedCollection.ViewerChanged -= value;
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerChangedAsync"/>
	public event EventHandler<ViewerDataNodeCollectionChangedEventArgs> ChangedAsync
	{
		add => WrappedCollection.ViewerChangedAsync += value;
		remove => WrappedCollection.ViewerChangedAsync -= value;
	}

	/// <summary>
	/// Initializes a new <see cref="ViewerChildDataNodeCollection"/> wrapping a <see cref="ChildDataNodeCollection"/>.
	/// </summary>
	/// <param name="collection">The <see cref="ChildDataNodeCollection"/> to wrap.</param>
	internal ViewerChildDataNodeCollection(ChildDataNodeCollection collection)
	{
		WrappedCollection = collection;
	}

	/// <summary>
	/// Gets the wrapped collection.
	/// </summary>
	internal ChildDataNodeCollection WrappedCollection { get; }

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerCount"/>
	public int Count => WrappedCollection.ViewerCount;

	/// <inheritdoc cref="ChildDataNodeCollection.Add(string)"/>
	public ViewerDataNode Add(string name)
	{
		return WrappedCollection.Add(name).ViewerWrapper;
	}

	/// <inheritdoc cref="ChildDataNodeCollection.Add(string,DataNodeProperties)"/>
	public ViewerDataNode Add(string name, DataNodeProperties properties)
	{
		return WrappedCollection.Add(name, properties).ViewerWrapper;
	}

	/// <inheritdoc cref="ChildDataNodeCollection.Clear"/>
	public void Clear()
	{
		WrappedCollection.Clear();
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerContains(string,bool)"/>
	public bool Contains(string name, bool considerDummies)
	{
		return WrappedCollection.ViewerContains(name, considerDummies);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerGetByName(string,bool)"/>
	public ViewerDataNode GetByName(string name, bool considerDummies)
	{
		return WrappedCollection.ViewerGetByName(name, considerDummies);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerGetEnumerator()"/>
	public IEnumerator<ViewerDataNode> GetEnumerator()
	{
		return WrappedCollection.ViewerGetEnumerator();
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerGetEnumerator()"/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return WrappedCollection.ViewerGetEnumerator();
	}

	/// <inheritdoc cref="ChildDataNodeCollection.GetNewNodeName(string)"/>
	public string GetNewNodeName(string baseName)
	{
		return WrappedCollection.GetNewNodeName(baseName);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.Remove(DataNode)"/>
	public bool Remove(ViewerDataNode node)
	{
		return WrappedCollection.Remove(node.WrappedNode);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.Remove(string)"/>
	public bool Remove(string name)
	{
		return WrappedCollection.Remove(name);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerRemoveAll(Predicate{ViewerDataNode})"/>
	public int RemoveAll(Predicate<ViewerDataNode> predicate)
	{
		return WrappedCollection.ViewerRemoveAll(predicate);
	}

	/// <inheritdoc cref="ChildDataNodeCollection.ViewerToArray()"/>
	public ViewerDataNode[] ToArray()
	{
		return WrappedCollection.ViewerToArray();
	}
}
