///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Expected event arguments for events concerning changes to a data node.
/// </summary>
/// <param name="node">The expected value of the <see cref="ViewerDataNodeChangedEventArgs.Node"/> property.</param>
/// <param name="snapshot">The expected value of the snapshot provided by the <see cref="ViewerDataNodeChangedEventArgs.Snapshot"/> property.</param>
/// <param name="changeFlags">Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.ChangeFlags"/> property.</param>
sealed class ExpectedViewerDataNodeChangedEventArgs(
	ViewerDataNode                 node,
	ExpectedViewerDataNodeSnapshot snapshot,
	ViewerDataNodeChangedFlags     changeFlags)
{
	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.Node"/> property.
	/// </summary>
	public ViewerDataNode Node { get; } = node;

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="ViewerDataNodeChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedViewerDataNodeSnapshot Snapshot { get; } = snapshot;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.ChangeFlags"/> property.
	/// </summary>
	public ViewerDataNodeChangedFlags ChangeFlags { get; } = changeFlags;
}
