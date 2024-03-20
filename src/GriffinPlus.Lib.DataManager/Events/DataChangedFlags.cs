///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager)
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Flags indicating which properties in a <see cref="Data{T}"/> or <see cref="IUntypedData"/> instance have changed.
/// </summary>
[Flags]
public enum DataChangedFlags
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data value properties as the DataValueProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = DataValueChangedFlagsInternal.None, // 0x00000000

	/// <summary>
	/// The <see cref="Data{T}.IsPersistent"/> property or the <see cref="IUntypedData.IsPersistent"/> property has changed.
	/// </summary>
	IsPersistent = DataValueChangedFlagsInternal.IsPersistent, // 0x00000001

	/// <summary>
	/// The <see cref="Data{T}.Properties"/> property or the <see cref="IUntypedData.Properties"/> property has changed.
	/// </summary>
	Properties = DataValueChangedFlagsInternal.Properties, // 0x00010000

	/// <summary>
	/// The <see cref="Data{T}.Value"/> property or the <see cref="IUntypedData.Value"/> property has changed.
	/// </summary>
	Value = DataValueChangedFlagsInternal.Value, // 0x00020000

	/// <summary>
	/// The <see cref="Data{T}.Timestamp"/> property or the <see cref="IUntypedData.Timestamp"/> property has changed.
	/// </summary>
	Timestamp = DataValueChangedFlagsInternal.Timestamp, // 0x00040000

	// The following flags do not exist for Data<T> instances
	// Name = DataValueChangedFlagsInternal.Name, // 0x00080000
	// Path = DataValueChangedFlagsInternal.Path, // 0x00100000

	/// <summary>
	/// The <see cref="Data{T}.IsHealthy"/> property or the <see cref="IUntypedData.IsHealthy"/> property has changed.
	/// </summary>
	IsHealthy = 0x00200000,

	/// <summary>
	/// All flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	All = IsPersistent | Properties | Value | Timestamp | IsHealthy,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data value
	/// (is also set, if using data value references and the underlying data value changes entirely).
	/// </summary>
	InitialUpdate = DataValueChangedFlagsInternal.InitialUpdate
}
