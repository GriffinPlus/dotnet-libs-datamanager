///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Event arguments for events concerning changes to a data node.
/// </summary>
public sealed class DataNodeChangedEventArgs : DataManagerEventArgs
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="DataNodeChangedEventArgs"/> class
	/// (for internal use only, not synchronized).
	/// </summary>
	/// <param name="node">Data node that has changed its properties.</param>
	/// <param name="changeFlags">Flags indicating which properties have changed.</param>
	internal DataNodeChangedEventArgs(
		DataNode             node,
		DataNodeChangedFlags changeFlags)
	{
		Debug.Assert(Monitor.IsEntered(node.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		Node = node;
		Snapshot = new DataNodeSnapshot(node);
		ChangeFlags = changeFlags;
	}

	#endregion

	#region Node

	/// <summary>
	/// Gets the data node that has changed.<br/>
	/// The data node will always return the latest value and properties, while the <see cref="Snapshot"/>
	/// property provides a snapshot of the data node just after the change the event notifies about.
	/// </summary>
	public DataNode Node { get; }

	#endregion

	#region Snapshot

	/// <summary>
	/// Gets the snapshot of <see cref="DataNode"/> just after the data value has changed.
	/// </summary>
	public DataNodeSnapshot Snapshot { get; }

	#endregion

	#region ChangedFlags

	/// <summary>
	/// Gets the flags indicating what properties have changed.
	/// </summary>
	public DataNodeChangedFlags ChangeFlags { get; }

	#endregion

	#region Copying Event Arguments

	/// <summary>
	/// Returns a number of copies of the current instance.
	/// </summary>
	/// <param name="count">Number of copies to create.</param>
	/// <returns>
	/// Copies of the current instance
	/// (actually the current instance is returned, since the instance is immutable).
	/// </returns>
	public override DataManagerEventArgs[] Dupe(int count)
	{
		var copies = new DataManagerEventArgs[count];
		for (int i = 0; i < count; i++) copies[i] = this;
		return copies;
	}

	#endregion
}
