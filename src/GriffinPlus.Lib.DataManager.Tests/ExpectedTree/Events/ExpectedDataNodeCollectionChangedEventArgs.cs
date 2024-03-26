///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data node collection.
/// </summary>
/// <param name="action">The expected value of the <see cref="DataNodeCollectionChangedEventArgs.Action"/> property.</param>
/// <param name="parentNode">The expected value of the <see cref="DataNodeCollectionChangedEventArgs.ParentNode"/> property.</param>
/// <param name="childNodes">The expected value of the <see cref="DataNodeCollectionChangedEventArgs.ChildNodes"/> property.</param>
sealed class ExpectedDataNodeCollectionChangedEventArgs(
	CollectionChangedAction action,
	DataNode                parentNode,
	DataNode[]              childNodes)
{
	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeCollectionChangedEventArgs.Action"/> property.
	/// </summary>
	public CollectionChangedAction Action { get; } = action;

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeCollectionChangedEventArgs.ParentNode"/> property.
	/// </summary>
	public DataNode ParentNode { get; } = parentNode;

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeCollectionChangedEventArgs.ChildNode"/> property.
	/// </summary>
	public DataNode ChildNode => ChildNodes?.FirstOrDefault();

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeCollectionChangedEventArgs.ChildNodes"/> property.
	/// </summary>
	public DataNode[] ChildNodes { get; } = childNodes;
}
