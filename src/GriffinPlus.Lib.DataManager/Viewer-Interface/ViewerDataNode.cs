/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
//// The source code is licensed under the MIT license.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Accessor for the internal state of a <see cref="DataNode"/> (for viewers).
/// </summary>
/// <remarks>
/// This accessor can be used to gain access to dummy nodes and values in the data tree,
/// which is important for viewers to show the correct state of the data tree - with regular and dummy data.
/// Modifying operations do not affect dummy nodes as dummy nodes are managed by the data tree manager.
/// </remarks>
[DebuggerDisplay("Name: {" + nameof(Name) + "}, Properties: {" + nameof(Properties) + "}, Path: {" + nameof(Path) + "}")]
public sealed class ViewerDataNode
{
	/// <inheritdoc cref="DataNode.ViewerChanged"/>
	public event EventHandler<ViewerDataNodeChangedEventArgs> Changed
	{
		add => WrappedNode.ViewerChanged += value;
		remove => WrappedNode.ViewerChanged -= value;
	}

	/// <inheritdoc cref="DataNode.ViewerChangedAsync"/>
	public event EventHandler<ViewerDataNodeChangedEventArgs> ChangedAsync
	{
		add => WrappedNode.ViewerChangedAsync += value;
		remove => WrappedNode.ViewerChangedAsync -= value;
	}

	/// <summary>
	/// Initializes a new <see cref="ViewerDataNode"/> wrapping a <see cref="DataNode"/>.
	/// </summary>
	/// <param name="node">The <see cref="DataNode"/> to wrap.</param>
	internal ViewerDataNode(DataNode node)
	{
		WrappedNode = node;
		Children = new ViewerChildDataNodeCollection(node.Children);
		Values = new ViewerDataValueCollection(node.Values);
	}

	/// <summary>
	/// Gets the wrapped data node.
	/// </summary>
	public DataNode WrappedNode { get; }

	/// <inheritdoc cref="DataNode.DataTreeManager"/>
	public DataTreeManager DataTreeManager => WrappedNode.DataTreeManager;

	/// <inheritdoc cref="DataNode.Name"/>
	public string Name
	{
		get => WrappedNode.Name;
		set => WrappedNode.Name = value;
	}

	/// <inheritdoc cref="DataNode.Properties"/>
	public DataNodeProperties Properties
	{
		get => WrappedNode.Properties;
		set => WrappedNode.Properties = value;
	}

	/// <inheritdoc cref="DataNode.IsPersistent"/>
	public bool IsPersistent
	{
		get => WrappedNode.IsPersistent;
		set => WrappedNode.IsPersistent = value;
	}

	/// <inheritdoc cref="DataNode.ViewerIsDummy"/>
	public bool IsDummy => WrappedNode.ViewerIsDummy;

	/// <inheritdoc cref="DataNode.ViewerParent"/>
	public ViewerDataNode Parent => WrappedNode.ViewerParent;

	/// <inheritdoc cref="DataNode.Path"/>
	public string Path => WrappedNode.Path;

	/// <summary>
	/// Gets the collection of child nodes associated with the current node.
	/// </summary>
	public ViewerChildDataNodeCollection Children { get; }

	/// <summary>
	/// Gets collection of values associated with the current node.
	/// </summary>
	public ViewerDataValueCollection Values { get; }

	/// <inheritdoc cref="DataNode.ViewerCopy(ViewerDataNode,bool)"/>
	public ViewerDataNode Copy(ViewerDataNode destinationNode, bool renameIfNecessary = false)
	{
		return WrappedNode.ViewerCopy(destinationNode, renameIfNecessary);
	}

	/// <inheritdoc cref="DataNode.ViewerExecuteAtomically(ViewerDataNodeAction)"/>
	public void ExecuteAtomically(ViewerDataNodeAction action)
	{
		WrappedNode.ViewerExecuteAtomically(action);
	}

	/// <inheritdoc cref="DataNode.ViewerExecuteAtomically{TState}(ViewerDataNodeAction{TState},TState)"/>
	public void ExecuteAtomically<TState>(ViewerDataNodeAction<TState> action, TState state)
	{
		WrappedNode.ViewerExecuteAtomically(action, state);
	}
}
