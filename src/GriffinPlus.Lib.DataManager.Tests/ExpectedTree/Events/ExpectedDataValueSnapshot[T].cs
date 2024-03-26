///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// An expected snapshot of a <see cref="DataValue{T}"/>.
/// </summary>
/// <param name="timestamp">The expected timestamp of the data value.</param>
/// <param name="properties">The expected properties of the data value.</param>
/// <param name="value">The expected value of the data value.</param>
readonly struct ExpectedDataValueSnapshot<T>(
	DateTime                    timestamp,
	DataValuePropertiesInternal properties,
	T                           value)
{
	#region Timestamp

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueSnapshot{T}.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; } = timestamp;

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties = properties;

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueSnapshot{T}.Properties"/> property.
	/// </summary>
	public DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueSnapshot{T}.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueSnapshot{T}.IsDetached"/> property.
	/// </summary>
	public bool IsDetached => (mProperties & DataValuePropertiesInternal.Detached) != 0;

	#endregion

	#region Value

	/// <summary>
	/// Gets the expected value of the <see cref="DataValueSnapshot{T}.Value"/> property.
	/// </summary>
	public T Value { get; } = value;

	#endregion
}
