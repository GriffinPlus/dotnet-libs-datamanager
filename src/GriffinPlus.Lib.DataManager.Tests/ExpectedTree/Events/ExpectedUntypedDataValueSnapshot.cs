///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An expected snapshot of an <see cref="IUntypedDataValue"/>.
/// </summary>
public readonly struct ExpectedUntypedDataValueSnapshot
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExpectedUntypedDataValueSnapshot"/> class.
	/// </summary>
	internal ExpectedUntypedDataValueSnapshot(
		DateTime                    timestamp,
		DataValuePropertiesInternal properties,
		object                      value)
	{
		mProperties = properties;
		Timestamp = timestamp;
		Value = value;
	}

	#region Timestamp

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueSnapshot.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; }

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueSnapshot.Properties"/> property.
	/// </summary>
	public DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueSnapshot.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueSnapshot.IsDetached"/> property.
	/// </summary>
	public bool IsDetached => (mProperties & DataValuePropertiesInternal.Detached) != 0;

	#endregion

	#region Value

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedDataValueSnapshot.Value"/> property.
	/// </summary>
	public object Value { get; }

	#endregion
}
