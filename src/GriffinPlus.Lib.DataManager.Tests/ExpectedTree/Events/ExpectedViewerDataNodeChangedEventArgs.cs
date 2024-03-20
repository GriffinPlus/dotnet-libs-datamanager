///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Expected event arguments for events concerning changes to a data node.
/// </summary>
sealed class ExpectedViewerDataNodeChangedEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedViewerDataNodeChangedEventArgs"/> class.
	/// </summary>
	/// <param name="node">The expected value of the <see cref="ViewerDataNodeChangedEventArgs.Node"/> property.</param>
	/// <param name="snapshot">The expected value of the snapshot provided by the <see cref="ViewerDataNodeChangedEventArgs.Snapshot"/> property.</param>
	/// <param name="changeFlags">Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.ChangeFlags"/> property.</param>
	internal ExpectedViewerDataNodeChangedEventArgs(
		ViewerDataNode                 node,
		ExpectedViewerDataNodeSnapshot snapshot,
		ViewerDataNodeChangedFlags     changeFlags)
	{
		Node = node;
		Snapshot = snapshot;
		ChangeFlags = changeFlags;
	}

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.Node"/> property.
	/// </summary>
	public ViewerDataNode Node { get; }

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="ViewerDataNodeChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedViewerDataNodeSnapshot Snapshot { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataNodeChangedEventArgs.ChangeFlags"/> property.
	/// </summary>
	public ViewerDataNodeChangedFlags ChangeFlags { get; }
}
