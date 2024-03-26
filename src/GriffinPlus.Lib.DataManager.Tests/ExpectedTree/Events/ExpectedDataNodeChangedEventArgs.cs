///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data node.
/// </summary>
/// <param name="node">The expected value of the <see cref="DataNodeChangedEventArgs.Node"/> property.</param>
/// <param name="snapshot">The expected value of the snapshot provided by the <see cref="DataNodeChangedEventArgs.Snapshot"/> property.</param>
/// <param name="changeFlags">Gets the expected value of the <see cref="DataNodeChangedEventArgs.ChangeFlags"/> property.</param>
sealed class ExpectedDataNodeChangedEventArgs(
	DataNode                 node,
	ExpectedDataNodeSnapshot snapshot,
	DataNodeChangedFlags     changeFlags)
{
	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeChangedEventArgs.Node"/> property.
	/// </summary>
	public DataNode Node { get; } = node;

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="DataNodeChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedDataNodeSnapshot Snapshot { get; } = snapshot;

	/// <summary>
	/// Gets the expected value of the <see cref="DataNodeChangedEventArgs.ChangeFlags"/> property.
	/// </summary>
	public DataNodeChangedFlags ChangeFlags { get; } = changeFlags;
}
