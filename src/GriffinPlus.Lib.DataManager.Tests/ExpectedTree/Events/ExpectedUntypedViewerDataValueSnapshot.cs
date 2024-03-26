///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// An expected snapshot of a <see cref="IUntypedViewerDataValue"/>.
/// </summary>
readonly struct ExpectedUntypedViewerDataValueSnapshot(
	DateTime                    timestamp,
	DataValuePropertiesInternal properties,
	object                      value)
{
	#region Timestamp

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.Timestamp"/> property.
	/// </summary>
	public DateTime Timestamp { get; } = timestamp;

	#endregion

	#region Properties (incl. Boolean Accessors)

	private readonly DataValuePropertiesInternal mProperties = properties;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.Properties"/> property.
	/// </summary>
	public DataValueProperties Properties => (DataValueProperties)(mProperties & DataValuePropertiesInternal.UserProperties);

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.IsPersistent"/> property.
	/// </summary>
	public bool IsPersistent => (mProperties & DataValuePropertiesInternal.Persistent) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.IsDummy"/> property.
	/// </summary>
	public bool IsDummy => (mProperties & DataValuePropertiesInternal.Dummy) != 0;

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.IsDetached"/> property.
	/// </summary>
	public bool IsDetached => (mProperties & DataValuePropertiesInternal.Detached) != 0;

	#endregion

	#region Value

	/// <summary>
	/// Gets the expected value of the <see cref="UntypedViewerDataValueSnapshot.Value"/> property.
	/// </summary>
	public object Value { get; } = value;

	#endregion
}
