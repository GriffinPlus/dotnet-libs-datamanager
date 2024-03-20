///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Expected event arguments for events concerning changes to a data value.
/// </summary>
sealed class ExpectedUntypedDataValueChangedEventArgs
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedUntypedDataValueChangedEventArgs"/> class.
	/// </summary>
	/// <param name="dataValue">The expected value of the <see cref="UntypedDataValueChangedEventArgs.DataValue"/> property.</param>
	/// <param name="snapshot">The expected value of the <see cref="UntypedDataValueChangedEventArgs.Snapshot"/> property.</param>
	/// <param name="changeFlags">The expected value of the <see cref="UntypedDataValueChangedEventArgs.ChangedFlags"/> property.</param>
	public ExpectedUntypedDataValueChangedEventArgs(
		IUntypedDataValueInternal        dataValue,
		ExpectedUntypedDataValueSnapshot snapshot,
		DataValueChangedFlags            changeFlags)
	{
		DataValue = dataValue;
		Snapshot = snapshot;
		ChangedFlags = changeFlags;
	}

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueChangedEventArgs.DataValue"/> property.
	/// </summary>
	public IUntypedDataValueInternal DataValue { get; }

	/// <summary>
	/// Gets the expected value of the snapshot provided by the <see cref="UntypedDataValueChangedEventArgs.Snapshot"/> property.
	/// </summary>
	public ExpectedUntypedDataValueSnapshot Snapshot { get; }

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueChangedEventArgs.ChangedFlags"/> property.
	/// </summary>
	public DataValueChangedFlags ChangedFlags { get; }
}
