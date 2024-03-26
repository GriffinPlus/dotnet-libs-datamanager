///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
/// <param name="dataValue">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.DataValue"/> property.</param>
/// <param name="snapshot">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.Snapshot"/> property.</param>
/// <param name="changeFlags">The expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.ChangedFlags"/> property.</param>
sealed class ExpectedUntypedViewerDataValueChangedEventArgs(
	IUntypedViewerDataValue                dataValue,
	ExpectedUntypedViewerDataValueSnapshot snapshot,
	ViewerDataValueChangedFlags            changeFlags)
{
	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedViewerDataValue DataValue { get; } = dataValue;

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="UntypedViewerDataValueChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedUntypedViewerDataValueSnapshot Snapshot { get; } = snapshot;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueChangedEventArgs.ChangedFlags"/> property.
	/// </summary>
	public ViewerDataValueChangedFlags ChangedFlags { get; } = changeFlags;
}
