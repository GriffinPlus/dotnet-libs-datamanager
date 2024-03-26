///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Griffin+ common library suite (https://github.com/griffinplus/dotnet-libs-datamanager).
// The source code is licensed under the MIT license.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GriffinPlus.Lib.DataManager.Viewer;

/// <summary>
/// Flags indicating what property in a <see cref="ViewerDataValue{T}"/> or <see cref="IUntypedViewerDataValue"/> object has changed.
/// </summary>
[Flags]
public enum ViewerDataValueChangedFlags
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
	/// The <see cref="ViewerDataValue{T}.IsPersistent"/> property or the <see cref="IUntypedViewerDataValue.IsPersistent"/> property has changed.
	/// </summary>
	IsPersistent = DataValueChangedFlagsInternal.IsPersistent,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.IsDummy"/> property or the <see cref="IUntypedViewerDataValue.IsDummy"/> property has changed.
	/// </summary>
	IsDummy = DataValueChangedFlagsInternal.IsDummy,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.IsDetached"/> property or the <see cref="IUntypedViewerDataValue.IsDetached"/> property has changed.
	/// </summary>
	IsDetached = DataValueChangedFlagsInternal.IsDetached,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.Properties"/> property or the <see cref="IUntypedViewerDataValue.Properties"/> property has changed.
	/// </summary>
	Properties = DataValueChangedFlagsInternal.Properties,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.Value"/> property or the <see cref="IUntypedViewerDataValue.Value"/> property has changed.
	/// </summary>
	Value = DataValueChangedFlagsInternal.Value,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.Timestamp"/> property or the <see cref="IUntypedViewerDataValue.Timestamp"/> property has changed.
	/// </summary>
	Timestamp = DataValueChangedFlagsInternal.Timestamp,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.Name"/> property or the <see cref="IUntypedViewerDataValue.Name"/> property has changed.
	/// </summary>
	Name = DataValueChangedFlagsInternal.Name,

	/// <summary>
	/// The <see cref="ViewerDataValue{T}.Path"/> property or the <see cref="IUntypedViewerDataValue.Path"/> property has changed.
	/// </summary>
	Path = DataValueChangedFlagsInternal.Path,

	/// <summary>
	/// All flags (except <see cref="InitialUpdate"/>).
	/// </summary>
	All = DataValueChangedFlagsInternal.AllViewerFlags,

	/// <summary>
	/// This flag indicates that this is the first update to a monitored data value.
	/// </summary>
	InitialUpdate = DataValueChangedFlagsInternal.InitialUpdate
}
