///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a viewer data node collection.
/// </summary>
/// <param name="action">The expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.Action"/> property.</param>
/// <param name="parentNode">The expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.ParentNode"/> property.</param>
/// <param name="childNodes">The expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.ChildNodes"/> property.</param>
sealed class ExpectedViewerDataNodeCollectionChangedEventArgs(
	CollectionChangedAction action,
	ViewerDataNode          parentNode,
	ViewerDataNode[]        childNodes)
{
	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.Action"/> property.
	/// </summary>
	public CollectionChangedAction Action { get; } = action;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.ParentNode"/> property.
	/// </summary>
	public ViewerDataNode ParentNode { get; } = parentNode;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.ChildNode"/> property.
	/// </summary>
	public ViewerDataNode ChildNode => ChildNodes?.FirstOrDefault();

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeCollectionChangedEventArgs.ChildNodes"/> property.
	/// </summary>
	public ViewerDataNode[] ChildNodes { get; } = childNodes;
}
