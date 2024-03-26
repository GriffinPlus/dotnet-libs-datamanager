///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Event arguments for events concerning adding or removing a data node to/from <see cref="ViewerChildDataNodeCollection"/>.
/// </summary>
public sealed class ViewerDataNodeCollectionChangedEventArgs : DataManagerEventArgs
{
	private readonly ViewerDataNode[] mChildNodes;

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataNodeCollectionChangedEventArgs"/> class (for an initial update).
	/// </summary>
	/// <param name="parentNode">Node the set of child nodes belongs to.</param>
	/// <param name="childNodes">The initial set of child nodes.</param>
	internal ViewerDataNodeCollectionChangedEventArgs(
		ViewerDataNode          parentNode,
		IReadOnlyList<DataNode> childNodes)
	{
		Action = CollectionChangedAction.InitialUpdate;
		ParentNode = parentNode;
		int count = childNodes.Count;
		mChildNodes = new ViewerDataNode[count];
		for (int i = 0; i < count; i++) mChildNodes[i] = childNodes[i].ViewerWrapper;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataNodeCollectionChangedEventArgs"/> class (for a collection change)
	/// </summary>
	/// <param name="action">Indicates whether <paramref name="childNode"/> was added to or removed from <paramref name="parentNode"/>.</param>
	/// <param name="parentNode">Node a child node was added to or removed from.</param>
	/// <param name="childNode">The added/removed child node.</param>
	internal ViewerDataNodeCollectionChangedEventArgs(
		CollectionChangedAction action,
		ViewerDataNode          parentNode,
		ViewerDataNode          childNode)
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
	public ViewerDataNode ParentNode { get; }

	/// <summary>
	/// Gets the added/removed child node if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/>.
	/// </summary>
	public ViewerDataNode ChildNode => mChildNodes.Length > 0 ? mChildNodes[0] : null;

	/// <summary>
	/// Gets the added/removed child node if <see cref="Action"/> is
	/// <see cref="CollectionChangedAction.Added"/> respectively
	/// <see cref="CollectionChangedAction.Removed"/> or the initial set of child nodes if
	/// <see cref="Action"/> is <see cref="CollectionChangedAction.InitialUpdate"/>.
	/// </summary>
	public IEnumerable<ViewerDataNode> ChildNodes => mChildNodes;
}
