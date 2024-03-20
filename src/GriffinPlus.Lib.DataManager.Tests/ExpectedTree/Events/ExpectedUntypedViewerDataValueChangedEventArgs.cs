///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
sealed class ExpectedUntypedViewerDataValueChangedEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedUntypedViewerDataValueChangedEventArgs"/> class.
	/// </summary>
	/// <param name="dataValue">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.DataValue"/> property.</param>
	/// <param name="snapshot">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.Snapshot"/> property.</param>
	/// <param name="changeFlags">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.ChangedFlags"/> property.</param>
	public ExpectedUntypedViewerDataValueChangedEventArgs(
		IUntypedViewerDataValue                dataValue,
		ExpectedUntypedViewerDataValueSnapshot snapshot,
		ViewerDataValueChangedFlags            changeFlags)
	{
		DataValue = dataValue;
		Snapshot = snapshot;
		ChangedFlags = changeFlags;
	}

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedViewerDataValue DataValue { get; }

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="UntypedViewerDataValueChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedUntypedViewerDataValueSnapshot Snapshot { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.ChangedFlags"/> property.
	/// </summary>
	public ViewerDataValueChangedFlags ChangedFlags { get; }
}
