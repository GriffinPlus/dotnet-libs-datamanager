///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using GriffinPlus.Lib.DataManager.Viewer;

namespace GriffinPlus.Lib.DataManager;

/// <summary>
/// Flags indicating which properties in a data value have changed (for internal use only).
/// </summary>
[Flags]
enum DataValueChangedFlagsInternal
{
	// --------------------------------------------------------------------------------------------------------------------
	// ATTENTION:
	// This enumeration uses the same encoding for the data value properties as the DataValueProperties enumeration.
	// Therefore, you must not change the encoding of these members!
	// --------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Nothing has changed.
	/// </summary>
	None = DataValuePropertiesInternal.None, // 0x00000000

	/// <summary>
	/// The <see cref="DataValue{T}.IsPersistent"/> property or the <see cref="IUntypedViewerDataValue.IsPersistent"/> has changed.
	/// </summary>
	IsPersistent = DataValuePropertiesInternal.Persistent, // 0x00000001

	/// <summary>
	/// The <see cref="IUntypedViewerDataValue.IsDummy"/> property has changed.
	/// </summary>
	IsDummy = DataValuePropertiesInternal.Dummy, // 0x00000100

	/// <summary>
	/// The <see cref="DataValue{T}.IsDetached"/> property or the <see cref="IUntypedDataValue.IsDetached"/> property has changed.
	/// </summary>
	IsDetached = DataValuePropertiesInternal.Detached, // 0x00000200

	/// <summary>
	/// The <see cref="DataValue{T}.Properties"/> property or the <see cref="IUntypedDataValue.Properties"/> property has changed.
	/// </summary>
	Properties = 0x00010000,

	/// <summary>
	/// The <see cref="DataValue{T}.Value"/> property or the <see cref="IUntypedDataValue.Value"/> property has changed.
	/// </summary>
	Value = 0x00020000,

	/// <summary>
	/// The <see cref="DataValue{T}.Timestamp"/> property or the <see cref="IUntypedDataValue.Timestamp"/> property has changed.
	/// </summary>
	Timestamp = 0x00040000,

	/// <summary>
	/// The <see cref="DataValue{T}.Name"/> property or the <see cref="IUntypedDataValue.Name"/> property has changed.
	/// </summary>
	Name = 0x00080000,

	/// <summary>
	/// The <see cref="DataValue{T}.Path"/> property or the <see cref="IUntypedDataValue.Path"/> property has changed.
	/// </summary>
	Path = 0x00100000,

	/// <summary>
	/// All user flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	AllUserFlags = IsPersistent | IsDetached | Properties | Value | Timestamp | Name | Path,

	/// <summary>
	/// All viewer flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	AllViewerFlags = AllUserFlags | IsDummy,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data value
	/// (is also set, if using data value references and the underlying data value changes entirely).
	/// </summary>
	InitialUpdate = 0x10000000
}
