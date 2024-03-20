///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
sealed class ExpectedViewerDataValueChangedEventArgs<T>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedViewerDataValueChangedEventArgs{T}"/> class.
	/// </summary>
	/// <param name="dataValue">The expected value of the <see cref="ViewerDataValueChangedEventArgs{T}.DataValue"/> property.</param>
	/// <param name="snapshot">The expected value of the <see cref="ViewerDataValueChangedEventArgs{T}.Snapshot"/> property.</param>
	/// <param name="changeFlags">The expected value of the <see cref="ViewerDataValueChangedEventArgs{T}.ChangedFlags"/> property.</param>
	public ExpectedViewerDataValueChangedEventArgs(
		ViewerDataValue<T>                 dataValue,
		ExpectedViewerDataValueSnapshot<T> snapshot,
		ViewerDataValueChangedFlags        changeFlags)
	{
		DataValue = dataValue;
		Snapshot = snapshot;
		ChangedFlags = changeFlags;
	}

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueChangedEventArgs{T}.DataValue"/> property.
	/// </summary>
	public ViewerDataValue<T> DataValue { get; }

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="ViewerDataValueChangedEventArgs{T}.Snapshot"/> property.
	/// </summary>
	public ExpectedViewerDataValueSnapshot<T> Snapshot { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueChangedEventArgs{T}.ChangedFlags"/> property.
	/// </summary>
	public ViewerDataValueChangedFlags ChangedFlags { get; }
}
