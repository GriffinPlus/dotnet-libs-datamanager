///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// An expected snapshot of a <see cref="ViewerDataValue{T}"/>.
/// </summary>
readonly struct ExpectedViewerDataValueSnapshot<T>(
	DateTime                    timestamp,
	DataValuePropertiesInternal properties,
	T                           value)
{
	#region Timestamp

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; } = timestamp;

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties = properties;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.Properties"/> property.
	/// </summary>
	public DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.IsDummy"/> property.
	/// </summary>
	public bool IsDummy => (mProperties & DataValuePropertiesInternal.Dummy) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.IsDetached"/> property.
	/// </summary>
	public bool IsDetached => (mProperties & DataValuePropertiesInternal.Detached) != 0;

	#endregion

	#region Value

	/// <summary>
	/// Gets the expected value of the <see cref="ViewerDataValueSnapshot{T}.Value"/> property.
	/// </summary>
	public T Value { get; } = value;

	#endregion
}
