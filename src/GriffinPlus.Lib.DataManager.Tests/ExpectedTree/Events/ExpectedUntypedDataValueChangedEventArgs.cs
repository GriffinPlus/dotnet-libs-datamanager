///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
/// <param name="dataValue">The expected value of the <see cref="UntypedDataValueChangedEventArgs.DataValue"/> property.</param>
/// <param name="snapshot">The expected value of the <see cref="UntypedDataValueChangedEventArgs.Snapshot"/> property.</param>
/// <param name="changeFlags">The expected value of the <see cref="UntypedDataValueChangedEventArgs.ChangedFlags"/> property.</param>
sealed class ExpectedUntypedDataValueChangedEventArgs(
	IUntypedDataValueInternal        dataValue,
	ExpectedUntypedDataValueSnapshot snapshot,
	DataValueChangedFlags            changeFlags)
{
	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedDataValueInternal DataValue { get; } = dataValue;

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="UntypedDataValueChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedUntypedDataValueSnapshot Snapshot { get; } = snapshot;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueChangedEventArgs.ChangedFlags"/> property.
	/// </summary>
	public DataValueChangedFlags ChangedFlags { get; } = changeFlags;
}
