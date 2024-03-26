///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning adding or removing a data node to/from a <see cref="ChildDataNodeCollection"/>.
/// </summary>
public sealed class DataNodeCollectionChangedEventArgs : DataManagerEventArgs
{
	private readonly DataNode[] mChildNodes;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataNodeCollectionChangedEventArgs"/> class (for an initial update).
	/// </summary>
	/// <param name="parentNode">Node the set of child nodes belongs to.</param>
	/// <param name="childNodes">The initial set of child nodes.</param>
	internal DataNodeCollectionChangedEventArgs(
		DataNode              parentNode,
		IEnumerable<DataNode> childNodes)
	{
		Action = CollectionChangedAction.InitialUpdate;
		ParentNode = parentNode;
		mChildNodes = [.. childNodes];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataNodeCollectionChangedEventArgs"/> class (for a collection change)
	/// </summary>
	/// <param name="action">Indicates whether <paramref name="childNode"/> was added to or removed from <paramref name="parentNode"/>.</param>
	/// <param name="parentNode">Node a child node was added to or removed from.</param>
	/// <param name="childNode">The added/removed child node.</param>
	internal DataNodeCollectionChangedEventArgs(
		CollectionChangedAction action,
		DataNode                parentNode,
		DataNode                childNode)
	{
		Action = action;
		ParentNode = parentNode;
		mChildNodes = [childNode];
	}

	/// <summary>
	/// Gets a value indicating the kind of change to the collection.
	/// </summary>
	public CollectionChangedAction Action { get; }

	/// <summary>
	/// Gets the node a child node was added to or removed from.
	/// </summary>
	public DataNode ParentNode { get; }

	/// <summary>
	/// Gets the added/removed child node if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/>.
	/// </summary>
	public DataNode ChildNode => mChildNodes.Length > 0 ? mChildNodes[0] : null;

	/// <summary>
	/// Gets the added/removed child node if <see cref="Action"/> is
	/// <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/> or the initial set of child nodes if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.InitialUpdate"/>.
	/// </summary>
	public IEnumerable<DataNode> ChildNodes => mChildNodes;
}
