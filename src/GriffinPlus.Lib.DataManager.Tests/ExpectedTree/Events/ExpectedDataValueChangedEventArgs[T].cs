///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
/// <param name="dataValue">The expected value of the <see cref="DataValueChangedEventArgs{T}.DataValue"/> property.</param>
/// <param name="snapshot">The expected value of the <see cref="DataValueChangedEventArgs{T}.Snapshot"/> property.</param>
/// <param name="changeFlags">The expected value of the <see cref="DataValueChangedEventArgs{T}.ChangedFlags"/> property.</param>
sealed class ExpectedDataValueChangedEventArgs<T>(
	DataValue<T>                 dataValue,
	ExpectedDataValueSnapshot<T> snapshot,
	DataValueChangedFlags        changeFlags)
{
	/// <summary>
	/// Gets the expected value of the <see cref="DataValueChangedEventArgs{T}.DataValue"/> property.
	/// </summary>
	public DataValue<T> DataValue { get; } = dataValue;

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="DataValueChangedEventArgs{T}.Snapshot"/> property.
	/// </summary>
	public ExpectedDataValueSnapshot<T> Snapshot { get; } = snapshot;

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueChangedEventArgs{T}.ChangedFlags"/> property.
	/// </summary>
	public DataValueChangedFlags ChangedFlags { get; } = changeFlags;
}
