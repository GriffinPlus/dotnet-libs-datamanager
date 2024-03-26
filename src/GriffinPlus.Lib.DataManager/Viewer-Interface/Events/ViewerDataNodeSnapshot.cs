///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using System.Threading;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// A snapshot of a <see cref="ViewerDataNode"/>.
/// </summary>
public readonly struct ViewerDataNodeSnapshot
{
	#region Construction

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataNodeSnapshot"/> class assuming nothing has changed
	/// (for internal use only, not synchronized).<br/>
	/// This constructor is used for the initial event notification informing the event recipient about the initial
	/// state of the data node.
	/// </summary>
	/// <param name="node">The data node.</param>
	internal ViewerDataNodeSnapshot(DataNode node)
	{
		Debug.Assert(Monitor.IsEntered(node.DataTreeManager.Sync), "The tree synchronization object is not locked.");

		Name = node.NameUnsynced;
		Path = node.PathUnsynced;
		mProperties = node.PropertiesUnsynced;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ViewerDataNodeSnapshot"/> class copying another instance.
	/// </summary>
	/// <param name="other">Event arguments to copy.</param>
	internal ViewerDataNodeSnapshot(ViewerDataNodeSnapshot other)
	{
		Name = other.Name;
		Path = other.Path;
		mProperties = other.mProperties;
	}

	#endregion

	#region Name

	/// <summary>
	/// Gets the snapshotted value of the <see cref="ViewerDataNode.Name"/> property.
	/// </summary>
	public string Name { get; }

	#endregion

	#region Path

	/// <summary>
	/// Gets the snapshotted value of the <see cref="ViewerDataNode.Path"/> property.
	/// </summary>
	public string Path { get; }

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataNodePropertiesInternal mProperties;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="ViewerDataNode.Properties"/> property.
	/// </summary>
	public DataNodeProperties Properties => (DataNodeProperties)(mProperties & DataNodePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the snapshotted value of the <see cref="ViewerDataNode.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataNodePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the snapshotted value of the <see cref="ViewerDataNode.IsDummy"/> property.
	/// </summary>
	public bool IsDummy => (mProperties & DataNodePropertiesInternal.Dummy) != 0;

	#endregion
}
