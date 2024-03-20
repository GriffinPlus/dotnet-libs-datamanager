///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
sealed class ExpectedDataValueChangedEventArgs<T>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedDataValueChangedEventArgs{T}"/> class.
	/// </summary>
	/// <param name="dataValue">The expected value of the <see cref="DataValueChangedEventArgs{T}.DataValue"/> property.</param>
	/// <param name="snapshot">The expected value of the <see cref="DataValueChangedEventArgs{T}.Snapshot"/> property.</param>
	/// <param name="changeFlags">The expected value of the <see cref="DataValueChangedEventArgs{T}.ChangedFlags"/> property.</param>
	public ExpectedDataValueChangedEventArgs(
		DataValue<T>                 dataValue,
		ExpectedDataValueSnapshot<T> snapshot,
		DataValueChangedFlags        changeFlags)
	{
		DataValue = dataValue;
		Snapshot = snapshot;
		ChangedFlags = changeFlags;
	}

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueChangedEventArgs{T}.DataValue"/> property.
	/// </summary>
	public DataValue<T> DataValue { get; }

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="DataValueChangedEventArgs{T}.Snapshot"/> property.
	/// </summary>
	public ExpectedDataValueSnapshot<T> Snapshot { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueChangedEventArgs{T}.ChangedFlags"/> property.
	/// </summary>
	public DataValueChangedFlags ChangedFlags { get; }
}
