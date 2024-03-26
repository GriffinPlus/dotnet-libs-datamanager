///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning changes to a data node.
/// </summary>
public sealed class ViewerDataNodeChangedEventArgs : DataManagerEventArgs
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataNodeChangedEventArgs"/> class
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="node">Data node that has changed its properties.</param>
	/// <param name="changeFlags">Flags indicating which properties have changed.</param>
	internal ViewerDataNodeChangedEventArgs(
		DataNode                   node,
		ViewerDataNodeChangedFlags changeFlags)
	{
		Debug.Assert(Monitor.IsEntered(node.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		Node = node.ViewerWrapper;
		Snapshot = new ViewerDataNodeSnapshot(node);
		ChangeFlags = changeFlags;
	}

	#endregion

	#region Node

	/// <summary>
	/// Gets the data node that has changed.<br/>
	/// The data node will always return the latest value and properties, while the <see cref="Snapshot"/>
	/// property provides a snapshot of the data node just after the change the event notifies about.
	/// </summary>
	public ViewerDataNode Node { get; }

	#endregion

	#region Snapshot

	/// <summary>
	/// Gets the snapshot of <see cref="Node"/> just after the data value has changed.
	/// </summary>
	public ViewerDataNodeSnapshot Snapshot { get; }

	#endregion

	#region ChangedFlags

	/// <summary>
	/// Gets the flags indicating what properties have changed.
	/// </summary>
	public ViewerDataNodeChangedFlags ChangeFlags { get; }

	#endregion
}
