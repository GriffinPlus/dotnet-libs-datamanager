///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Flags indicating which properties in a data value have changed.
/// </summary>
[Flags]
public enum DataValueChangedFlags
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data value properties as the DataValueProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = DataValueChangedFlagsInternal.None,

	/// <summary>
	/// The <see cref="DataValue{T}.IsPersistent"/> property or the <see cref="IUntypedDataValue.IsPersistent"/> property has changed.
	/// </summary>
	IsPersistent = DataValueChangedFlagsInternal.IsPersistent,

	/// <summary>
	/// The <see cref="DataValue{T}.IsDetached"/> property or the <see cref="IUntypedDataValue.IsDetached"/> property has changed.
	/// </summary>
	IsDetached = DataValueChangedFlagsInternal.IsDetached,

	/// <summary>
	/// The <see cref="DataValue{T}.Properties"/> property or the <see cref="IUntypedDataValue.Properties"/> property has changed.
	/// </summary>
	Properties = DataValueChangedFlagsInternal.Properties,

	/// <summary>
	/// The <see cref="DataValue{T}.Value"/> property or the <see cref="IUntypedDataValue.Value"/> property has changed.
	/// </summary>
	Value = DataValueChangedFlagsInternal.Value,

	/// <summary>
	/// The <see cref="DataValue{T}.Timestamp"/> property or the <see cref="IUntypedDataValue.Timestamp"/> property has changed.
	/// </summary>
	Timestamp = DataValueChangedFlagsInternal.Timestamp,

	/// <summary>
	/// All flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	All = IsPersistent | IsDetached | Properties | Value | Timestamp,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data value.
	/// </summary>
	InitialUpdate = 0x00010000
}
